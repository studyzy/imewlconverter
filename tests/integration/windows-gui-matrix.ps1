# =============================================================================
# GUI 版集成测试 - 输入法格式转换矩阵
#
# 驱动 WinForms 图形界面版深蓝词库转换，循环执行多条"导入格式 -> 导出格式"
# 转换路径（错位导出：格式1词库导出为格式2，格式2导出为格式3，以此类推），
# 校验每个转换产物。
#
# 测试矩阵：
#   A 组：真实词库（src/ImeWlConverterCoreTest/Test/），10 条路径
#   B 组：脚本内合成的小型文本样本，15 条路径
#   C 组：缺少样本文件的二进制格式占位，样本放入 Test/ 目录后自动执行
#   D 组：工具窗口（词库合并 / 文件分割三种模式），驱动帮助菜单下的模态工具窗
#
# 用法（在交互桌面会话中运行）：
#   powershell -ExecutionPolicy Bypass -File tests\integration\windows-gui-matrix.ps1
#
# 选项：
#   -GuiExe <路径>    指定已构建的 GUI exe；默认 dotnet build 现场构建
#   -SkipBuild        跳过构建（需已构建或指定 -GuiExe）
#   -Only <名称>      只运行 ID 或名称匹配的用例（如 -Only A1）
#   -List             仅列出所有测试用例，不运行
#
# 退出码：
#   0  全部用例通过（SKIP 的 C 组用例不影响）
#   1  存在失败用例（汇总表中列出）
#
# 注意：脚本会操作真实鼠标键盘（UIA + Win32 消息），运行期间请勿动鼠标。
# =============================================================================

param(
    [string]$GuiExe = "",
    [switch]$SkipBuild,
    [string]$Only = "",
    [switch]$List
)

$ErrorActionPreference = 'Stop'

# 共享 UI 自动化辅助函数（Win32 + UIA）
. (Join-Path $PSScriptRoot 'lib/gui-automation.ps1')

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$TestDir  = Join-Path $RepoRoot 'src/ImeWlConverterCoreTest/Test'
$WorkDir  = Join-Path $env:TEMP ("ime-gui-matrix-" + [guid]::NewGuid().ToString('N').Substring(0, 8))
$GuiProj  = Join-Path $RepoRoot 'src/IME WL Converter Win/IME WL Converter Win.csproj'

# =============================================================================
# 合成样本生成（B 组）
# =============================================================================
$SampleWords = @(
    @{ W = '深蓝词库转换'; P = "shen'lan'ci'ku'zhuan'huan"; S = 'shen lan ci ku zhuan huan'; C = 'igca' }
    @{ W = '测试';         P = "ce'shi";                    S = 'ce shi';                    C = 'ga' }
    @{ W = '词库转换';     P = "ci'ku'zhuan'huan";          S = 'ci ku zhuan huan';          C = 'icth' }
)

function Write-SampleFile {
    param([string]$Path, [string[]]$Lines, [string]$Enc)
    switch ($Enc) {
        'gbk'     { [System.IO.File]::WriteAllLines($Path, $Lines, [System.Text.Encoding]::GetEncoding(936)) }
        'utf16le' { [System.IO.File]::WriteAllLines($Path, $Lines, (New-Object System.Text.UnicodeEncoding($false, $true))) }
        'utf8'    { [System.IO.File]::WriteAllLines($Path, $Lines, (New-Object System.Text.UTF8Encoding($false))) }
        default   { throw "未知编码: $Enc" }
    }
}

function New-SampleLines {
    # 各文本格式的样本行生成；返回 @{ Lines=...; Enc=... }
    param([string]$Type)
    $w0 = $SampleWords[0]; $w1 = $SampleWords[1]; $w2 = $SampleWords[2]
    switch ($Type) {
        'sgpy'      { return @{ Enc = 'gbk';     Lines = @("'$($w0.P) $($w0.W)", "'$($w1.P) $($w1.W)", "'$($w2.P) $($w2.W)") } }
        'qqpy'      { return @{ Enc = 'utf16le'; Lines = @("$($w0.P) $($w0.W) 1", "$($w1.P) $($w1.W) 2", "$($w2.P) $($w2.W) 3") } }
        'ggpy'      { return @{ Enc = 'gbk';     Lines = @("$($w0.W)`t1`t$($w0.S)", "$($w1.W)`t2`t$($w1.S)", "$($w2.W)`t3`t$($w2.S)") } }
        'wubi-space'{ return @{ Enc = 'utf16le'; Lines = @("$($w0.C) $($w0.W)", "$($w1.C) $($w1.W)", "$($w2.C) $($w2.W)") } } # qqwb/xywb/jd 共用
        'wubi-tab'  { return @{ Enc = 'utf16le'; Lines = @("$($w0.C)`t$($w0.W)", "$($w1.C)`t$($w1.W)", "$($w2.C)`t$($w2.W)") } } # wb86/wb98/wbnewage 共用
        'bdpy'      { return @{ Enc = 'utf16le'; Lines = @("$($w0.W)`t$($w0.P)`t1", "$($w1.W)`t$($w1.P)`t2", "$($w2.W)`t$($w2.P)`t3") } }
        'xlpy'      { return @{ Enc = 'gbk';     Lines = @("$($w0.P) $($w0.W)", "$($w1.P) $($w1.W)", "$($w2.P) $($w2.W)") } }
        'sxpy'      { return @{ Enc = 'utf16le'; Lines = @("$($w0.W)`t$($w0.P)`t1", "$($w1.W)`t$($w1.P)`t2", "$($w2.W)`t$($w2.P)`t3") } }
        'pyjj'      { return @{ Enc = 'utf16le'; Lines = @('深shen蓝lan词ci库ku转zhuan换huan', '测试', '词ci库ku转zhuan换huan') } }
        'bing'      { return @{ Enc = 'utf16le'; Lines = @("$($w0.W) $($w0.S)", "$($w1.W) $($w1.S)", "$($w2.W) $($w2.S)") } }
        'fit'       { return @{ Enc = 'utf8';    Lines = @("$($w0.P),$($w0.W)", "$($w1.P),$($w1.W)", "$($w2.P),$($w2.W)") } }
        'bdsj'      { return @{ Enc = 'utf16le'; Lines = @("$($w0.W)($($w0.P))", "$($w1.W)($($w1.P))", "$($w2.W)($($w2.P))") } } # 百度手机/Mac版文本词库
        default     { throw "未知样本类型: $Type" }
    }
}

# =============================================================================
# 测试用例表
#   Import/Export 为 GUI 下拉框显示名（[FormatPlugin] DisplayName）
#   Validate: Type=Text(Keyword/MinLines 可选) | Binary(MinSize 默认 64)
#   Synthetic: 合成样本类型；File: Test/ 目录下的真实文件名；NeedsFile: C 组占位文件名
# =============================================================================
$BExportRotation = @('搜狗拼音txt', 'QQ拼音', '无拼音纯汉字')

