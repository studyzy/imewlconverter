using Avalonia;
using System;
using System.Text;

namespace ImeWlConverterMac;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // GBK/GB2312 等编码在 .NET 里需要显式注册。CLI 与 WinForm 版本都做了这件事,
        // 这里以前漏了, 导致读取搜狗拼音/谷歌拼音等 GBK 词库时全部解析失败。
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}