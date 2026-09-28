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

function Write-Step($msg) { Write-Host "[e2e] $msg" -ForegroundColor Cyan }
function Write-Pass($msg) { Write-Host "[e2e] PASS: $msg" -ForegroundColor Green }
function Write-Fail($msg) { Write-Host "[e2e] FAIL: $msg" -ForegroundColor Red }

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

# =============================================================================
# Win32 原生辅助（比 UIA 更可靠：不依赖 UI 线程响应，不受模态框影响）
# Native 提供窗口消息/枚举；Helper 基于它封装"按 PID+类名找窗口"与"按文本找子窗口"。
# 两个类必须在同一次 Add-Type 中编译（跨程序集引用不可用）。
# =============================================================================
if (-not ('ImeE2E.Native' -as [type])) {
    Add-Type -TypeDefinition @'
namespace ImeE2E
{
    public class Native
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern System.IntPtr SendMessage(System.IntPtr hWnd, int Msg, System.IntPtr wParam, System.IntPtr lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern System.IntPtr SendMessage(System.IntPtr hWnd, int Msg, System.IntPtr wParam, string lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern System.IntPtr SendMessage(System.IntPtr hWnd, int Msg, System.IntPtr wParam, System.Text.StringBuilder lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern System.IntPtr FindWindow(string lpClassName, string lpWindowName);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern System.IntPtr FindWindowEx(System.IntPtr hWndParent, System.IntPtr hWndChildAfter, string lpszClass, string lpszWindow);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern int GetWindowText(System.IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern int GetClassName(System.IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(System.IntPtr hWnd, out uint lpdwProcessId);
        public delegate bool EnumProc(System.IntPtr hWnd, System.IntPtr lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumProc lpEnumFunc, System.IntPtr lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool EnumChildWindows(System.IntPtr hWndParent, EnumProc lpEnumFunc, System.IntPtr lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern System.IntPtr GetAncestor(System.IntPtr hWnd, uint flags);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int GetDlgCtrlID(System.IntPtr hWnd);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(System.IntPtr hWnd);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, System.UIntPtr dwExtraInfo);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, System.UIntPtr dwExtraInfo);
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool GetWindowRect(System.IntPtr hWnd, out RECT rect);
    }

    public static class Helper
    {
        public static System.IntPtr FindWindowByPidAndClass(int procId, string className, int timeoutMs)
        {
            var deadline = System.DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (System.DateTime.UtcNow < deadline)
            {
                System.IntPtr found = System.IntPtr.Zero;
                var sb = new System.Text.StringBuilder(256);
                Native.EnumWindows(delegate(System.IntPtr h, System.IntPtr l)
                {
                    sb.Length = 0;
                    Native.GetClassName(h, sb, 256);
                    if (sb.ToString() != className) return true;
                    uint pid;
                    Native.GetWindowThreadProcessId(h, out pid);
                    if (pid == (uint)procId) { found = h; return false; }
                    return true;
                }, System.IntPtr.Zero);
                if (found != System.IntPtr.Zero) return found;
                System.Threading.Thread.Sleep(300);
            }
            return System.IntPtr.Zero;
        }

        public static System.IntPtr FindChildByText(System.IntPtr parent, string text)
        {
            System.IntPtr found = System.IntPtr.Zero;
            var sb = new System.Text.StringBuilder(256);
            Native.EnumChildWindows(parent, delegate(System.IntPtr h, System.IntPtr l)
            {
                sb.Length = 0;
                // GetWindowText 跨进程取不到其他进程控件的文本，必须发 WM_GETTEXT
                Native.SendMessage(h, 0x0D, (System.IntPtr)sb.Capacity, sb);
                if (sb.ToString().Contains(text)) { found = h; return false; }
                return true;
            }, System.IntPtr.Zero);
            return found;
        }

        // 在指定进程（排除主窗口）的所有顶层窗口中找含指定按钮的模态框，
        // 返回按钮句柄。不依赖窗口标题，规避 FindWindow 按标题匹配失败的问题。
        public static System.IntPtr FindConfirmButton(int procId, string buttonText, System.IntPtr exclude)
        {
            System.IntPtr result = System.IntPtr.Zero;
            var sb = new System.Text.StringBuilder(256);
            Native.EnumWindows(delegate(System.IntPtr h, System.IntPtr l)
            {
                uint pid;
                Native.GetWindowThreadProcessId(h, out pid);
                if (pid != (uint)procId || h == exclude) return true;
                System.IntPtr btn = System.IntPtr.Zero;
                Native.EnumChildWindows(h, delegate(System.IntPtr c, System.IntPtr l2)
                {
                    sb.Length = 0;
                    // 跨进程控件文本必须用 WM_GETTEXT
                    Native.SendMessage(c, 0x0D, (System.IntPtr)sb.Capacity, sb);
                    if (sb.ToString() == buttonText) { btn = c; return false; }
                    return true;
                }, System.IntPtr.Zero);
                if (btn != System.IntPtr.Zero) { result = btn; return false; }
                return true;
            }, System.IntPtr.Zero);
            return result;
        }

        public static System.IntPtr FindChildByClass(System.IntPtr parent, string className)
        {
            System.IntPtr found = System.IntPtr.Zero;
            Native.EnumChildWindows(parent, delegate(System.IntPtr h, System.IntPtr l)
            {
                var sbC = new System.Text.StringBuilder(256);
                Native.GetClassName(h, sbC, 256);
                if (sbC.ToString() == className) { found = h; return false; }
                return true;
            }, System.IntPtr.Zero);
            return found;
        }

        public static string DescribeWindow(System.IntPtr h)
        {
            var sbT = new System.Text.StringBuilder(256);
            Native.SendMessage(h, 0x0D, (System.IntPtr)sbT.Capacity, sbT);
            var sbC = new System.Text.StringBuilder(256);
            Native.GetClassName(h, sbC, 256);
            var cls = sbC.ToString();
            if (cls.Length > 30) cls = cls.Substring(0, 30);
            return "[" + cls + "] '" + sbT.ToString() + "'";
        }

        public static string[] DescribeChildren(System.IntPtr parent)
        {
            var list = new System.Collections.Generic.List<string>();
            Native.EnumChildWindows(parent, delegate(System.IntPtr h, System.IntPtr l)
            {
                list.Add(DescribeWindow(h));
                return true;
            }, System.IntPtr.Zero);
            return list.ToArray();
        }
    }
}
'@
}

function Select-ComboItemNative {
    # CB_SHOWDROPDOWN 展开原生下拉列表 -> 按索引点击列表项（等效真实用户操作，
    # SelectedIndexChanged 必然自然触发）
    param($ComboElement, [string]$ItemText)
    $hwnd = [IntPtr]$ComboElement.Current.NativeWindowHandle
    if ($hwnd -eq [IntPtr]::Zero) { throw "无原生句柄" }

    $idx = [ImeE2E.Native]::SendMessage($hwnd, 0x158, [IntPtr](-1), $ItemText).ToInt32()
    if ($idx -lt 0) { throw "下拉框中找不到项: $ItemText" }

    [ImeE2E.Native]::SendMessage($hwnd, 0x14F, [IntPtr]1, [IntPtr]::Zero) | Out-Null # CB_SHOWDROPDOWN
    Start-Sleep -Milliseconds 500

    $hList = [ImeE2E.Native]::FindWindow('ComboLBox', $null)
    if ($hList -eq [IntPtr]::Zero) { throw "未找到展开的下拉列表 (ComboLBox)" }

    $itemH = [ImeE2E.Native]::SendMessage($hwnd, 0x154, [IntPtr]::Zero, [IntPtr]::Zero).ToInt32() # CB_GETITEMHEIGHT
    if ($itemH -le 0) { $itemH = 18 }
    $rect = New-Object 'ImeE2E.Native+RECT'
    [ImeE2E.Native]::GetWindowRect($hList, [ref]$rect) | Out-Null

    $x = [int](($rect.Left + $rect.Right) / 2)
    $y = [int]($rect.Top + $idx * $itemH + $itemH / 2)
    [ImeE2E.Native]::SetCursorPos($x, $y) | Out-Null
    Start-Sleep -Milliseconds 150
    [ImeE2E.Native]::mouse_event(0x02, 0, 0, 0, [UIntPtr]::Zero)  # LEFTDOWN
    Start-Sleep -Milliseconds 60
    [ImeE2E.Native]::mouse_event(0x04, 0, 0, 0, [UIntPtr]::Zero)  # LEFTUP
    Start-Sleep -Milliseconds 500
}

function Select-ComboItemUia {
    # 纯 UIA 选择；模态框阻塞时 Select() 可能抛 UIA 超时，由调用方决定是否视为成功
    param($ComboElement, [string]$ItemText)
    $expand = $ComboElement.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
    $expand.Expand()
    Start-Sleep -Milliseconds 600
    $nameCond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, $ItemText)
    $item = $ComboElement.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $nameCond)
    if ($null -eq $item) {
        $item = [System.Windows.Automation.AutomationElement]::RootElement.FindFirst(
            [System.Windows.Automation.TreeScope]::Descendants, $nameCond)
    }
    if ($null -eq $item) { throw "UIA 未找到列表项: $ItemText" }
    ($item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)).Select()
    Start-Sleep -Milliseconds 500
}

function Select-ComboItem {
    # 优先 UIA 选择（需已设置前台；实测可靠），失败再回退原生下拉+鼠标点击
    param($ComboElement, [string]$ItemText)
    try {
        Select-ComboItemUia -ComboElement $ComboElement -ItemText $ItemText
        return
    }
    catch {
        Write-Host "[e2e] UIA 选择失败, 回退原生下拉点击: $($_.Exception.Message)"
        try { ($ComboElement.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)).Collapse() } catch { }
    }

    # 回退：CB_SHOWDROPDOWN 展开原生列表 -> 按索引点击列表项
    Select-ComboItemNative -ComboElement $ComboElement -ItemText $ItemText
}

