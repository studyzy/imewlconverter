using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Studyzy.IMEWLConverter.Helpers;

namespace ImeWlConverterMac.Views;

public partial class MergeWLWindow : Window
{
    public MergeWLWindow()
    {
        InitializeComponent();
        richTextBox1.Text = @"请保证主词库和附加词库中每一行的格式为：
编码 词1 词2 词3
不要保留任何注释备注等。
主词库只可选择一个，附加词库可多选";
    }

    private async void BtnSelectMainWLFile_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择主词库文件",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    FilePickerFileTypes.TextPlain,
                    FilePickerFileTypes.All
                }
            });

            if (files.Count > 0)
            {
                txbMainWLFile.Text = files[0].Path.LocalPath;
            }
        }
    }

    private async void BtnSelectUserWLFile_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择附加词库文件",
                AllowMultiple = true,
                FileTypeFilter = new[]
                {
                    FilePickerFileTypes.TextPlain,
                    FilePickerFileTypes.All
                }
            });

            if (files.Count > 0)
            {
                var filePaths = files.Select(f => f.Path.LocalPath).ToArray();
                txbUserWLFiles.Text = string.Join(" | ", filePaths);
            }
        }
    }

    private async void BtnMergeWL_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(txbMainWLFile.Text))
        {
            await ShowMessage("请先选择主词库文件", "词库合并");
            return;
        }

        if (string.IsNullOrEmpty(txbUserWLFiles.Text))
        {
            await ShowMessage("请先选择附加词库文件", "词库合并");
            return;
        }

        btnMergeWL.IsEnabled = false;
        richTextBox1.Text = "正在合并词库，请稍候...\n";

        try
        {
            await Task.Run(() => PerformMerge());
        }
        catch (Exception ex)
        {
            await ShowMessage($"合并失败: {ex.Message}", "错误");
        }
        finally
        {
            btnMergeWL.IsEnabled = true;
        }
    }

    private async void PerformMerge()
    {
        var mainWL = FileOperationHelper.ReadFile(txbMainWLFile.Text);
        var mainDict = ConvertTxt2Dictionary(mainWL);
        var userFiles = (txbUserWLFiles.Text ?? "").Split('|');

        foreach (var userFile in userFiles)
        {
            var filePath = userFile.Trim();
            var userTxt = FileOperationHelper.ReadFile(filePath);
            var userDict = ConvertTxt2Dictionary(userTxt);
            Merge2Dict(mainDict, userDict);
        }

        if (cbxSortByCode.IsChecked == true)
        {
            var keys = new List<string>(mainDict.Keys);
            keys.Sort();
            var sortedDict = new Dictionary<string, List<string>>();
            foreach (var key in keys)
                sortedDict.Add(key, mainDict[key]);
            mainDict = sortedDict;
        }

        var result = Dict2String(mainDict);

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            richTextBox1.Text = result;
        });

        // 询问是否保存
        var shouldSave = await ShowYesNoMessage(
            $"是否将合并的{mainDict.Count}条词库保存到本地硬盘上？",
            "是否保存"
        );

        if (shouldSave)
        {
            await SaveMergedFile(result);
        }
    }

    private static Dictionary<string, List<string>> ConvertTxt2Dictionary(string txt)
    {
        var lines = txt.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var mainDict = new Dictionary<string, List<string>>();

        foreach (var line in lines)
        {
            var array = line.Split(' ');
            var key = array[0];

            if (!mainDict.ContainsKey(key))
                mainDict.Add(key, new List<string>());

            for (var i = 1; i < array.Length; i++)
            {
                var word = array[i];
                mainDict[key].Add(word);
            }
        }

        return mainDict;
    }

    private static void Merge2Dict(
        Dictionary<string, List<string>> d1,
        Dictionary<string, List<string>> d2)
    {
        foreach (var pair in d2)
        {
            if (!d1.TryGetValue(pair.Key, out var v))
            {
                d1.Add(pair.Key, pair.Value);
            }
            else
            {
                foreach (var word in pair.Value)
                {
                    if (!v.Contains(word))
                        v.Add(word);
                }
            }
        }
    }

    private static string Dict2String(Dictionary<string, List<string>> dictionary)
    {
        var sb = new StringBuilder();

        foreach (var pair in dictionary)
        {
            sb.Append(pair.Key);

            if (pair.Value != null && pair.Value.Count > 0)
            {
                sb.Append(' ');
                sb.Append(string.Join(" ", pair.Value));
            }

            sb.Append("\n");
        }

        return sb.ToString();
    }

    private async Task SaveMergedFile(string content)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "保存合并后的词库",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("文本文件") { Patterns = new[] { "*.txt" } },
                    FilePickerFileTypes.All
                },
                DefaultExtension = ".txt",
                SuggestedFileName = "合并词库.txt"
            });

            if (file != null)
            {
                var filePath = file.Path.LocalPath;
                FileOperationHelper.WriteFile(filePath, Encoding.Unicode, content);
                await ShowMessage($"保存成功！\n文件路径：{filePath}", "保存成功");
            }
        }
    }

    private async Task ShowMessage(string message, string title)
    {
        var msgBox = new Window
        {
            Title = title,
            Width = 350,
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

    private async Task<bool> ShowYesNoMessage(string message, string title)
    {
        var result = false;
        var msgBox = new Window
        {
            Title = title,
            Width = 350,
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

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 10
        };

        var btnYes = new Button
        {
            Content = "是",
            Width = 80
        };
        btnYes.Click += (s, e) =>
        {
            result = true;
            msgBox.Close();
        };

        var btnNo = new Button
        {
            Content = "否",
            Width = 80
        };
        btnNo.Click += (s, e) =>
        {
            result = false;
            msgBox.Close();
        };

        buttonPanel.Children.Add(btnYes);
        buttonPanel.Children.Add(btnNo);
        panel.Children.Add(buttonPanel);

        msgBox.Content = panel;
        await msgBox.ShowDialog(this);

        return result;
    }

    private void BtnClose_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
