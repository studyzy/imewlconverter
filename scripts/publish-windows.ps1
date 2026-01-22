# ============================================================================
# Windows 发布脚本 (PowerShell)
# ============================================================================
# 用于在Windows环境下发布优化的深蓝词库转换程序
# ============================================================================

param(
    [string]$Configuration = "Release",
    [string]$Architecture = "x64",  # x64 or x86
    [switch]$Help
)

# 显示帮助信息
if ($Help) {
    Write-Host "Windows 发布脚本" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "用法: .\publish-windows.ps1 [-Configuration <配置>] [-Architecture <架构>] [-Help]"
    Write-Host ""
    Write-Host "参数:"
    Write-Host "  -Configuration  构建配置 (Debug/Release, 默认: Release)"
    Write-Host "  -Architecture   目标架构 (x64/x86, 默认: x64)"
    Write-Host "  -Help          显示此帮助信息"
    Write-Host ""
    Write-Host "示例:"
    Write-Host "  .\publish-windows.ps1                    # 发布 x64 Release版本"
    Write-Host "  .\publish-windows.ps1 -Architecture x86  # 发布 x86 Release版本"
    Write-Host "  .\publish-windows.ps1 -Configuration Debug -Architecture x64"
    Write-Host ""
    exit 0
}

# 颜色输出函数
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Type = "Info"
    )
    
    switch ($Type) {
        "Success" { Write-Host "✅ $Message" -ForegroundColor Green }
        "Error"   { Write-Host "❌ $Message" -ForegroundColor Red }
        "Warning" { Write-Host "⚠️  $Message" -ForegroundColor Yellow }
        "Info"    { Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
        default   { Write-Host $Message }
    }
}

# 检查 .NET SDK
Write-ColorOutput "检查 .NET SDK..." "Info"
try {
    $dotnetVersion = dotnet --version
    Write-ColorOutput ".NET SDK 版本: $dotnetVersion" "Success"
} catch {
    Write-ColorOutput ".NET SDK 未安装！请从 https://dotnet.microsoft.com/download 下载安装" "Error"
    exit 1
}

# 设置路径
$ProjectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$WinProject = Join-Path $ProjectRoot "src\IME WL Converter Win\IME WL Converter Win.csproj"
$OutputDir = Join-Path $ProjectRoot "publish\win-$Architecture"

# 检查项目文件
if (-not (Test-Path $WinProject)) {
    Write-ColorOutput "项目文件未找到: $WinProject" "Error"
    exit 1
}

Write-ColorOutput "项目路径: $WinProject" "Info"
Write-ColorOutput "输出目录: $OutputDir" "Info"
Write-ColorOutput "配置: $Configuration" "Info"
Write-ColorOutput "架构: $Architecture" "Info"
Write-Host ""

# 清理旧的发布文件
if (Test-Path $OutputDir) {
    Write-ColorOutput "清理旧的发布文件..." "Info"
    Remove-Item -Path $OutputDir -Recurse -Force
}

# 发布项目
Write-ColorOutput "开始发布 Windows $Architecture 版本..." "Info"
Write-Host ""

$publishArgs = @(
    "publish",
    "`"$WinProject`"",
    "--configuration", $Configuration,
    "--runtime", "win-$Architecture",
    "--self-contained", "true",
    "--output", "`"$OutputDir`"",
    "-p:PublishTrimmed=true",
    "-p:PublishSingleFile=true",
    "-p:DebugType=none",
    "-p:DebugSymbols=false"
)

Write-ColorOutput "执行命令: dotnet $($publishArgs -join ' ')" "Info"
Write-Host ""

$process = Start-Process -FilePath "dotnet" -ArgumentList $publishArgs -NoNewWindow -Wait -PassThru

if ($process.ExitCode -eq 0) {
    Write-Host ""
    Write-ColorOutput "发布成功！" "Success"
    Write-Host ""
    
    # 显示包大小
    $exeFile = Get-ChildItem -Path $OutputDir -Filter "*.exe" | Select-Object -First 1
    if ($exeFile) {
        $sizeInMB = [math]::Round($exeFile.Length / 1MB, 2)
        Write-ColorOutput "可执行文件: $($exeFile.Name)" "Info"
        Write-ColorOutput "文件大小: $sizeInMB MB" "Info"
        Write-ColorOutput "输出路径: $($exeFile.FullName)" "Info"
    }
    
    # 显示目录总大小
    $totalSize = (Get-ChildItem -Path $OutputDir -Recurse | Measure-Object -Property Length -Sum).Sum
    $totalSizeInMB = [math]::Round($totalSize / 1MB, 2)
    Write-ColorOutput "总包大小: $totalSizeInMB MB" "Info"
    
    Write-Host ""
    Write-ColorOutput "可以运行程序测试: $OutputDir\深蓝词库转换.exe" "Success"
} else {
    Write-Host ""
    Write-ColorOutput "发布失败！退出代码: $($process.ExitCode)" "Error"
    exit $process.ExitCode
}

# 显示优化提示
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  优化说明" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "已启用的优化选项:" -ForegroundColor Yellow
Write-Host "  ✓ 代码裁剪 (PublishTrimmed)" -ForegroundColor Green
Write-Host "  ✓ 单文件发布 (PublishSingleFile)" -ForegroundColor Green
Write-Host "  ✓ 移除调试符号" -ForegroundColor Green
Write-Host "  ✓ ReadyToRun 预编译" -ForegroundColor Green
Write-Host ""
Write-Host "如果包大小仍然较大，可以考虑:" -ForegroundColor Yellow
Write-Host "  • 使用 x86 架构 (比 x64 小约 10-15%)"
Write-Host "  • 移除 Microsoft.Office.Interop.Word 依赖"
Write-Host "  • 查看 docs/optimization/ 目录的优化文档"
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