$Cases = @(
    # ---- A 组：真实词库错位转换 ----
    @{ Id = 'A1';  Import = '搜狗细胞词库scel';              File = '唐诗300首【官方推荐】.scel';    Export = 'QQ拼音';       Validate = @{ Type = 'Text'; MinLines = 100 } }
    @{ Id = 'A2';  Import = 'QQ拼音';                        File = 'QQPinyin.txt';                  Export = '谷歌拼音';     Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }
    @{ Id = 'A3';  Import = 'QQ拼音英文';                    File = 'QQPinyin_English.txt';          Export = '搜狗拼音txt';  Validate = @{ Type = 'Text'; Keyword = 'imewlconverter' } }
    @{ Id = 'A4';  Import = 'QQ分类词库qpyd';                File = '成语.qpyd';                     Export = 'Rime中州韵';   Validate = @{ Type = 'Text'; MinLines = 100 } }
    @{ Id = 'A5';  Import = 'QQ分类词库qcel';                File = '星际战甲.qcel';                 Export = '手心输入法';   Validate = @{ Type = 'Text'; MinLines = 100 } }
    @{ Id = 'A6';  Import = '百度分类词库bdict';             File = 'travel.bdict';                  Export = '无拼音纯汉字'; Validate = @{ Type = 'Text'; MinLines = 100 } }
    @{ Id = 'A6B'; Import = '百度分类词库bdict';             File = '百度官网.bdict';                Export = '搜狗拼音txt';  Validate = @{ Type = 'Text'; MinLines = 5 } }
    @{ Id = 'A7';  Import = '灵格斯ld2';                     File = 'i.ld2';                         Export = '微软拼音';     Validate = @{ Type = 'Binary'; MinSize = 64 } }
    @{ Id = 'A8';  Import = 'Rime中州韵';                    File = 'luna_pinyin_export.txt';        Export = 'Mac简体拼音';  Validate = @{ Type = 'Text'; Keyword = '阿扁' } }
    # 搜狗备份.bin 为搜狗输入法"导出词库"生成的真实备份（旧 sougoubak.bin 与解析器不兼容已弃用）
    @{ Id = 'A9';  Import = '搜狗拼音备份词库bin';           File = '搜狗备份.bin';                  Export = '百度拼音';     Validate = @{ Type = 'Text'; MinLines = 10 } }
    @{ Id = 'A10'; Import = '无拼音纯汉字';                  File = '纯汉字.txt';                    Export = '极点五笔';     Validate = @{ Type = 'Text'; Keyword = '阿扁' } }
    @{ Id = 'A6C'; Import = '极点五笔';                      File = '极点五笔_freeime_user.txt';     Export = '搜狗拼音txt';  Validate = @{ Type = 'Text'; MinLines = 20 } }

    # ---- B 组：合成文本样本 ----
    @{ Id = 'B1';  Import = '搜狗拼音txt';      Synthetic = 'sgpy';       Export = $BExportRotation[0];  Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }
    @{ Id = 'B2';  Import = 'QQ拼音';           Synthetic = 'qqpy';       Export = $BExportRotation[1];  Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }
    @{ Id = 'B3';  Import = '谷歌拼音';         Synthetic = 'ggpy';       Export = $BExportRotation[2];  Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }
    @{ Id = 'B4';  Import = 'QQ五笔';           Synthetic = 'wubi-space'; Export = $BExportRotation[0];  Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }
    @{ Id = 'B5';  Import = '五笔86版';         Synthetic = 'wubi-tab';   Export = $BExportRotation[1];  Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }
    @{ Id = 'B6';  Import = '五笔98版';         Synthetic = 'wubi-tab';   Export = $BExportRotation[2];  Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }
    @{ Id = 'B7';  Import = '五笔新世纪版';     Synthetic = 'wubi-tab';   Export = $BExportRotation[0];  Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }
    @{ Id = 'B8';  Import = '小鸭五笔';         Synthetic = 'wubi-space'; Export = $BExportRotation[1];  Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }
    @{ Id = 'B9';  Import = '极点五笔';         Synthetic = 'wubi-space'; Export = $BExportRotation[2];  Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }
    @{ Id = 'B10'; Import = '百度拼音';         Synthetic = 'bdpy';       Export = $BExportRotation[0];  Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }
    @{ Id = 'B11'; Import = '新浪拼音';         Synthetic = 'xlpy';       Export = $BExportRotation[1];  Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }
    @{ Id = 'B12'; Import = '手心输入法';       Synthetic = 'sxpy';       Export = $BExportRotation[2];  Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }
    @{ Id = 'B13'; Import = '拼音加加';         Synthetic = 'pyjj';       Export = $BExportRotation[0];  Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }
    @{ Id = 'B14'; Import = '必应输入法';       Synthetic = 'bing';       Export = $BExportRotation[1];  Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }
    @{ Id = 'B15'; Import = 'FIT输入法';        Synthetic = 'fit';        Export = $BExportRotation[2];  Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }

    # ---- C 组：待补样本占位（样本放入 src/ImeWlConverterCoreTest/Test/ 后自动执行） ----
    # 已补样本并启用：C2(记者必备.bcd) / C4(百度拼音备份.bin) / C6(rime userdb) / C8(自学习) / C9(微软五笔)
    @{ Id = 'C1';  Import = '紫光拼音词库uwl';                 File = '华宇紫光economics.uwl';    Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1000 } }
    @{ Id = 'C2';  Import = '百度手机bcd';                     File = '记者必备.bcd';             Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 500 } }
    # bdsj 是文本格式（行格式"词(pin|yin)"，UTF-16LE），可直接合成，无需下载
    @{ Id = 'C3';  Import = '百度手机或Mac版百度拼音';         Synthetic = 'bdsj';                Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; Keyword = '深蓝词库转换' } }
    @{ Id = 'C4';  Import = '百度拼音备份词库bin';             File = '百度拼音备份.bin';         Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1 } }
    @{ Id = 'C5';  Import = '极点五笔.mb文件';                 NeedsFile = '极点五笔.mb';         Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1 } }
    @{ Id = 'C6';  Import = 'Rime UserDb 用户词典';            File = 'rime_luna_pinyin_export.txt'; Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 10 } }
    # GBoard_dictionary.txt 是 Gboard 文本词典导出（shortcut\tword，# 注释头）；
    # gboardbin 的二进制 user_dict_3_3 (.dict) 样本仍缺，保留占位
    @{ Id = 'C7';  Import = 'Gboard';                           File = 'GBoard_dictionary.txt';    Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; Keyword = '曾毅' } }
    @{ Id = 'C7B'; Import = 'Gboard user_dict_3_3';             NeedsFile = 'gboard_user_dict.dict'; Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1 } }
    @{ Id = 'C8';  Import = 'Win10微软拼音（自学习词汇）';     File = 'Win10拼音自学习_ChsPinyinUDL.dat'; Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1 } }
    @{ Id = 'C9';  Import = 'Win10微软五笔（用户自定义短语）'; File = '微软五笔UserDefinedPhrase.dat';   Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1 } }
    @{ Id = 'C10'; Import = '微软拼音';                        NeedsFile = '微软拼音.dctx';       Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1 } }

    # ---- D 组：工具窗口（帮助菜单 -> 词库合并 / 文件分割） ----
    # Merge: 主词库+2 附加词库，勾选按编码排序，保存后校验合并内容
    @{ Id = 'D1'; Tool = 'Merge';  Validate = @{ Type = 'MergeContent' } }
    # Split: 按行数（5 行文件按 2 行分割 -> 3 片）
    @{ Id = 'D2'; Tool = 'SplitLine';   Max = 2;   Validate = @{ Type = 'SplitLine' } }
    # Split: 按字数（3 行 x 10 字，取字长度 8 -> 切断点必须对齐行尾，每片恰好一行）
    @{ Id = 'D3'; Tool = 'SplitLength'; Max = 108; Validate = @{ Type = 'SplitLength' } }
    # Split: 按大小（约 4KB 内容，1KB 缓冲 -> >=2 片且行序列完整，验证行对齐不丢行）
    @{ Id = 'D4'; Tool = 'SplitSize';   Max = 11;  Validate = @{ Type = 'SplitSize' } }
)

