# 截取主屏保存为 PNG（用于 GUI 集成测试调试）
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$outPath = 'C:\Users\Administrator\AppData\Local\Temp\2\ime-gui-shot.png'
if ($args.Count -ge 1 -and $args[0]) { $outPath = $args[0] }
$bounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
$bmp = New-Object System.Drawing.Bitmap($bounds.Width, $bounds.Height)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
$bmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()
Write-Host "saved: $outPath ($($bounds.Width)x$($bounds.Height))"