function Invoke-Element {
    # UIA Invoke，失败则按缓存的矩形真实点击按钮中心
    param($Element, $Rect)
    try {
        ($Element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)).Invoke()
        return
    }
    catch { Write-Host "[e2e] UIA Invoke 失败, 回退鼠标点击: $($_.Exception.Message)" }
    if ($Rect -eq $null -or $Rect.Width -le 0) {
        $Rect = $Element.Current.BoundingRectangle
    }
    if ($Rect -eq $null -or $Rect.Width -le 0) { throw "无法获取按钮位置" }
    $x = [int]($Rect.X + $Rect.Width / 2)
    $y = [int]($Rect.Y + $Rect.Height / 2)
    [ImeE2E.Native]::SetCursorPos($x, $y) | Out-Null
    Start-Sleep -Milliseconds 200
    [ImeE2E.Native]::mouse_event(0x02, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 100
    [ImeE2E.Native]::mouse_event(0x04, 0, 0, 0, [UIntPtr]::Zero)
}

function Wait-ConfigDialogHandled {
    # 处理"拼音配置设置"等模态配置框：
    #   1. 等待出现（部分格式无配置框，未出现直接通过）
    #   2. 自动点击"确定"（按钮矩形非零才点；本会话曾出现零矩形幻影句柄）
    #   3. 自动失败则提示人工点击，脚本轮询到对话框关闭后继续
    param([int]$ProcId, [IntPtr]$MainHwnd, [int]$HandleTimeoutSec = 180)

    # 等待出现
    $btn = [IntPtr]::Zero
    $deadline = [DateTime]::UtcNow.AddSeconds(8)
    while ([DateTime]::UtcNow -lt $deadline) {
        $btn = [ImeE2E.Helper]::FindConfirmButton($ProcId, '确定', $MainHwnd)
        if ($btn -ne [IntPtr]::Zero) { break }
        Start-Sleep -Milliseconds 300
    }
    if ($btn -eq [IntPtr]::Zero) {
        Write-Step "未弹出配置对话框（该格式无需配置）"
        return $true
    }
    Write-Step "检测到配置对话框，尝试自动点击'确定' (失败请人工点击, 最长 ${HandleTimeoutSec} 秒)..."

    # 自动尝试：BM_CLICK / 真实鼠标，各试一次，矩形非零才点
    for ($i = 0; $i -lt 2; $i++) {
        $btn = [ImeE2E.Helper]::FindConfirmButton($ProcId, '确定', $MainHwnd)
        if ($btn -eq [IntPtr]::Zero) { Write-Step "对话框已关闭"; return $true }
        $rect = New-Object 'ImeE2E.Native+RECT'
        [ImeE2E.Native]::GetWindowRect($btn, [ref]$rect) | Out-Null
        if ($rect.Right - $rect.Left -le 0) {
            Write-Host "[e2e] 按钮矩形为零 (幻影句柄), 跳过自动点击"
            break
        }
        [ImeE2E.Native]::SendMessage($btn, 0xF5, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
        Start-Sleep -Milliseconds 1200
        if ([ImeE2E.Helper]::FindConfirmButton($ProcId, '确定', $MainHwnd) -eq [IntPtr]::Zero) {
            Write-Step "配置对话框已确认 (BM_CLICK)"
            return $true
        }
        $x = [int](($rect.Left + $rect.Right) / 2)
        $y = [int](($rect.Top + $rect.Bottom) / 2)
        [ImeE2E.Native]::SetCursorPos($x, $y) | Out-Null
        Start-Sleep -Milliseconds 200
        [ImeE2E.Native]::mouse_event(0x02, 0, 0, 0, [UIntPtr]::Zero)
        Start-Sleep -Milliseconds 100
        [ImeE2E.Native]::mouse_event(0x04, 0, 0, 0, [UIntPtr]::Zero)
        Start-Sleep -Milliseconds 1200
        if ([ImeE2E.Helper]::FindConfirmButton($ProcId, '确定', $MainHwnd) -eq [IntPtr]::Zero) {
            Write-Step "配置对话框已确认 (鼠标点击)"
            return $true
        }
    }

    # 人工兜底
    Write-Host "[e2e] ** 请手动点击'确定'按钮关闭配置对话框 (最长 ${HandleTimeoutSec} 秒) **" -ForegroundColor Yellow
    $deadline = [DateTime]::UtcNow.AddSeconds($HandleTimeoutSec)
    while ([DateTime]::UtcNow -lt $deadline) {
        if ([ImeE2E.Helper]::FindConfirmButton($ProcId, '确定', $MainHwnd) -eq [IntPtr]::Zero) {
            Write-Step "配置对话框已确认 (人工)"
            return $true
        }
        Start-Sleep -Milliseconds 500
    }
    Write-Fail "等待配置对话框确认超时"
    return $false
}

function Wait-SaveDialogHandled {
    # 转换完成后 GUI 先弹"是否保存"询问框（是/否），点"是"后才弹真正的另存为对话框
    param([int]$ProcId, [string]$SavePath, [int]$HandleTimeoutSec = 180)

    # ---- 第 1 步：处理"是否保存"询问框 ----
    $ask = [ImeE2E.Helper]::FindWindowByPidAndClass($ProcId, '#32770', 60000)
    if ($ask -eq [IntPtr]::Zero) { throw "未等到保存询问对话框" }
    Write-Step "询问框: $([ImeE2E.Helper]::DescribeWindow($ask))"

    $yesBtn = [IntPtr]::Zero
    foreach ($name in @('是(&Y)', '是')) {
        $yesBtn = [ImeE2E.Helper]::FindChildByText($ask, $name)
        if ($yesBtn -ne [IntPtr]::Zero) { break }
    }
    $autoClicked = $false
    if ($yesBtn -ne [IntPtr]::Zero) {
        $rect = New-Object 'ImeE2E.Native+RECT'
        [ImeE2E.Native]::GetWindowRect($yesBtn, [ref]$rect) | Out-Null
        if ($rect.Right - $rect.Left -gt 0) {
            [ImeE2E.Native]::SendMessage($yesBtn, 0xF5, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
            Start-Sleep -Milliseconds 800
            $autoClicked = $true
            Write-Step "已点击'是'"
        }
    }
    if (-not $autoClicked) {
        Write-Host "[e2e] ** 请点击'是否保存'询问框的'是'按钮 **" -ForegroundColor Yellow
    }
    # 等询问框关闭（无论自动还是人工）
    $deadline = [DateTime]::UtcNow.AddSeconds($HandleTimeoutSec)
    while ([DateTime]::UtcNow -lt $deadline) {
        $stillAsk = [ImeE2E.Native]::FindWindow($null, '是否保存')
        if ($stillAsk -eq [IntPtr]::Zero) { break }
        Start-Sleep -Milliseconds 500
    }

    # ---- 第 2 步：处理另存为对话框 ----
    $dialog = [ImeE2E.Helper]::FindWindowByPidAndClass($ProcId, '#32770', 60000)
    if ($dialog -eq [IntPtr]::Zero) { throw "未等到另存为对话框" }
    Write-Step "另存为对话框: $([ImeE2E.Helper]::DescribeWindow($dialog))"

    # 文件名输入框：优先 IFileDialog 固定层级 ComboBoxEx32 > ComboBox > Edit，再任意层级 Edit。
    # 对话框刚弹出时子控件可能尚未创建完成，轮询重试。
    $edit = [IntPtr]::Zero
    $editDeadline = [DateTime]::UtcNow.AddSeconds(10)
    while ([DateTime]::UtcNow -lt $editDeadline -and $edit -eq [IntPtr]::Zero) {
        $cbxEx = [ImeE2E.Native]::FindWindowEx($dialog, [IntPtr]::Zero, 'ComboBoxEx32', $null)
        $combo = [IntPtr]::Zero
        if ($cbxEx -ne [IntPtr]::Zero) {
            $combo = [ImeE2E.Native]::FindWindowEx($cbxEx, [IntPtr]::Zero, 'ComboBox', $null)
        }
        if ($combo -ne [IntPtr]::Zero) {
            $edit = [ImeE2E.Native]::FindWindowEx($combo, [IntPtr]::Zero, 'Edit', $null)
        }
        if ($edit -eq [IntPtr]::Zero) {
            $edit = [ImeE2E.Helper]::FindChildByClass($dialog, 'Edit')
        }
        if ($edit -eq [IntPtr]::Zero) { Start-Sleep -Milliseconds 500 }
    }
    if ($edit -eq [IntPtr]::Zero) {
        # 现代另存为对话框的文件名框在 DirectUI 内部，不是传统子窗口：
        # 用 UIA 定位（读取可靠），再用原生 WM_SETTEXT 写入、鼠标点击保存
        try {
            $byPid = New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::ProcessIdProperty, $ProcId)
            $windows = [System.Windows.Automation.AutomationElement]::RootElement.FindAll(
                [System.Windows.Automation.TreeScope]::Children, $byPid)
            foreach ($w in $windows) {
                if ([IntPtr]$w.Current.NativeWindowHandle -ne $dialog) { continue }
                $isEdit = New-Object System.Windows.Automation.PropertyCondition(
                    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                    [System.Windows.Automation.ControlType]::Edit)
                $editEl = $w.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $isEdit)
                if ($null -ne $editEl) {
                    $eh = [IntPtr]$editEl.Current.NativeWindowHandle
                    if ($eh -ne [IntPtr]::Zero) {
                        [ImeE2E.Native]::SendMessage($eh, 0x0C, [IntPtr]::Zero, $SavePath) | Out-Null
                        $edit = $eh
                        Write-Step "已通过 UIA 定位文件名框并填入路径"
                    }
                }
                break
            }
        }
        catch { Write-Host "[e2e] UIA 定位文件名框失败: $($_.Exception.Message)" }
    }
    $autoClicked = $false
    if ($edit -ne [IntPtr]::Zero) {
        # 写入路径并回读验证；WM_SETTEXT 对现代对话框可能不生效，此时改用键入
        [ImeE2E.Native]::SendMessage($edit, 0x0C, [IntPtr]::Zero, $SavePath) | Out-Null
        Start-Sleep -Milliseconds 400
        $sb = New-Object System.Text.StringBuilder 512
        [ImeE2E.Native]::SendMessage($edit, 0x0D, [IntPtr]$sb.Capacity, $sb) | Out-Null
        if ($sb.ToString() -ne $SavePath) {
            Write-Host "[e2e] WM_SETTEXT 未生效 ('$($sb.ToString())'), 改用键入路径..."
            $rectE = New-Object 'ImeE2E.Native+RECT'
            [ImeE2E.Native]::GetWindowRect($edit, [ref]$rectE) | Out-Null
            if ($rectE.Right - $rectE.Left -gt 0) {
                [ImeE2E.Native]::SetCursorPos([int](($rectE.Left + $rectE.Right) / 2), [int](($rectE.Top + $rectE.Bottom) / 2)) | Out-Null
                Start-Sleep -Milliseconds 200
                [ImeE2E.Native]::mouse_event(0x02, 0, 0, 0, [UIntPtr]::Zero)
                Start-Sleep -Milliseconds 80
                [ImeE2E.Native]::mouse_event(0x04, 0, 0, 0, [UIntPtr]::Zero)
                Start-Sleep -Milliseconds 400
                [System.Windows.Forms.SendKeys]::SendWait('^a')
                Start-Sleep -Milliseconds 200
                [System.Windows.Forms.SendKeys]::SendWait($SavePath)
                Start-Sleep -Milliseconds 400
            }
            else {
                Write-Host "[e2e] 文件名框矩形为零, 无法键入"
            }
        }
        Write-Step "已填入保存路径, 尝试点击'保存'..."
        $btn = [ImeE2E.Helper]::FindChildByText($dialog, '保存')
        if ($btn -ne [IntPtr]::Zero) {
            $rect = New-Object 'ImeE2E.Native+RECT'
            [ImeE2E.Native]::GetWindowRect($btn, [ref]$rect) | Out-Null
            if ($rect.Right - $rect.Left -gt 0) {
                [ImeE2E.Native]::SendMessage($btn, 0xF5, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
                Start-Sleep -Milliseconds 1000
                if (-not (Test-Path $SavePath)) {
                    # BM_CLICK 无效时用真实鼠标点击按钮中心
                    [ImeE2E.Native]::SetCursorPos([int](($rect.Left + $rect.Right) / 2), [int](($rect.Top + $rect.Bottom) / 2)) | Out-Null
                    Start-Sleep -Milliseconds 200
                    [ImeE2E.Native]::mouse_event(0x02, 0, 0, 0, [UIntPtr]::Zero)
                    Start-Sleep -Milliseconds 100
                    [ImeE2E.Native]::mouse_event(0x04, 0, 0, 0, [UIntPtr]::Zero)
                }
                $autoClicked = $true
                Start-Sleep -Milliseconds 800
            }
        }
    }
    if (-not $autoClicked) {
        $children = [ImeE2E.Helper]::DescribeChildren($dialog)
        Write-Host "[e2e] 另存为对话框子控件: $($children -join ' | ')"
        Write-Host "[e2e] ** 请在另存为对话框中填入路径并点击'保存': $SavePath **" -ForegroundColor Yellow
    }

    # ---- 第 3 步：等待产物落盘（自动/人工统一） ----
    Write-Host "[e2e] 等待保存完成 (最长 ${HandleTimeoutSec} 秒): $SavePath" -ForegroundColor Yellow
    $deadline = [DateTime]::UtcNow.AddSeconds($HandleTimeoutSec)
    while ([DateTime]::UtcNow -lt $deadline) {
        if (Test-Path $SavePath) { Write-Step "保存完成"; return }
        Start-Sleep -Milliseconds 500
    }
    throw "等待保存超时: $SavePath"
}

function Find-ElementByCondition {
    param($Parent, $Condition, [int]$TimeoutMs = 5000, [int]$Scope = 4) # 4=Descendants
    $deadline = [DateTime]::UtcNow.AddMilliseconds($TimeoutMs)
    while ([DateTime]::UtcNow -lt $deadline) {
        $found = $Parent.FindFirst($Scope, $Condition)
        if ($null -ne $found) { return $found }
        Start-Sleep -Milliseconds 200
    }
    return $null
}

function Find-MainWindow {
    param([int]$ProcId, [int]$TimeoutMs = 20000)
    $byPid = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ProcessIdProperty, $ProcId)
    $isWindow = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Window)
    $cond = New-Object System.Windows.Automation.AndCondition($byPid, $isWindow)
    return Find-ElementByCondition -Parent ([System.Windows.Automation.AutomationElement]::RootElement) `
        -Condition $cond -TimeoutMs $TimeoutMs -Scope 2 # 2=Children
}

function Find-ById {
    param($Parent, [string]$AutomationId, [int]$TimeoutMs = 8000)
    $cond = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::AutomationIdProperty, $AutomationId)
    return Find-ElementByCondition -Parent $Parent -Condition $cond -TimeoutMs $TimeoutMs
}

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
