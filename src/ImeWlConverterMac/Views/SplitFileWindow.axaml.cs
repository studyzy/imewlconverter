using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ImeWlConverter.Application.MergeSplit;

namespace ImeWlConverterMac.Views;

/// <summary>
/// 文件分割窗口。分割算法在 <see cref="MergeSplitService"/>（三端共享），此处仅是 UI 壳。
/// </summary>
public partial class SplitFileWindow : Window
{
    public SplitFileWindow()
    {
        InitializeComponent();
    }

    private async void BtnSelectFile_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择要分割的文件",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    FilePickerFileTypes.TextPlain,
                    FilePickerFileTypes.All
                }
            });

            if (files.Count > 0)
            {
                txbFilePath.Text = files[0].Path.LocalPath;
            }
        }
    }

    private async void BtnSplit_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(txbFilePath.Text))
        {
            await ShowMessage("请先选择要分割的文件", "分割");
            return;
        }

        if (!File.Exists(txbFilePath.Text))
        {
            await ShowMessage($"{txbFilePath.Text}，该文件不存在", "分割");
            return;
        }

        rtbLogs.Text = "";
        btnSplit.IsEnabled = false;

        try
        {
            await Task.Run(() =>
            {
                var mode = rbtnSplitByLine.IsChecked == true ? SplitMode.ByLine
                    : rbtnSplitBySize.IsChecked == true ? SplitMode.BySize
                    : SplitMode.ByLength;
                var max = mode == SplitMode.ByLine ? (int)(numdMaxLine.Value ?? 0)
                    : mode == SplitMode.BySize ? (int)(numdMaxSize.Value ?? 0)
                    : (int)(numdMaxLength.Value ?? 0);

                var parts = MergeSplitService.SplitFile(
                    txbFilePath.Text, new SplitOptions { Mode = mode, Max = max });
                foreach (var part in parts)
                    AppendLog(part);
            });

            await ShowMessage("恭喜你，文件分割完成!", "分割");
        }
        catch (Exception ex)
        {
            await ShowMessage($"分割失败: {ex.Message}", "错误");
        }
        finally
        {
            btnSplit.IsEnabled = true;
        }
    }

    private void AppendLog(string message)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            rtbLogs.Text += message + "\n";
        });
    }

    private async Task ShowMessage(string message, string title)
    {
        var msgBox = new Window
        {
            Title = title,
            Width = 300,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var panel = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            Spacing = 20
        };

        panel.Children.Add(new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });

        var btn = new Button
        {
            Content = "确定",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Width = 80
        };
        btn.Click += (s, e) => msgBox.Close();
        panel.Children.Add(btn);

        msgBox.Content = panel;
        await msgBox.ShowDialog(this);
    }

    private void BtnClose_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