# ---- E 组：导出"自定义"格式（SelfDefiningExporter 配置写回路径） ----
# 曾有 Bug：配置窗体默认值确认后导出 0 条（导出侧 ParsePattern 写回接线丢失，issue #421），
# 此用例回归守护：默认配置确认后必须正常导出
$Cases += @(
    @{ Id = 'E1'; Import = '搜狗细胞词库scel'; File = '唐诗300首【官方推荐】.scel'; Export = '自定义'; Validate = @{ Type = 'Text'; MinLines = 100; Keyword = ',' } }
)

# =============================================================================
# 工具函数
# =============================================================================
function Read-TextAuto {
    # 自动检测编码读取文本产物（BOM / 严格 UTF-8 / GB18030 回退）
    param([string]$Path)
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    if ($bytes.Length -ge 2) {
        if ($bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) { return [System.Text.Encoding]::Unicode.GetString($bytes, 2, $bytes.Length - 2) }
        if ($bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF) { return [System.Text.Encoding]::BigEndianUnicode.GetString($bytes, 2, $bytes.Length - 2) }
        if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
            return (New-Object System.Text.UTF8Encoding($false)).GetString($bytes, 3, $bytes.Length - 3)
        }
    }
    try {
        $strict = New-Object System.Text.UTF8Encoding($false, $true)
        return $strict.GetString($bytes)
    }
    catch {
        return [System.Text.Encoding]::GetEncoding(936).GetString($bytes)
    }
}

function Test-CaseOutput {
    # 校验转换产物，返回 $null 表示通过，否则返回失败原因
    param([hashtable]$Case, [string]$OutPath)
    if (-not (Test-Path $OutPath)) { return "产物文件不存在" }
    $v = $Case.Validate
    if ($v.Type -eq 'Binary') {
        $minSize = if ($v.MinSize) { $v.MinSize } else { 64 }
        $len = (Get-Item $OutPath).Length
        if ($len -lt $minSize) { return "二进制产物过小: $len 字节 (要求 >= $minSize)" }
        return $null
    }
    # Text
    $text = Read-TextAuto -Path $OutPath
    $lines = @($text -split "`r?`n") | Where-Object { $_.Trim() -ne '' }
    if ($lines.Count -eq 0) { return "文本产物为空" }
    if ($v.MinLines -and $lines.Count -lt $v.MinLines) { return "词条行数不足: $($lines.Count) < $($v.MinLines)" }
    if ($v.Keyword -and -not $text.Contains($v.Keyword)) { return "产物中未找到关键词 '$($v.Keyword)'" }
    return $null
}

function Get-CaseSourcePath {
    param([hashtable]$Case)
    if ($Case.NeedsFile) { return (Join-Path $TestDir $Case.NeedsFile) }
    if ($Case.File)      { return (Join-Path $TestDir $Case.File) }
    return (Join-Path $WorkDir ("sample-" + $Case.Synthetic + ".txt"))
}

# =============================================================================
# D 组（工具窗口：词库合并 / 文件分割）辅助函数
# =============================================================================
function New-ToolSamples {
    # 生成 D 组样本文件（WorkDir 下）。
    # 合并样本用带 BOM 的 UTF-16LE：GetEncodingType 对"UTF-8 无 BOM + 少量中文"会误判为 GBK
    # （产品既有局限，老代码同样如此），带 BOM 则检测必准。
    # 分割样本为纯 ASCII（无歧义）。
    $utf8 = New-Object System.Text.UTF8Encoding($false)
    $uni = New-Object System.Text.UnicodeEncoding($false, $true)
    [System.IO.File]::WriteAllText((Join-Path $WorkDir 'merge-main.txt'), "a wo 我`r`nb ni 你`r`n", $uni)
    [System.IO.File]::WriteAllText((Join-Path $WorkDir 'merge-u1.txt'), "a ta 他`r`n", $uni)
    [System.IO.File]::WriteAllText((Join-Path $WorkDir 'merge-u2.txt'), "c ta2 他2`r`n", $uni)
    [System.IO.File]::WriteAllText((Join-Path $WorkDir 'split-lines.txt'), "l1`r`nl2`r`nl3`r`nl4`r`nl5`r`n", $utf8)
    [System.IO.File]::WriteAllText((Join-Path $WorkDir 'split-len.txt'), "aaaaaaaaaa`r`nbbbbbbbbbb`r`ncccccccccc`r`n", $utf8)
    [System.IO.File]::WriteAllText((Join-Path $WorkDir 'split-size.txt'), ("line-of-text`r`n" * 300), $utf8)
}

function Find-ToolWindow {
    # 按 PID + 标题查找工具窗口（模态 ShowDialog 窗口）。
    # 实测：进程有模态窗口时，对该窗口的 UIA 查询（含 FromHandle）全部超时，
    # 而 Win32 枚举一切正常 —— 因此工具窗口一律返回 Win32 句柄，用消息驱动。
    param([int]$ProcId, [string]$Title, [int]$TimeoutMs = 15000)
    $deadline = [DateTime]::UtcNow.AddMilliseconds($TimeoutMs)
    while ([DateTime]::UtcNow -lt $deadline) {
        $hwnd = [ImeE2E.Helper]::FindWindowByTitle($ProcId, $Title)
        if ($hwnd -ne [IntPtr]::Zero) { return $hwnd }
        Start-Sleep -Milliseconds 400
    }
    return [IntPtr]::Zero
}

