# =============================================================================
# GUI 集成测试共享 UI 自动化库
#
# 被 windows-ime-e2e.ps1 / windows-gui-matrix.ps1 dot-source 复用。
# 依赖：Windows PowerShell 5.1+，交互桌面会话（UI Automation + Win32 消息）。
# =============================================================================

function Write-Step($msg) { Write-Host "[gui-test] $msg" -ForegroundColor Cyan }
function Write-Pass($msg) { Write-Host "[gui-test] PASS: $msg" -ForegroundColor Green }
function Write-Fail($msg) { Write-Host "[gui-test] FAIL: $msg" -ForegroundColor Red }

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
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool IsWindowVisible(System.IntPtr hWnd);
    }

    public static class Helper
    {
        const uint SMTO_ABORTIFHUNG = 0x0002;

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern System.IntPtr SendMessageTimeout(System.IntPtr hWnd, int Msg, System.IntPtr wParam, System.Text.StringBuilder lParam, uint fuFlags, uint uTimeout, out System.IntPtr lpdwResult);

        // 跨进程读取窗口文本：必须用 WM_GETTEXT，且必须带超时——
        // 目标窗口若属于已挂死的后台线程，无超时的 SendMessage 会永久阻塞
        public static string GetText(System.IntPtr h)
        {
            var sb = new System.Text.StringBuilder(512);
            System.IntPtr res;
            SendMessageTimeout(h, 0x0D, (System.IntPtr)sb.Capacity, sb, SMTO_ABORTIFHUNG, 2000, out res);
            return sb.ToString();
        }

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

        // 按标题查找指定进程的顶层窗口（枚举方式，FindWindow 按标题匹配实测不可靠）
        public static System.IntPtr FindWindowByTitle(int procId, string title)
        {
            System.IntPtr found = System.IntPtr.Zero;
            Native.EnumWindows(delegate(System.IntPtr h, System.IntPtr l)
            {
                uint pid;
                Native.GetWindowThreadProcessId(h, out pid);
                if (pid != (uint)procId) return true;
                if (!Native.IsWindowVisible(h)) return true;
                if (GetText(h).Contains(title)) { found = h; return false; }
                return true;
            }, System.IntPtr.Zero);
            return found;
        }

        public static System.IntPtr FindChildByText(System.IntPtr parent, string text)
        {
            System.IntPtr found = System.IntPtr.Zero;
            Native.EnumChildWindows(parent, delegate(System.IntPtr h, System.IntPtr l)
            {
                // GetWindowText 跨进程取不到其他进程控件的文本，必须发 WM_GETTEXT
                if (GetText(h).Contains(text)) { found = h; return false; }
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
                    var normalized = GetText(c).Replace(" ", "").Replace("\u3000", "");
                    if (normalized == buttonText) { btn = c; return false; }
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

        // 在指定进程的顶层窗口中找含指定文本子控件的窗口（文本用包含匹配），
        // 可排除一个窗口（通常是主窗口）。不依赖窗口标题，规避标题匹配失败的问题。
        public static System.IntPtr FindWindowWithButton(int procId, string buttonText, System.IntPtr exclude)
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
                    if (GetText(c).Contains(buttonText)) { btn = c; return false; }
                    return true;
                }, System.IntPtr.Zero);
                if (btn != System.IntPtr.Zero) { result = h; return false; }
                return true;
            }, System.IntPtr.Zero);
            return result;
        }

        public static string DescribeWindow(System.IntPtr h)
        {
            var sbC = new System.Text.StringBuilder(256);
            Native.GetClassName(h, sbC, 256);
            var cls = sbC.ToString();
            if (cls.Length > 30) cls = cls.Substring(0, 30);
            return "[" + cls + "] '" + GetText(h) + "'";
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
        Write-Host "[gui-test] UIA 选择失败, 回退原生下拉点击: $($_.Exception.Message)"
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
    catch { Write-Host "[gui-test] UIA Invoke 失败, 回退鼠标点击: $($_.Exception.Message)" }
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
            Write-Host "[gui-test] 按钮矩形为零 (幻影句柄), 跳过自动点击"
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
    Write-Host "[gui-test] ** 请手动点击'确定'按钮关闭配置对话框 (最长 ${HandleTimeoutSec} 秒) **" -ForegroundColor Yellow
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
    # 转换完成后 GUI 的对话框序列（MainForm.btnConvert_Click）：
    #   1) MessageBox"是否保存"(YesNo)  2) SaveFileDialog  3) 保存成功 MessageBox(OK)
    # 本函数处理前两步；保存成功提示框由调用方用 Wait-ConfigDialogHandled 关闭。
    param([int]$ProcId, [string]$SavePath, [int]$HandleTimeoutSec = 180)

    # ---- 第 1 步：等待"是否保存"询问框（以含"是"按钮为标志，不依赖窗口标题） ----
    $ask = [IntPtr]::Zero
    $deadline = [DateTime]::UtcNow.AddSeconds(60)
    while ([DateTime]::UtcNow -lt $deadline -and $ask -eq [IntPtr]::Zero) {
        $ask = [ImeE2E.Helper]::FindWindowWithButton($ProcId, '是', $MainHwnd)
        if ($ask -ne [IntPtr]::Zero) { break }
        # 转换出错时 GUI 先弹"错误日志"窗体（普通 Form，无按钮），关闭后流程继续
        $errWnd = [ImeE2E.Helper]::FindWindowByTitle($ProcId, '错误日志')
        if ($errWnd -ne [IntPtr]::Zero) {
            Write-Step "检测到错误日志窗口（转换有报错），关闭并继续..."
            [ImeE2E.Native]::SendMessage($errWnd, 0x0010, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null # WM_CLOSE
            Start-Sleep -Milliseconds 600
            continue
        }
        # 只有"确定"的 #32770 框（转换失败、残留提示等）：记录文本、点掉并报错
        $cand = [ImeE2E.Helper]::FindWindowByPidAndClass($ProcId, '#32770', 1000)
        if ($cand -ne [IntPtr]::Zero) {
            $okBtn = [ImeE2E.Helper]::FindChildByText($cand, '确定')
            if ($okBtn -ne [IntPtr]::Zero) {
                $boxText = ([ImeE2E.Helper]::DescribeChildren($cand) -join ' | ')
                [ImeE2E.Native]::SendMessage($okBtn, 0xF5, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
                Start-Sleep -Milliseconds 500
                throw "GUI 弹出非保存询问框: $([ImeE2E.Helper]::DescribeWindow($cand)) 内容[$boxText]（已点确定）"
            }
        }
        Start-Sleep -Milliseconds 400
    }
    if ($ask -eq [IntPtr]::Zero) { throw "未等到保存询问对话框" }
    Write-Step "询问框: $([ImeE2E.Helper]::DescribeWindow($ask))"

    # ---- 第 2 步：点击"是"并验证询问框关闭（GUI 线程忙时 BM_CLICK 可能被吞，需重试） ----
    $autoClicked = $false
    $clickDeadline = [DateTime]::UtcNow.AddSeconds(20)
    while ([DateTime]::UtcNow -lt $clickDeadline) {
        $ask = [ImeE2E.Helper]::FindWindowWithButton($ProcId, '是', $MainHwnd)
        if ($ask -eq [IntPtr]::Zero) { $autoClicked = $true; break }
        $yesBtn = [ImeE2E.Helper]::FindChildByText($ask, '是')
        if ($yesBtn -ne [IntPtr]::Zero) {
            [ImeE2E.Native]::SendMessage($yesBtn, 0xF5, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
            Start-Sleep -Milliseconds 700
            # BM_CLICK 未生效时用真实鼠标点击按钮中心
            if ([ImeE2E.Helper]::FindWindowWithButton($ProcId, '是', $MainHwnd) -ne [IntPtr]::Zero) {
                $rect = New-Object 'ImeE2E.Native+RECT'
                [ImeE2E.Native]::GetWindowRect($yesBtn, [ref]$rect) | Out-Null
                if ($rect.Right - $rect.Left -gt 0) {
                    [ImeE2E.Native]::SetCursorPos([int](($rect.Left + $rect.Right) / 2), [int](($rect.Top + $rect.Bottom) / 2)) | Out-Null
                    Start-Sleep -Milliseconds 150
                    [ImeE2E.Native]::mouse_event(0x02, 0, 0, 0, [UIntPtr]::Zero)
                    Start-Sleep -Milliseconds 80
                    [ImeE2E.Native]::mouse_event(0x04, 0, 0, 0, [UIntPtr]::Zero)
                    Start-Sleep -Milliseconds 700
                }
            }
        }
        else { Start-Sleep -Milliseconds 400 }
    }
    if ($autoClicked) {
        Write-Step "已点击'是'，询问框已关闭"
    }
    else {
        Write-Host "[gui-test] ** 请点击'是否保存'询问框的'是'按钮 **" -ForegroundColor Yellow
        $deadline = [DateTime]::UtcNow.AddSeconds($HandleTimeoutSec)
        while ([DateTime]::UtcNow -lt $deadline) {
            if ([ImeE2E.Helper]::FindWindowWithButton($ProcId, '是', $MainHwnd) -eq [IntPtr]::Zero) { $autoClicked = $true; break }
            Start-Sleep -Milliseconds 500
        }
        if (-not $autoClicked) { throw "等待'是否保存'询问框关闭超时" }
    }

    # ---- 第 3 步：等待另存为对话框（跳过尚未关闭的"是否保存"框） ----
    $dialog = [IntPtr]::Zero
    $deadline = [DateTime]::UtcNow.AddSeconds(60)
    while ([DateTime]::UtcNow -lt $deadline -and $dialog -eq [IntPtr]::Zero) {
        $cand = [ImeE2E.Helper]::FindWindowByPidAndClass($ProcId, '#32770', 3000)
        if ($cand -ne [IntPtr]::Zero -and
            [ImeE2E.Helper]::DescribeWindow($cand) -notmatch '是否保存' -and
            [ImeE2E.Helper]::FindChildByText($cand, '是') -eq [IntPtr]::Zero) {
            $dialog = $cand
        }
        else { Start-Sleep -Milliseconds 300 }
    }
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
        catch { Write-Host "[gui-test] UIA 定位文件名框失败: $($_.Exception.Message)" }
    }
    $autoClicked = $false
    if ($edit -ne [IntPtr]::Zero) {
        # 写入路径并回读验证；WM_SETTEXT 对现代对话框可能不生效，此时改用键入
        [ImeE2E.Native]::SendMessage($edit, 0x0C, [IntPtr]::Zero, $SavePath) | Out-Null
        Start-Sleep -Milliseconds 400
        $sb = New-Object System.Text.StringBuilder 512
        [ImeE2E.Native]::SendMessage($edit, 0x0D, [IntPtr]$sb.Capacity, $sb) | Out-Null
        if ($sb.ToString() -ne $SavePath) {
            Write-Host "[gui-test] WM_SETTEXT 未生效 ('$($sb.ToString())'), 改用键入路径..."
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
                Write-Host "[gui-test] 文件名框矩形为零, 无法键入"
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
        Write-Host "[gui-test] 另存为对话框子控件: $($children -join ' | ')"
        Write-Host "[gui-test] ** 请在另存为对话框中填入路径并点击'保存': $SavePath **" -ForegroundColor Yellow
    }

    # ---- 第 3 步：等待产物落盘（自动/人工统一） ----
    Write-Host "[gui-test] 等待保存完成 (最长 ${HandleTimeoutSec} 秒): $SavePath" -ForegroundColor Yellow
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
        try {
            $found = $Parent.FindFirst($Scope, $Condition)
            if ($null -ne $found) { return $found }
        }
        catch [System.Management.Automation.MethodInvocationException], [System.Runtime.InteropServices.COMException] {
            # UIA 跨进程调用在目标 GUI 线程繁忙时会抛"操作超时"，
            # 属瞬态错误：等待后重试，直至截止时间
        }
        Start-Sleep -Milliseconds 300
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
