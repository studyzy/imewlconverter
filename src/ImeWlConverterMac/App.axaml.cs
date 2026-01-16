using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ImeWlConverterMac.ViewModels;
using ImeWlConverterMac.Views;

namespace ImeWlConverterMac;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewModel = new MainWindowViewModel();

            var mainWindow = new MainWindow
            {
                DataContext = viewModel,
            };

            desktop.MainWindow = mainWindow;

            // 设置 macOS 原生菜单（在设置 MainWindow 之后）
            if (OperatingSystem.IsMacOS())
            {
                SetupMacOSMenu(mainWindow, viewModel);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void SetupMacOSMenu(Window window, MainWindowViewModel viewModel)
    {
        var menu = new NativeMenu();

        // 应用程序菜单（第一个菜单）
        // macOS 会自动将第一个菜单的标题替换为应用程序名称
        var appMenu = new NativeMenuItem("");
        var appSubMenu = new NativeMenu();

        // 关于
        var aboutItem = new NativeMenuItem("关于深蓝词库转换");
        aboutItem.Click += (s, e) => viewModel.AboutCommand.Execute(null);
        appSubMenu.Add(aboutItem);

        appSubMenu.Add(new NativeMenuItemSeparator());

        // 退出
        var quitItem = new NativeMenuItem("退出深蓝词库转换")
        {
            Gesture = KeyGesture.Parse("Cmd+Q")
        };
        quitItem.Click += (s, e) =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                lifetime.Shutdown();
            }
        };
        appSubMenu.Add(quitItem);

        appMenu.Menu = appSubMenu;
        menu.Add(appMenu);

        // 高级设置菜单
        var advancedMenu = new NativeMenuItem("高级设置");
        var advancedSubMenu = new NativeMenu();

        var filterConfigItem = new NativeMenuItem("词条过滤设置");
        filterConfigItem.Click += (s, e) => viewModel.FilterConfigCommand.Execute(null);
        advancedSubMenu.Add(filterConfigItem);

        var rankGenerateItem = new NativeMenuItem("词频生成设置");
        rankGenerateItem.Click += (s, e) => viewModel.RankGenerateCommand.Execute(null);
        advancedSubMenu.Add(rankGenerateItem);

        advancedSubMenu.Add(new NativeMenuItemSeparator());

        // 切换选项
        var toggleShowLessItem = new NativeMenuItem("结果只显示首、末10万字符");
        toggleShowLessItem.Click += (s, e) => viewModel.ToggleShowLessCommand.Execute(null);
        advancedSubMenu.Add(toggleShowLessItem);

        var toggleExportDirectlyItem = new NativeMenuItem("不显示结果，直接导出");
        toggleExportDirectlyItem.Click += (s, e) => viewModel.ToggleExportDirectlyCommand.Execute(null);
        advancedSubMenu.Add(toggleExportDirectlyItem);

        var toggleStreamExportItem = new NativeMenuItem("一边读取，一边导出");
        toggleStreamExportItem.Click += (s, e) => viewModel.ToggleStreamExportCommand.Execute(null);
        advancedSubMenu.Add(toggleStreamExportItem);

        var toggleMergeToOneFileItem = new NativeMenuItem("合并多词库到一个文件");
        toggleMergeToOneFileItem.Click += (s, e) => viewModel.ToggleMergeToOneFileCommand.Execute(null);
        advancedSubMenu.Add(toggleMergeToOneFileItem);

        advancedSubMenu.Add(new NativeMenuItemSeparator());

        var chineseTransItem = new NativeMenuItem("简繁体转换设置");
        chineseTransItem.Click += (s, e) => viewModel.ChineseTransConfigCommand.Execute(null);
        advancedSubMenu.Add(chineseTransItem);

        advancedMenu.Menu = advancedSubMenu;
        menu.Add(advancedMenu);

        // 帮助菜单
        var helpMenu = new NativeMenuItem("帮助");
        var helpSubMenu = new NativeMenu();

        var donateItem = new NativeMenuItem("捐赠");
        donateItem.Click += (s, e) => viewModel.DonateCommand.Execute(null);
        helpSubMenu.Add(donateItem);

        var helpDocItem = new NativeMenuItem("帮助文档");
        helpDocItem.Click += (s, e) => viewModel.HelpCommand.Execute(null);
        helpSubMenu.Add(helpDocItem);

        var checkVersionItem = new NativeMenuItem("查看最新版本");
        checkVersionItem.Click += (s, e) => viewModel.AccessWebSiteCommand.Execute(null);
        helpSubMenu.Add(checkVersionItem);

        helpSubMenu.Add(new NativeMenuItemSeparator());

        var splitFileItem = new NativeMenuItem("文件分割");
        splitFileItem.Click += (s, e) => viewModel.SplitFileCommand.Execute(null);
        helpSubMenu.Add(splitFileItem);

        var mergeWLItem = new NativeMenuItem("词库合并");
        mergeWLItem.Click += (s, e) => viewModel.MergeWLCommand.Execute(null);
        helpSubMenu.Add(mergeWLItem);

        helpMenu.Menu = helpSubMenu;
        menu.Add(helpMenu);

        // 设置为窗口的原生菜单
        // 这会替换默认的 Avalonia 菜单
        NativeMenu.SetMenu(window, menu);
    }
}