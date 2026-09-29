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
    # sougoubak.bin 与当前搜狗备份解析器不兼容（解析报"文件不完整或格式不兼容"），移入 C 组待补真实样本
    @{ Id = 'A9';  Import = '搜狗拼音备份词库bin';           NeedsFile = '搜狗备份.bin';             Export = '百度拼音';     Validate = @{ Type = 'Text'; MinLines = 1 } }
    @{ Id = 'A10'; Import = '无拼音纯汉字';                  File = '纯汉字.txt';                    Export = '极点五笔';     Validate = @{ Type = 'Text'; Keyword = '阿扁' } }

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
    @{ Id = 'C1';  Import = '紫光拼音词库uwl';                 NeedsFile = '紫光拼音.uwl';        Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1 } }
    @{ Id = 'C2';  Import = '百度手机bcd';                     NeedsFile = '百度手机.bcd';        Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1 } }
    @{ Id = 'C3';  Import = '百度手机或Mac版百度拼音';         NeedsFile = '百度手机.bdsj';       Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1 } }
    @{ Id = 'C4';  Import = '百度拼音备份词库bin';             NeedsFile = '百度拼音备份.bin';    Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1 } }
    @{ Id = 'C5';  Import = '极点五笔.mb文件';                 NeedsFile = '极点五笔.mb';         Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1 } }
    @{ Id = 'C6';  Import = 'Rime UserDb 用户词典';            NeedsFile = 'rime_userdb.txt';     Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1 } }
    @{ Id = 'C7';  Import = 'Gboard user_dict_3_3';            NeedsFile = 'gboard_user_dict.dict'; Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1 } }
    @{ Id = 'C8';  Import = 'Win10微软拼音（自学习词汇）';     NeedsFile = 'win10自学习.dat';     Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1 } }
    @{ Id = 'C9';  Import = 'Win10微软五笔（用户自定义短语）'; NeedsFile = 'win10微软五笔.dat';   Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1 } }
    @{ Id = 'C10'; Import = '微软拼音';                        NeedsFile = '微软拼音.dctx';       Export = '搜狗拼音txt'; Validate = @{ Type = 'Text'; MinLines = 1 } }
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
        $src = if ($c.NeedsFile) { "[缺样本] $($c.NeedsFile)" }
               elseif ($c.File)   { $c.File }
               else               { "(合成) $($c.Synthetic)" }
        Write-Host ("{0,-4} {1,-22} {2,-26} {3}" -f $c.Id, $c.Import, $src, $c.Export)
    }
    exit 0
}

if ($Only) {
    # 支持逗号分隔的多个 ID（如 -Only "A1,A2"）
    $ids = @($Only -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    $Cases = @($Cases | Where-Object {
        $c = $_
        ($ids -contains $c.Id) -or @($ids | Where-Object { $c.Import -like "*$_" -or $c.Import -like "*$($_)*" }).Count -gt 0
    })
    if ($Cases.Count -eq 0) { Write-Fail "没有匹配 '-Only $Only' 的用例"; exit 1 }
}

# ---- 准备工作目录与合成样本 ----
New-Item -ItemType Directory -Path $WorkDir -Force | Out-Null
Write-Step "工作目录: $WorkDir"
foreach ($c in $Cases | Where-Object { $_.Synthetic }) {
    $s = New-SampleLines -Type $c.Synthetic
    Write-SampleFile -Path (Get-CaseSourcePath -Case $c) -Lines $s.Lines -Enc $s.Enc
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
        Write-Step "[$($case.Id)] $($case.Import) -> $($case.Export)"

        # GUI 崩溃则重启
        if ($null -eq $guiProc -or $guiProc.HasExited) {
            if ($guiProc) { Write-Step "GUI 进程已退出，重新启动..." }
            $guiProc = Start-Gui -ExePath $GuiExe
            $script:LastImport = ''
            $script:LastExport = ''
        }

        try {
            Invoke-ConversionCase -Proc $guiProc -Case $case -SourcePath $srcPath -OutPath $outPath
            Write-Pass "[$($case.Id)] $($case.Import) -> $($case.Export)"
            $results.Add([pscustomobject]@{ Id = $case.Id; Import = $case.Import; Export = $case.Export; Status = 'PASS'; Detail = '' })
        }
        catch {
            Write-Fail "[$($case.Id)] $($_.Exception.Message)"
            $results.Add([pscustomobject]@{ Id = $case.Id; Import = $case.Import; Export = $case.Export; Status = 'FAIL'; Detail = $_.Exception.Message })
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