function Get-ChildEditHandlesSorted {
    # 枚举窗口内全部 Edit 子窗口（WinForms 类名带 .NET 哈希后缀，需前缀匹配），
    # 按屏幕 Y 再 X 排序（Designer 布局行序稳定：靠上的输入框排前面）
    param([IntPtr]$Window)
    $edits = New-Object System.Collections.Generic.List[object]
    $cb = [ImeE2E.Native+EnumProc]{
        param($h, $l)
        $sb = New-Object System.Text.StringBuilder 256
        [ImeE2E.Native]::GetClassName($h, $sb, 256) | Out-Null
        if ($sb.ToString().StartsWith('WindowsForms10.EDIT', [System.StringComparison]::OrdinalIgnoreCase)) {
            $r = New-Object 'ImeE2E.Native+RECT'
            [ImeE2E.Native]::GetWindowRect($h, [ref]$r) | Out-Null
            $edits.Add([pscustomobject]@{ Hwnd = $h; Y = $r.Top; X = $r.Left })
        }
        return $true
    }
    [ImeE2E.Native]::EnumChildWindows($Window, $cb, [IntPtr]::Zero) | Out-Null
    return @($edits | Sort-Object Y, X | ForEach-Object { $_.Hwnd })
}

function Send-WindowText {
    # WM_SETTEXT 设置 Win32 控件文本（WinForms TextBox/NumericUpDown 内部 Edit 均适用）
    param([IntPtr]$Hwnd, [string]$Text)
    [ImeE2E.Native]::SendMessage($Hwnd, 0x000C, [IntPtr]::Zero, $Text) | Out-Null
}

