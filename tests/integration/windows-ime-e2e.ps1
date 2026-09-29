# =============================================================================
# Win10 微软拼音"用户自定义短语"实机端到端测试 (GUI 版)
#
# 驱动 WinForms 图形界面版深蓝词库转换完成"无拼音纯汉字 -> Win10微软拼音
# 用户自定义短语 .dat"的完整转换，再把产物放进输入法实测候选：
#
#   1. 构建 GUI 版可执行文件
#   2. 启动 GUI，原生窗口消息 + UI Automation 操作主窗体：
#      填入源词库路径 -> 选择导入/导出格式 -> 确认"拼音配置设置" -> 点击"转 换"
#   3. 自动处理保存对话框，得到 ChsPinyinUDP.dat 并做结构校验
#   4. 备份并替换 %APPDATA%\Microsoft\InputMethod\Chs\ChsPinyinUDP.dat
#   5. 重启输入法进程 (ctfmon / ChsIME)
#   6. 弹出输入框，模拟键入测试词拼音，断言微软拼音候选窗出现该词
#      （兜底：提交首选候选后检查输入框内容）
#   7. 恢复原词库，关闭 GUI
#
# 用法（在交互桌面会话中运行）：
#   powershell -ExecutionPolicy Bypass -File tests\integration\windows-ime-e2e.ps1
#
# 选项：
#   -GuiExe <路径>    指定已构建的 GUI exe；默认 dotnet build 现场构建
#   -ConvertOnly      只构建 GUI 与生成输入文件，不运行 UI 自动化（安全模式）
#   -SkipRestore      调试用：结束后不恢复原词库文件
#   -RestartExplorer  候选未出现时的重试手段：连 explorer 一起重启
#
# 退出码：
#   0  全部通过（GUI 转换 + 输入法候选验证）
#   1  失败（转换失败 / 格式校验失败 / 输入法无法验证）
#   3  GUI 转换通过, 但输入法未加载到新词库 —— 微软拼音只在登录时读取
#      UDP 词典, 杀进程无效, 需注销重新登录后重跑
#
# 警告：完整测试会临时替换真实输入法的用户自定义短语文件（结束自动恢复），
#       仅建议在本机/自建测试机上运行，不要在 GitHub 托管 CI 中运行。
# =============================================================================
param(
    [string]$GuiExe = "",
    [switch]$ConvertOnly,
    [switch]$SkipRestore,
    [switch]$RestartExplorer
)

$ErrorActionPreference = 'Stop'

# ===== 测试常量 =====
# 测试词选长音节串，核心词库几乎不会产生竞争候选
$TestWord    = '词库转换测试'
$TestPinyin  = "ce'shi'ci'ku'zhuan'huan"
$InputFormat = '无拼音纯汉字'
$OutputFormat = 'Win10微软拼音（用户自定义短语）'
$UdpDir      = Join-Path $env:APPDATA 'Microsoft\InputMethod\Chs'
$UdpPath     = Join-Path $UdpDir 'ChsPinyinUDP.dat'
$BackupPath  = "$UdpPath.e2e-backup"
$RepoRoot    = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$WorkDir     = Join-Path $env:TEMP ("ime-e2e-" + [guid]::NewGuid().ToString('N').Substring(0, 8))
$GuiProj     = Join-Path $RepoRoot 'src/IME WL Converter Win/IME WL Converter Win.csproj'

# 共享 UI 自动化辅助函数（Win32 + UIA）由共享库提供
. (Join-Path $PSScriptRoot 'lib/gui-automation.ps1')


# =============================================================================
# 第 1 步：准备输入文件与 GUI 可执行文件
# =============================================================================
Write-Step "工作目录: $WorkDir"
New-Item -ItemType Directory -Path $WorkDir -Force | Out-Null

$InputFile = Join-Path $WorkDir 'input.txt'
$DatFile   = Join-Path $WorkDir 'ChsPinyinUDP.dat'
@('深蓝词库转换', '集成测试词库', $TestWord) | Set-Content -Path $InputFile -Encoding UTF8

