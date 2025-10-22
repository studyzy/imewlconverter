using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ImeWlConverterMac.Views;

public partial class HelpWindow : Window
{
    public HelpWindow()
    {
        InitializeComponent();
        LoadHelpText();
    }

    private void LoadHelpText()
    {
        textBoxHelp.Text = @"深蓝词库转换 使用帮助

1. 选择源词库文件
   点击""...""按钮或直接拖拽文件到文本框中

2. 选择源词库类型
   从下拉列表中选择对应的输入法类型

3. 选择目标词库类型
   从下拉列表中选择要转换到的输入法类型

4. 点击""转换""按钮
   等待转换完成后，可以保存转换结果

高级功能：

• 词条过滤设置：可以设置词长、词频等过滤条件
• 词频生成设置：可以选择不同的词频生成方式
• 简繁体转换：支持简繁体之间的转换
• 文件分割：可以将大文件分割成多个小文件
• 词库合并：可以将多个词库合并成一个

更多信息请访问：
https://github.com/studyzy/imewlconverter";
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