function Click-ControlHwnd {
    # 真实鼠标点击控件中心（合 并/分 割 按钮的处理器会开模态框，
    # 跨进程 SendMessage BM_CLICK 会同步阻塞到模态框关闭，绝不能用）
    param([IntPtr]$Hwnd)
    $r = New-Object 'ImeE2E.Native+RECT'
    [ImeE2E.Native]::GetWindowRect($Hwnd, [ref]$r) | Out-Null
    $x = [int](($r.Left + $r.Right) / 2)
    $y = [int](($r.Top + $r.Bottom) / 2)
    [ImeE2E.Native]::SetCursorPos($x, $y) | Out-Null
    Start-Sleep -Milliseconds 200
    [ImeE2E.Native]::mouse_event(0x02, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 100
    [ImeE2E.Native]::mouse_event(0x04, 0, 0, 0, [UIntPtr]::Zero)
}

function Close-ToolWindow {
    # 用 WM_CLOSE 关闭可能残留的工具窗口（不存在时静默通过）
    param([int]$ProcId, [string]$Title)
    $hwnd = [ImeE2E.Helper]::FindWindowByTitle($ProcId, $Title)
    if ($hwnd -ne [IntPtr]::Zero) {
        [ImeE2E.Native]::SendMessage($hwnd, 0x0010, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null # WM_CLOSE
        Start-Sleep -Milliseconds 600
        Write-Step "已关闭工具窗口: $Title"
    }
}

function Invoke-MenuItemById {
    # 打开 帮助 菜单下的工具入口（词库合并/文件分割）。
    # 注意：WinForms MenuStrip 的 UIA 树里 ToolStripMenuItem 的 AutomationId 不可靠，
    # 且下拉子项只有在菜单展开后才出现在 UIA 树中（弹出菜单是独立顶层窗口）。
    # 因此流程：找菜单栏 → 按 Name 找"帮助" → 鼠标展开 → RootElement 下按 Name 找子项 → 点击。
    param([int]$ProcId, $Main, [string]$ItemText)

    $mainHwnd = [IntPtr]$Main.Current.NativeWindowHandle
    [ImeE2E.Native]::SetForegroundWindow($mainHwnd) | Out-Null
    Start-Sleep -Milliseconds 300

    # 1. 找菜单栏中的"帮助"顶层项
    $menuBarCond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::MenuBar)
    $menuBar = $Main.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $menuBarCond)
    if ($null -eq $menuBar) { throw "未找到菜单栏 (MenuBar)" }
    $nameCond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, '帮助')
    $topItem = $menuBar.FindFirst([System.Windows.Automation.TreeScope]::Children, $nameCond)
    if ($null -eq $topItem) { throw "未找到 帮助 顶层菜单" }

    # 2. 鼠标点击展开（菜单项 Invoke 实测会被吞，真实点击最可靠）
    $rect = $topItem.Current.BoundingRectangle
    if (-not $rect -or $rect.Width -le 0) { throw "帮助 菜单矩形无效" }
    [ImeE2E.Native]::SetCursorPos([int]($rect.X + $rect.Width / 2), [int]($rect.Y + $rect.Height / 2)) | Out-Null
    Start-Sleep -Milliseconds 200
    [ImeE2E.Native]::mouse_event(0x02, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 100
    [ImeE2E.Native]::mouse_event(0x04, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 700

    # 3. 在弹出菜单（RootElement 下，同进程）中按 Name 找子项，最多重试 2 次。
    #    注意：Invoke 抛"操作超时"通常代表模态框已被它触发弹出（GUI 线程阻塞了 UIA 调用），
    #    此时不要回退鼠标点击（屏幕焦点已变），交给后续按标题等待窗口出现。
    foreach ($attempt in 1..2) {
        $byPid = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::ProcessIdProperty, $ProcId)
        $byName = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::NameProperty, $ItemText)
        $item = Find-ElementByCondition -Parent ([System.Windows.Automation.AutomationElement]::RootElement) `
            -Condition (New-Object System.Windows.Automation.AndCondition($byPid, $byName)) -TimeoutMs 5000
        if ($null -ne $item) {
            try {
                ($item.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)).Invoke()
            }
            catch {
                Write-Step "菜单项 Invoke 超时（通常代表模态窗口已弹出），继续..."
            }
            return
        }
        Write-Step "第 $attempt 次未找到菜单项 '$ItemText'，重新展开菜单..."
        [ImeE2E.Native]::SetCursorPos([int]($rect.X + $rect.Width / 2), [int]($rect.Y + $rect.Height / 2)) | Out-Null
        Start-Sleep -Milliseconds 200
        [ImeE2E.Native]::mouse_event(0x02, 0, 0, 0, [UIntPtr]::Zero)
        Start-Sleep -Milliseconds 100
        [ImeE2E.Native]::mouse_event(0x04, 0, 0, 0, [UIntPtr]::Zero)
        Start-Sleep -Milliseconds 700
    }
    throw "未找到菜单项 '$ItemText'（帮助菜单已展开仍不可见）"
}

function Set-ValueById {
    # 向窗口内指定 AutomationId 的 TextBox 写值（ValuePattern）
    param($Window, [string]$Id, [string]$Value)
    $box = Find-ById -Parent $Window -AutomationId $Id
    if ($null -eq $box) { throw "未找到输入框 ($Id)" }
    ($box.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)).SetValue($Value)
}

function Set-NumericUpDown {
    # 设置 WinForms NumericUpDown 的值：文本承载在内部 UpDownEdit (Edit 控件)，
    # 用 ValuePattern 写入；焦点离开（点击分割按钮）时控件自行解析
    param($Window, [string]$Id, [int]$Value)
    $num = Find-ById -Parent $Window -AutomationId $Id
    if ($null -eq $num) { throw "未找到数值控件 ($Id)" }
    $editCond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Edit)
    $edit = $num.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $editCond)
    if ($null -eq $edit) { throw "未找到数值编辑框 ($Id 内部 Edit)" }
    ($edit.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)).SetValue("$Value")
    Write-Step "设置 $Id = $Value"
}

function Invoke-MergeCase {
    # D1 词库合并：主窗口菜单 -> 词库合并 -> 填主/附加词库 -> 勾排序 -> 合并 -> 保存 -> 校验
    param([System.Diagnostics.Process]$Proc, [string]$SavePath)

    $main = Find-MainWindow -ProcId $Proc.Id
    if ($null -eq $main) { throw "未找到 GUI 主窗口" }
    # $mainHwnd 供 Wait-SaveDialogHandled 经动态作用域引用（lib 既有约定）
    $mainHwnd = [IntPtr]$main.Current.NativeWindowHandle
    [ImeE2E.Native]::SetForegroundWindow($mainHwnd) | Out-Null
    Start-Sleep -Milliseconds 300

    Write-Step "打开 词库合并 窗口..."
    Invoke-MenuItemById -ProcId $Proc.Id -Main $main -ItemText '词库合并'
    $win = Find-ToolWindow -ProcId $Proc.Id -Title '词库合并'
    if ($win -eq [IntPtr]::Zero) { throw "未找到 词库合并 窗口" }
    [ImeE2E.Native]::SetForegroundWindow($win) | Out-Null
    Start-Sleep -Milliseconds 300

    # 两个输入框按 Y 排序：靠上的是主词库，靠下的是附加词库
    $edits = Get-ChildEditHandlesSorted -Window $win
    if ($edits.Count -lt 2) { throw "词库合并窗口 Edit 输入框数量异常: $($edits.Count)" }
    Send-WindowText -Hwnd $edits[0] -Text (Join-Path $WorkDir 'merge-main.txt')
    Send-WindowText -Hwnd $edits[1] -Text ((Join-Path $WorkDir 'merge-u1.txt') + ' | ' + (Join-Path $WorkDir 'merge-u2.txt'))

    # 勾选"合并后按编码重新排序"
    $chk = [ImeE2E.Helper]::FindChildByText($win, '合并后按编码重新排序')
    if ($chk -eq [IntPtr]::Zero) { throw "未找到排序勾选框" }
    [ImeE2E.Native]::SendMessage($chk, 0x00F1, [IntPtr]1, [IntPtr]::Zero) | Out-Null # BM_SETCHECK

    Write-Step "点击 合并..."
    $btn = [ImeE2E.Helper]::FindChildByText($win, '合 并')
    if ($btn -eq [IntPtr]::Zero) { $btn = [ImeE2E.Helper]::FindChildByText($win, '合并') }
    if ($btn -eq [IntPtr]::Zero) { throw "未找到合并按钮" }

    # 点击后 GUI 弹"是否保存"(YesNo) 模态框；同步 BM_CLICK 会阻塞到模态框关闭，
    # 必须真实鼠标点击 + 轮询弹窗
    $dialogUp = $false
    foreach ($attempt in 1..3) {
        [ImeE2E.Native]::SetForegroundWindow($win) | Out-Null
        Start-Sleep -Milliseconds 300
        Click-ControlHwnd -Hwnd $btn
        $deadline = [DateTime]::UtcNow.AddSeconds(25)
        while ([DateTime]::UtcNow -lt $deadline) {
            if ([ImeE2E.Helper]::FindWindowWithButton($Proc.Id, '是', $win) -ne [IntPtr]::Zero) { $dialogUp = $true; break }
            Start-Sleep -Milliseconds 500
        }
        if ($dialogUp) { break }
        Write-Step "第 $attempt 次点击后 25 秒无弹窗，重试..."
    }

    # "是否保存"(YesNo) -> 是 -> 另存为 -> 落盘（与转换用例同一对话框序列，复用处理器）
    Wait-SaveDialogHandled -ProcId $Proc.Id -SavePath $SavePath

    if (-not (Test-Path $SavePath)) { throw "合并产物未落盘: $SavePath" }
    $text = Read-TextAuto -Path $SavePath
    $lines = @($text -split "`r?`n") | Where-Object { $_.Trim() -ne '' }
    $expected = @('a wo 我 ta 他', 'b ni 你', 'c ta2 他2')   # 排序 + 按词去重合并后的预期
    if ($lines.Count -ne 3) { throw "合并结果行数不符: $($lines.Count) (期望 3)，实际[$($lines -join '; ')]" }
    for ($i = 0; $i -lt 3; $i++) {
        if ($lines[$i] -ne $expected[$i]) { throw "合并结果第 $($i+1) 行不符: '$($lines[$i])' (期望 '$($expected[$i])')" }
    }
}

function Invoke-SplitCase {
    # D2/D3/D4 文件分割：主窗口菜单 -> 文件分割 -> 填路径/设值 -> 分割 -> 确定提示框 -> 校验分片
    param([System.Diagnostics.Process]$Proc, [hashtable]$Case)

    $main = Find-MainWindow -ProcId $Proc.Id
    if ($null -eq $main) { throw "未找到 GUI 主窗口" }
    [ImeE2E.Native]::SetForegroundWindow([IntPtr]$main.Current.NativeWindowHandle) | Out-Null
    Start-Sleep -Milliseconds 300

    Write-Step "打开 文件分割 窗口..."
    Invoke-MenuItemById -ProcId $Proc.Id -Main $main -ItemText '文件分割'
    $win = Find-ToolWindow -ProcId $Proc.Id -Title '文件分割'
    if ($win -eq [IntPtr]::Zero) { throw "未找到 文件分割 窗口" }
    [ImeE2E.Native]::SetForegroundWindow($win) | Out-Null
    Start-Sleep -Milliseconds 300

    $sourceName = switch ($Case.Tool) {
        'SplitLine'   { 'split-lines.txt' }
        'SplitLength' { 'split-len.txt' }
        'SplitSize'   { 'split-size.txt' }
    }
    $numIndex = switch ($Case.Tool) {
        # Edits 按 Y 排序后：[0]=txbFilePath, [1]=行, [2]=KB, [3]=字
        'SplitLine'   { 1 }
        'SplitLength' { 3 }
        'SplitSize'   { 2 }
    }
    $radioText = switch ($Case.Tool) {
        'SplitLine'   { '按行数分割' }
        'SplitLength' { '按字数分割' }
        'SplitSize'   { '按文件大小分割' }
    }
    $sourcePath = Join-Path $WorkDir $sourceName

    $edits = Get-ChildEditHandlesSorted -Window $win
    if ($edits.Count -lt 4) { throw "文件分割窗口 Edit 输入框数量异常: $($edits.Count)" }
    Send-WindowText -Hwnd $edits[0] -Text $sourcePath
    Send-WindowText -Hwnd $edits[$numIndex] -Text "$($Case.Max)"

    # 切换分割方式（默认按行数；D3/D4 需点对应单选钮）
    if ($Case.Tool -ne 'SplitLine') {
        $radio = [ImeE2E.Helper]::FindChildByText($win, $radioText)
        if ($radio -eq [IntPtr]::Zero) { throw "未找到单选钮: $radioText" }
        Click-ControlHwnd -Hwnd $radio
        Start-Sleep -Milliseconds 300
    }

    Write-Step "点击 分割..."
    $btn = [ImeE2E.Helper]::FindChildByText($win, '分 割')
    if ($btn -eq [IntPtr]::Zero) { $btn = [ImeE2E.Helper]::FindChildByText($win, '分割') }
    if ($btn -eq [IntPtr]::Zero) { throw "未找到分割按钮" }

    # 点击后弹"恭喜你，文件分割完成!"(OK) 提示框，真实鼠标 + 轮询
    $dialogUp = $false
    foreach ($attempt in 1..3) {
        [ImeE2E.Native]::SetForegroundWindow($win) | Out-Null
        Start-Sleep -Milliseconds 300
        Click-ControlHwnd -Hwnd $btn
        $deadline = [DateTime]::UtcNow.AddSeconds(25)
        while ([DateTime]::UtcNow -lt $deadline) {
            if ([ImeE2E.Helper]::FindConfirmButton($Proc.Id, '确定', $win) -ne [IntPtr]::Zero) { $dialogUp = $true; break }
            Start-Sleep -Milliseconds 500
        }
        if ($dialogUp) { break }
        Write-Step "第 $attempt 次点击后 25 秒无弹窗，重试..."
    }
    if (-not $dialogUp) { throw "分割后未出现完成提示框" }
    if (-not (Wait-ConfigDialogHandled -ProcId $Proc.Id -MainHwnd $win)) {
        throw "分割完成提示框未被确认关闭"
    }

    # 校验分片（分片与源文件同目录：原名+两位序号+扩展名）
    $stem = [System.IO.Path]::GetFileNameWithoutExtension($sourceName)
    $ext = [System.IO.Path]::GetExtension($sourceName)
    $partPath = { param($i) Join-Path $WorkDir ("{0}{1:d2}{2}" -f $stem, $i, $ext) }

    switch ($Case.Tool) {
        'SplitLine' {
            if (-not (Test-Path (& $partPath 3))) { throw "分片数量不足（缺 03 片）" }
            if ((Get-Content (& $partPath 1) -Raw) -ne "l1`r`nl2`r`n") { throw "01 片内容不符" }
            if ((Get-Content (& $partPath 2) -Raw) -ne "l3`r`nl4`r`n") { throw "02 片内容不符" }
            if ((Get-Content (& $partPath 3) -Raw) -ne "l5`r`n") { throw "03 片内容不符" }
            if (Test-Path (& $partPath 4)) { throw "出现多余分片 04" }
        }
        'SplitLength' {
            # 行对齐修复验证：每片恰好一整行（不从行中间切断）
            $expected = @("aaaaaaaaaa`r`n", "bbbbbbbbbb`r`n", "cccccccccc`r`n")
            for ($i = 1; $i -le 3; $i++) {
                $p = & $partPath $i
                if (-not (Test-Path $p)) { throw "缺少分片 $i" }
                if ((Get-Content $p -Raw) -ne $expected[$i - 1]) { throw "第 $i 片内容不符（未对齐行尾?）: $(Get-Content $p -Raw)" }
            }
            if (Test-Path (& $partPath 4)) { throw "出现多余分片 04" }
        }
        'SplitSize' {
            # 按大小分割是字节级切割（产品既有行为）：分片边界可能落在 \r\n 中间，
            # 边界处部分行尾字节会被吸收（相邻行黏合），因此断言"内容字节等价"：
            # 拼接全部分片、剥掉所有行尾字符后必须与源内容一致（不丢任何词条内容）
            $i = 1; $raw = ''
            while (Test-Path (& $partPath $i)) {
                $raw += Read-TextAuto -Path (& $partPath $i)
                $i++
            }
            if (($i - 1) -lt 2) { throw "分片数量不足: $($i - 1) 片 (期望 >= 2)" }
            $strip = { param($s) $s -replace "`r", '' -replace "`n", '' }
            if ((& $strip $raw) -ne (& $strip ("line-of-text`r`n" * 300))) {
                throw "分片拼接后内容与源文件不符"
            }
        }
    }
}

function Start-Gui {
    param([string]$ExePath)
    Write-Step "启动 GUI: $ExePath"
    $proc = Start-Process $ExePath -PassThru
    $main = Find-MainWindow -ProcId $proc.Id
    if ($null -eq $main) { throw "未找到 GUI 主窗口" }
    Write-Step "主窗口: $($main.Current.Name)"
    [ImeE2E.Native]::SetForegroundWindow([IntPtr]$main.Current.NativeWindowHandle) | Out-Null
    Start-Sleep -Milliseconds 500
    return $proc
}

function Test-AnyDialogUp {
    # 转换点击后是否已有任何弹窗（保存询问框/错误日志/消息框）
    param([int]$ProcId, [IntPtr]$MainHwnd)
    if ([ImeE2E.Helper]::FindWindowWithButton($ProcId, '是', $MainHwnd) -ne [IntPtr]::Zero) { return $true }
    if ([ImeE2E.Native]::FindWindow($null, '错误日志') -ne [IntPtr]::Zero) { return $true }
    if ([ImeE2E.Helper]::FindWindowByPidAndClass($ProcId, '#32770', 500) -ne [IntPtr]::Zero) { return $true }
    return $false
}

function Invoke-ConversionCase {
    # 在 GUI 中执行一条转换路径；失败抛异常
    param([System.Diagnostics.Process]$Proc, [hashtable]$Case, [string]$SourcePath, [string]$OutPath)

    if (Test-Path $OutPath) { Remove-Item $OutPath -Force }

    # 上一用例保存对话框关闭后 GUI 主线程可能仍忙，先等待其空闲
    Start-Sleep -Milliseconds 1500

    $main = Find-MainWindow -ProcId $Proc.Id -TimeoutMs 15000
    if ($null -eq $main) { throw "未找到 GUI 主窗口" }
    $mainHwnd = [IntPtr]$main.Current.NativeWindowHandle
    [ImeE2E.Native]::SetForegroundWindow($mainHwnd) | Out-Null
    Start-Sleep -Milliseconds 300

    # 1. 填入源词库路径
    $txbPath = Find-ById -Parent $main -AutomationId 'txbWLPath'
    if ($null -eq $txbPath) { throw "未找到源词库路径输入框 (txbWLPath)" }
    ($txbPath.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)).SetValue($SourcePath)

    # 2. 选择导入格式（弹配置框则自动确认）
    if ($script:LastImport -ne $Case.Import) {
        Write-Step "选择导入格式: $($Case.Import)"
        $cbxFrom = Find-ById -Parent $main -AutomationId 'cbxFrom'
        if ($null -eq $cbxFrom) { throw "未找到导入格式下拉框 (cbxFrom)" }
        # UIA Select 可能因模态配置框阻塞而超时——超时同样说明选择已生效
        try { Select-ComboItemUia -ComboElement $cbxFrom -ItemText $Case.Import }
        catch { Write-Step "UIA 选择导入格式异常(超时通常代表模态框已弹出): $($_.Exception.Message)" }
        if (-not (Wait-ConfigDialogHandled -ProcId $Proc.Id -MainHwnd $mainHwnd)) { throw "导入配置对话框未被确认" }
        $script:LastImport = $Case.Import
        $main = Find-MainWindow -ProcId $Proc.Id
        if ($null -eq $main) { throw "选择导入格式后未找回主窗口" }
        $mainHwnd = [IntPtr]$main.Current.NativeWindowHandle
    }

    # 3. 选择导出格式（弹配置框则自动确认）
    if ($script:LastExport -ne $Case.Export) {
        Write-Step "选择导出格式: $($Case.Export)"
        $cbxTo = Find-ById -Parent $main -AutomationId 'cbxTo'
        if ($null -eq $cbxTo) { throw "未找到导出格式下拉框 (cbxTo)" }
        try { Select-ComboItemUia -ComboElement $cbxTo -ItemText $Case.Export }
        catch { Write-Step "UIA 选择导出格式异常(超时通常代表模态框已弹出): $($_.Exception.Message)" }
        if (-not (Wait-ConfigDialogHandled -ProcId $Proc.Id -MainHwnd $mainHwnd)) { throw "导出配置对话框未被确认" }
        $script:LastExport = $Case.Export
        $main = Find-MainWindow -ProcId $Proc.Id
        if ($null -eq $main) { throw "选择导出格式后未找回主窗口" }
    }

    # 4. 点击转换并处理保存对话框
    $btnConvert = Find-ById -Parent $main -AutomationId 'btnConvert'
    if ($null -eq $btnConvert) { throw "未找到转换按钮 (btnConvert)" }
    $btnRect = $btnConvert.Current.BoundingRectangle  # 提前缓存坐标, 防止元素失效
    Write-Step "点击转换..."
    # 直接鼠标点击（UIA Invoke 在此按钮上实测频繁超时，耗时且无额外收益）。
    # 实测首次点击常被窗口激活吞掉，最多尝试 3 次，每次点击后等待弹窗出现。
    $dialogUp = $false
    foreach ($attempt in 1..3) {
        [ImeE2E.Native]::SetForegroundWindow($mainHwnd) | Out-Null
        Start-Sleep -Milliseconds 300
        if ($btnRect -and $btnRect.Width -gt 0) {
            [ImeE2E.Native]::SetCursorPos([int]($btnRect.X + $btnRect.Width / 2), [int]($btnRect.Y + $btnRect.Height / 2)) | Out-Null
            Start-Sleep -Milliseconds 200
            [ImeE2E.Native]::mouse_event(0x02, 0, 0, 0, [UIntPtr]::Zero)
            Start-Sleep -Milliseconds 100
            [ImeE2E.Native]::mouse_event(0x04, 0, 0, 0, [UIntPtr]::Zero)
        }
        else { Invoke-Element -Element $btnConvert -Rect $btnRect }
        $clickDeadline = [DateTime]::UtcNow.AddSeconds(25)
        while ([DateTime]::UtcNow -lt $clickDeadline) {
            if (Test-AnyDialogUp -ProcId $Proc.Id -MainHwnd $mainHwnd) { $dialogUp = $true; break }
            Start-Sleep -Milliseconds 500
        }
        if ($dialogUp) { break }
        Write-Step "第 $attempt 次点击后 25 秒无弹窗，重试..."
    }

    Wait-SaveDialogHandled -ProcId $Proc.Id -SavePath $OutPath

    # 4.5 保存后可能弹出"保存成功"等提示框，模态框会阻塞 GUI 主线程导致
    #     后续 UIA 调用全部超时，必须确认关闭（无提示框时 8 秒等待直接通过）
    if (-not (Wait-ConfigDialogHandled -ProcId $Proc.Id -MainHwnd $mainHwnd)) {
        throw "保存提示框未被确认关闭"
    }

    # 5. 等待产物落盘并校验
    $deadline = [DateTime]::UtcNow.AddSeconds(30)
    while ([DateTime]::UtcNow -lt $deadline -and -not (Test-Path $OutPath)) {
        Start-Sleep -Milliseconds 300
    }
    $err = Test-CaseOutput -Case $Case -OutPath $OutPath
    if ($err) { throw $err }
}

# =============================================================================
# 主流程
# =============================================================================
if ($List) {
    Write-Host ("{0,-4} {1,-22} {2,-26} {3}" -f 'ID', '导入格式', '源/样本', '导出格式')
    foreach ($c in $Cases) {
        if ($c.Tool) {
            Write-Host ("{0,-4} {1,-22} {2,-26} {3}" -f $c.Id, "[工具] $($c.Tool)", "(Max=$($c.Max))", '')
            continue
        }
        $src = if ($c.NeedsFile) {
                   $p = Join-Path $TestDir $c.NeedsFile
                   $(if (Test-Path $p) { $c.NeedsFile } else { "[缺样本] $($c.NeedsFile)" })
               }
               elseif ($c.File)   { $c.File }
               else               { "(合成) $($c.Synthetic)" }
        Write-Host ("{0,-4} {1,-22} {2,-26} {3}" -f $c.Id, $c.Import, $src, $c.Export)
    }
    exit 0
}

if ($Only) {
    # 仅按 ID 精确匹配（支持逗号分隔，如 -Only "A1,A2"）。
    # 不做名称子串匹配：-like 不区分大小写，id "D2" 会误命中导入格式"灵格斯ld2"
    $ids = @($Only -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    $Cases = @($Cases | Where-Object { $ids -contains $_.Id })
    if ($Cases.Count -eq 0) { Write-Fail "没有匹配 '-Only $Only' 的用例"; exit 1 }
}

# ---- 准备工作目录与合成样本 ----
New-Item -ItemType Directory -Path $WorkDir -Force | Out-Null
Write-Step "工作目录: $WorkDir"
foreach ($c in $Cases | Where-Object { $_.Synthetic }) {
    $s = New-SampleLines -Type $c.Synthetic
    Write-SampleFile -Path (Get-CaseSourcePath -Case $c) -Lines $s.Lines -Enc $s.Enc
}
if (@($Cases | Where-Object { $_.Tool }).Count -gt 0) {
    New-ToolSamples
}

# ---- 构建 GUI ----
if (-not ($GuiExe -and (Test-Path $GuiExe)) -and -not $SkipBuild) {
    Write-Step "构建 GUI 版..."
    Push-Location $RepoRoot
    try {
        dotnet build $GuiProj -c Release --nologo -v q
        if ($LASTEXITCODE -ne 0) { throw "GUI 构建失败 (exit=$LASTEXITCODE)" }
    }
    finally { Pop-Location }
}
if (-not ($GuiExe -and (Test-Path $GuiExe))) {
    # 优先按 GUI 程序名定位（bin 目录可能混入依赖拷贝的其他 exe，不能按"最新"选）
    $GuiExe = Get-ChildItem (Join-Path $RepoRoot 'src/IME WL Converter Win/bin/Release') `
        -Recurse -Filter '深蓝词库转换.exe' |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty FullName
    if (-not $GuiExe) {
        $GuiExe = Get-ChildItem (Join-Path $RepoRoot 'src/IME WL Converter Win/bin/Release') `
            -Recurse -Filter '*.exe' |
            Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty FullName
    }
}
if (-not $GuiExe -or -not (Test-Path $GuiExe)) { throw "找不到 GUI 可执行文件" }

# ---- 循环执行用例 ----
$script:LastImport = ''
$script:LastExport = ''
$results = New-Object System.Collections.Generic.List[object]
$guiProc = $null

try {
    foreach ($case in $Cases) {
        $srcPath = Get-CaseSourcePath -Case $case
        $outPath = Join-Path $WorkDir ("out-" + $case.Id + ".dat")

        # C 组占位：样本文件不存在则 SKIP
        if ($case.NeedsFile -and -not (Test-Path $srcPath)) {
            Write-Step "[$($case.Id)] SKIP: 缺少样本文件 $srcPath"
            $results.Add([pscustomobject]@{ Id = $case.Id; Import = $case.Import; Export = $case.Export; Status = 'SKIP'; Detail = "缺样本: $($case.NeedsFile)" })
            continue
        }

        Write-Host ""
        $caseLabel = if ($case.Tool) { "[$($case.Id)] 工具-$($case.Tool)" } else { "[$($case.Id)] $($case.Import) -> $($case.Export)" }
        Write-Step $caseLabel

        # GUI 崩溃则重启
        if ($null -eq $guiProc -or $guiProc.HasExited) {
            if ($guiProc) { Write-Step "GUI 进程已退出，重新启动..." }
            $guiProc = Start-Gui -ExePath $GuiExe
            $script:LastImport = ''
            $script:LastExport = ''
        }

        try {
            if ($case.Tool) {
                # D 组：工具窗口（合并/分割），结束后确保模态窗口已关闭
                try {
                    if ($case.Tool -eq 'Merge') {
                        Invoke-MergeCase -Proc $guiProc -SavePath $outPath
                    }
                    else {
                        Invoke-SplitCase -Proc $guiProc -Case $case
                    }
                    Write-Pass $caseLabel
                    $results.Add([pscustomobject]@{ Id = $case.Id; Import = "[工具] $($case.Tool)"; Export = ''; Status = 'PASS'; Detail = '' })
                }
                finally {
                    Close-ToolWindow -ProcId $guiProc.Id -Title '词库合并'
                    Close-ToolWindow -ProcId $guiProc.Id -Title '文件分割'
                }
            }
            else {
                Invoke-ConversionCase -Proc $guiProc -Case $case -SourcePath $srcPath -OutPath $outPath
                Write-Pass $caseLabel
                $results.Add([pscustomobject]@{ Id = $case.Id; Import = $case.Import; Export = $case.Export; Status = 'PASS'; Detail = '' })
            }
        }
        catch {
            Write-Fail "[$($case.Id)] $($_.Exception.Message)"
            $results.Add([pscustomobject]@{ Id = $case.Id; Import = $(if ($case.Tool) { "[工具] $($case.Tool)" } else { $case.Import }); Export = $case.Export; Status = 'FAIL'; Detail = $_.Exception.Message })
            # 失败后 GUI 状态不可信，重启
            try {
                if ($guiProc -and -not $guiProc.HasExited) {
                    $guiProc.Kill()
                    Start-Sleep -Milliseconds 800
                }
            } catch { }
            $guiProc = $null
        }
    }
}
finally {
    if ($guiProc -and -not $guiProc.HasExited) {
        $guiProc.CloseMainWindow() | Out-Null
        Start-Sleep -Milliseconds 800
        if (-not $guiProc.HasExited) { $guiProc.Kill() }
    }
}

# ---- 汇总 ----
Write-Host ""
Write-Host "==================== 测试汇总 ===================="
foreach ($r in $results) {
    $color = switch ($r.Status) { 'PASS' { 'Green' } 'FAIL' { 'Red' } default { 'Yellow' } }
    Write-Host ("  [{0}] {1,-4} {2} -> {3} {4}" -f $r.Status, $r.Id, $r.Import, $r.Export, $r.Detail) -ForegroundColor $color
}
$pass = @($results | Where-Object Status -eq 'PASS').Count
$fail = @($results | Where-Object Status -eq 'FAIL').Count
$skip = @($results | Where-Object Status -eq 'SKIP').Count
Write-Host "--------------------------------------------------"
Write-Host ("  共 {0} 条: PASS {1} / FAIL {2} / SKIP {3}" -f $results.Count, $pass, $fail, $skip)

if ($skip -gt 0) {
    Write-Host ""
    Write-Host "待准备样本文件（放入 src/ImeWlConverterCoreTest/Test/ 后自动执行）:" -ForegroundColor Yellow
    foreach ($r in $results | Where-Object Status -eq 'SKIP') {
        Write-Host ("    {0}  (用例 {1})" -f $r.Detail.Replace('缺样本: ', ''), $r.Id) -ForegroundColor Yellow
    }
}

Remove-Item $WorkDir -Recurse -Force -ErrorAction SilentlyContinue

if ($fail -gt 0) { exit 1 }
exit 0