if (-not ($GuiExe -and (Test-Path $GuiExe))) {
    Write-Step "构建 GUI 版..."
    Push-Location $RepoRoot
    try {
        dotnet build $GuiProj -c Release --nologo -v q
        if ($LASTEXITCODE -ne 0) { throw "GUI 构建失败 (exit=$LASTEXITCODE)" }
    }
    finally { Pop-Location }
    $GuiExe = Get-ChildItem (Join-Path $RepoRoot 'src/IME WL Converter Win/bin/Release') `
        -Recurse -Filter '*.exe' |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $GuiExe -or -not (Test-Path $GuiExe)) { throw "找不到 GUI 可执行文件" }
Write-Step "GUI: $GuiExe"

if ($ConvertOnly) {
    Write-Pass "ConvertOnly 模式结束：GUI 构建成功, 输入文件就绪 ($InputFile)"
    exit 0
}

# =============================================================================
# 第 2 步：启动 GUI 并完成转换
# =============================================================================
try {
    $guiProc = Start-Process $GuiExe -PassThru
    $main = Find-MainWindow -ProcId $guiProc.Id
    if ($null -eq $main) { throw "未找到 GUI 主窗口" }
    Write-Step "主窗口: $($main.Current.Name)"
    $mainHwnd = [IntPtr]$main.Current.NativeWindowHandle
    [ImeE2E.Native]::SetForegroundWindow($mainHwnd) | Out-Null
    Start-Sleep -Milliseconds 500

    $txbPath = Find-ById -Parent $main -AutomationId 'txbWLPath'
    if ($null -eq $txbPath) { throw "未找到源词库路径输入框 (txbWLPath)" }
    ($txbPath.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)).SetValue($InputFile)

    $cbxFrom = Find-ById -Parent $main -AutomationId 'cbxFrom'
    $cbxTo   = Find-ById -Parent $main -AutomationId 'cbxTo'
    if ($null -eq $cbxFrom -or $null -eq $cbxTo) { throw "未找到格式下拉框 (cbxFrom/cbxTo)" }
    Write-Step "选择导入格式: $InputFormat"
    Select-ComboItem -ComboElement $cbxFrom -ItemText $InputFormat

    # 选导出格式会弹出"拼音配置设置"模态框。注意：UIA Select() 可能因模态框
    # 阻塞而超时——超时同样说明选择已生效（对话框已弹出），不能当作失败。
    Write-Step "选择导出格式: $OutputFormat"
    try {
        Select-ComboItemUia -ComboElement $cbxTo -ItemText $OutputFormat
    }
    catch {
        Write-Step "UIA Select 异常(超时通常代表模态框已弹出): $($_.Exception.Message)"
    }
    if (-not (Wait-ConfigDialogHandled -ProcId $guiProc.Id -MainHwnd $mainHwnd)) {
        throw "配置对话框未被确认"
    }

    # 模态框处理后主窗口元素引用可能失效，重新获取
    $main = Find-MainWindow -ProcId $guiProc.Id
    if ($null -eq $main) { throw "确认配置后未找回主窗口" }

    $btnConvert = Find-ById -Parent $main -AutomationId 'btnConvert'
    if ($null -eq $btnConvert) { throw "未找到转换按钮 (btnConvert)" }
    $btnConvertRect = $btnConvert.Current.BoundingRectangle  # 提前缓存坐标, 防止元素失效
    Write-Step "点击转换..."
    Invoke-Element -Element $btnConvert -Rect $btnConvertRect

    # 保存对话框（合并导出模式：转换完成后弹出）
    Wait-SaveDialogHandled -ProcId $guiProc.Id -SavePath $DatFile
    Write-Step "已确认保存对话框"

    # 等待产物落盘
    $deadline = [DateTime]::UtcNow.AddSeconds(30)
    while ([DateTime]::UtcNow -lt $deadline -and -not (Test-Path $DatFile)) {
        Start-Sleep -Milliseconds 300
    }
    if (-not (Test-Path $DatFile) -or (Get-Item $DatFile).Length -lt 0x40) {
        throw "GUI 转换产物异常: $DatFile"
    }

    # 结构自检：魔数 + 头部字段
    $bytes = [System.IO.File]::ReadAllBytes($DatFile)
    $magic = [System.Text.Encoding]::ASCII.GetString($bytes, 0, 8)
    if ($magic -ne 'mschxudp') { throw "产物魔数错误: $magic" }
    $dataStart  = [BitConverter]::ToUInt32($bytes, 0x14)
    $fileSize   = [BitConverter]::ToUInt32($bytes, 0x18)
    $entryCount = [BitConverter]::ToUInt32($bytes, 0x1C)
    if ($fileSize -ne $bytes.Length) { throw "头部文件大小 ($fileSize) 与实际 ($($bytes.Length)) 不符" }
    if ($dataStart -ne (0x40 + 4 * $entryCount)) { throw "数据区起始字段异常: $dataStart" }
    Write-Pass "GUI 转换成功: $DatFile ($($bytes.Length) 字节, $entryCount 条词条)"
}
finally {
    if ($guiProc -and -not $guiProc.HasExited) {
        $guiProc.CloseMainWindow() | Out-Null
        Start-Sleep -Milliseconds 800
        if (-not $guiProc.HasExited) { $guiProc.Kill() }
    }
}

# =============================================================================
# 第 3 步：备份并替换 UDP 词库
# =============================================================================
$hadOriginal = Test-Path $UdpPath
try {
    if ($hadOriginal) {
        Copy-Item $UdpPath $BackupPath -Force
        Write-Step "已备份原词库 -> $BackupPath"
    }
    New-Item -ItemType Directory -Path $UdpDir -Force | Out-Null
    Copy-Item $DatFile $UdpPath -Force
    Write-Step "已替换 $UdpPath"

    # =============================================================================
    # 第 4 步：重启输入法进程使其重新加载词库
    # 注意：微软拼音通常只在登录时读取 UDP 词典，杀进程重启不保证生效；
    #       若候选验证失败请注销重新登录后再跑一次本脚本（先 -SkipRestore 保留词库）。
    # =============================================================================
    foreach ($proc in 'ctfmon', 'ChsIME', 'TextInputHost') {
        Get-Process -Name $proc -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    }
    if ($RestartExplorer) {
        Get-Process -Name explorer -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    }
    Start-Process ctfmon
    Start-Sleep -Seconds 5
    Write-Step "输入法进程已重启"

    # =============================================================================
    # 第 5 步：模拟键入拼音，断言候选窗出现测试词
    # =============================================================================
    $form = New-Object System.Windows.Forms.Form
    $form.Text = 'IME E2E'
    $form.TopMost = $true
    $form.Size = New-Object System.Drawing.Size(600, 200)
    $form.StartPosition = 'Manual'
    $form.Location = New-Object System.Drawing.Point(50, 50)
    $textbox = New-Object System.Windows.Forms.TextBox
    $textbox.Multiline = $true
    $textbox.Dock = 'Fill'
    $textbox.Font = New-Object System.Drawing.Font('Microsoft YaHei', 14)
    $form.Controls.Add($textbox)
    $form.Add_Shown({ $textbox.Focus() })
    $form.Show()
    $form.Activate()
    $textbox.Focus()
    [System.Windows.Forms.Application]::DoEvents()
    Start-Sleep -Milliseconds 500

    foreach ($ch in $TestPinyin.ToCharArray()) {
        [System.Windows.Forms.SendKeys]::SendWait([string]$ch)
        Start-Sleep -Milliseconds 40
    }
    Start-Sleep -Milliseconds 1200

    $candidateText = ''
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    foreach ($className in 'Microsoft.IME.UIManager.CandidateWindow.Host',
                           'Microsoft.IME.UIManager.CompositionWindow.Host') {
        $cond = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::ClassNameProperty, $className)
        $windows = $root.FindAll([System.Windows.Automation.TreeScope]::Children, $cond)
        foreach ($win in $windows) {
            $all = $win.FindAll([System.Windows.Automation.TreeScope]::Descendants,
                [System.Windows.Automation.Condition]::TrueCondition)
            foreach ($el in $all) {
                if (-not [string]::IsNullOrEmpty($el.Current.Name)) {
                    $candidateText += $el.Current.Name + ' '
                }
            }
        }
    }

    $passed = $false
    $detail = ''
    if ($candidateText.Contains($TestWord)) {
        $passed = $true
        $detail = "候选窗中出现测试词 (候选内容: $($candidateText.Trim()))"
    }
    else {
        # 空格上屏首选候选（微软拼音 Enter 是原样上屏拼音）
        [System.Windows.Forms.SendKeys]::SendWait(' ')
        Start-Sleep -Milliseconds 800
        if ($textbox.Text.Contains($TestWord)) {
            $passed = $true
            $detail = "测试词为首选候选，已上屏"
        }
        elseif ($textbox.Text.Trim() -eq $TestPinyin) {
            # 拼音原样上屏 = 输入法处于英文模式，切换后重试
            for ($modeTry = 1; $modeTry -le 2 -and -not $passed; $modeTry++) {
                if ($modeTry -eq 1) {
                    Write-Step "输入法处于英文模式, 发送 Shift 切换中英..."
                    [System.Windows.Forms.SendKeys]::SendWait('+')
                }
                else {
                    Write-Step "仍为英文, 发送 Win+Space 切换键盘..."
                    [ImeE2E.Native]::keybd_event(0x5B, 0, 0, [UIntPtr]::Zero)      # LWIN down
                    [ImeE2E.Native]::keybd_event(0x20, 0, 0, [UIntPtr]::Zero)      # SPACE down
                    [ImeE2E.Native]::keybd_event(0x20, 0, 2, [UIntPtr]::Zero)      # SPACE up
                    [ImeE2E.Native]::keybd_event(0x5B, 0, 2, [UIntPtr]::Zero)      # LWIN up
                }
                Start-Sleep -Milliseconds 1000
                $textbox.Clear()
                [System.Windows.Forms.Application]::DoEvents()
                $textbox.Focus()
                Start-Sleep -Milliseconds 500
                foreach ($ch in $TestPinyin.ToCharArray()) {
                    [System.Windows.Forms.SendKeys]::SendWait([string]$ch)
                    Start-Sleep -Milliseconds 40
                }
                Start-Sleep -Milliseconds 1200
                [System.Windows.Forms.SendKeys]::SendWait(' ')
                Start-Sleep -Milliseconds 800
                if ($textbox.Text.Contains($TestWord)) {
                    $passed = $true
                    $detail = "切换输入法后测试词上屏 (第 $modeTry 次切换)"
                }
            }
        }

        if (-not $passed) {
            $convertedText = $textbox.Text.Trim()
            if ($convertedText -ne '' -and $convertedText -ne $TestPinyin -and
                -not ($convertedText -match '^[a-z'']+$')) {
                # 拼音被转换成了其他中文 = 输入法正常但 UDP 词典未重载
                # （微软拼音只在登录时读取 ChsPinyinUDP.dat）
                $detail = "输入法工作正常, 但未加载到测试词 —— UDP 词典需要注销重新登录后才重载。" +
                    "系统转换结果: '$convertedText'。" +
                    "请执行: 1) 注销并重新登录  2) 重跑本脚本 (若原机器无 UDP 词库请先加 -SkipRestore)"
                Write-Fail $detail
                exit 3
            }
            $detail = "候选窗与上屏内容均未出现测试词。" +
                "候选窗内容: '$($candidateText.Trim())', 输入框: '$($textbox.Text)'" +
                "。已尝试 Shift/Win+Space 切换输入法；请检查系统是否已安装微软拼音，" +
                "或注销重新登录后检查 $UdpPath"
        }
    }

    if ($passed) { Write-Pass $detail; exit 0 }
    Write-Fail $detail; exit 1
}
finally {
    if (-not $SkipRestore) {
        Get-Process -Name ctfmon, ChsIME -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
        if ($hadOriginal -and (Test-Path $BackupPath)) {
            Copy-Item $BackupPath $UdpPath -Force
            Remove-Item $BackupPath -Force
            Write-Step "已恢复原词库"
        }
        elseif (Test-Path $UdpPath) {
            Remove-Item $UdpPath -Force
            Write-Step "已移除测试词库 (原机器无 UDP 词库)"
        }
        Start-Process ctfmon
        Write-Step "输入法进程已恢复"
    }
    else {
        Write-Step "SkipRestore: 保留测试词库于 $UdpPath (备份在 $BackupPath)"
    }
    Remove-Item $WorkDir -Recurse -Force -ErrorAction SilentlyContinue
}
