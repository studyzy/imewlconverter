using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Studyzy.IMEWLConverter.Helpers;

namespace ImeWlConverterMac.Views;

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
                if (rbtnSplitByLine.IsChecked == true)
                SplitFileByLine((int)(numdMaxLine.Value ?? 0));
                else if (rbtnSplitBySize.IsChecked == true)
                    SplitFileBySize((int)(numdMaxSize.Value ?? 0));
                else
                    SplitFileByLength((int)(numdMaxLength.Value ?? 0));
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

    private void SplitFileByLine(int maxLine)
    {
        var encoding = FileOperationHelper.GetEncodingType(txbFilePath.Text);
        var str = FileOperationHelper.ReadFile(txbFilePath.Text, encoding);

        var splitLineChar = "\r\n";
        if (str.IndexOf(splitLineChar) < 0)
        {
            if (str.IndexOf('\r') > 0)
            {
                splitLineChar = "\r";
            }
            else if (str.IndexOf('\n') > 0)
            {
                splitLineChar = "\n";
            }
            else
            {
                AppendLog("不能找到行分隔符");
                return;
            }
        }

        var list = str.Split(new[] { splitLineChar }, StringSplitOptions.RemoveEmptyEntries);
        var fileContent = new StringBuilder();
        var fileIndex = 1;

        for (var i = 0; i < list.Length; i++)
        {
            fileContent.Append(list[i]);
            fileContent.Append(splitLineChar);

            if ((i + 1) % maxLine == 0 || i == list.Length - 1)
            {
                if (i != 0)
                {
                    var newFile = GetWriteFilePath(fileIndex++);
                    FileOperationHelper.WriteFile(newFile, encoding, fileContent.ToString());
                    AppendLog(newFile);
                    fileContent = new StringBuilder();
                }
            }
        }
    }

    private void SplitFileBySize(int maxSize)
    {
        var encoding = FileOperationHelper.GetEncodingType(txbFilePath.Text);
        var fileIndex = 1;
        var size = (maxSize - 10) * 1024; // 10K的Buffer

        using var inFile = new FileStream(txbFilePath.Text!, FileMode.Open, FileAccess.Read);

        do
        {
            var newFile = GetWriteFilePath(fileIndex++);
            if (string.IsNullOrEmpty(newFile))
            {
                AppendLog("无法生成有效的文件路径");
                return;
            }
            using var outFile = new FileStream(newFile!, FileMode.OpenOrCreate, FileAccess.Write);

            if (fileIndex != 2) // 不是第一个文件，那么就要写文件头
                FileOperationHelper.WriteFileHeader(outFile, encoding);

            var buffer = new byte[size];
            var data = inFile.Read(buffer, 0, size);

            if (data > 0)
            {
                outFile.Write(buffer, 0, data);
                var hasContent = true;

                do
                {
                    var b = inFile.ReadByte();
                    if (b == 0xA || b == 0xD)
                    {
                        ReadToNextLine(inFile);
                        hasContent = false;
                    }

                    if (b != -1) // 文件已经读完
                        outFile.WriteByte((byte)b);
                    else
                        hasContent = false;
                } while (hasContent);
            }

            AppendLog(newFile);
        } while (inFile.Position != inFile.Length);
    }

    private bool ReadToNextLine(FileStream fs)
    {
        do
        {
            var b = fs.ReadByte();
            if (b == -1) return false;

            if (b != 0xA && b != 0xD && b != 0)
            {
                fs.Position--;
                return true;
            }
        } while (true);
    }

    private void SplitFileByLength(int length)
    {
        length = length - 100; // 100个字的Buffer
        var encoding = FileOperationHelper.GetEncodingType(txbFilePath.Text);
        var str = FileOperationHelper.ReadFile(txbFilePath.Text, encoding);
        var fileIndex = 1;

        do
        {
            if (str.Length == 0) break;

            var content = str.Substring(0, Math.Min(str.Length, length));
            str = str.Substring(content.Length);

            var i = Math.Min(
                str.IndexOf('\r') >= 0 ? str.IndexOf('\r') : int.MaxValue,
                str.IndexOf('\n') >= 0 ? str.IndexOf('\n') : int.MaxValue
            );

            if (i != int.MaxValue && i != -1)
            {
                content += str.Substring(0, Math.Min(i + 2, str.Length));
                str = str.Substring(Math.Min(i + 2, str.Length));
            }

            var newFile = GetWriteFilePath(fileIndex++);
            FileOperationHelper.WriteFile(newFile, encoding, content);
            AppendLog(newFile);
        } while (true);
    }

    private string GetWriteFilePath(int i)
    {
        var path = txbFilePath.Text;
        return Path.Combine(
            Path.GetDirectoryName(path) ?? "",
            Path.GetFileNameWithoutExtension(path) + i.ToString("00") + Path.GetExtension(path)
        );
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
