using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ImeWlConverterMac.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        LoadAssemblyInfo();
    }

    private void LoadAssemblyInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        
        labelProductName.Text = AssemblyProduct;
        labelVersion.Text = $"版本 {version?.Major}.{version?.Minor}";
        if (version?.Revision != 0)
        {
            labelVersion.Text += $".{version?.Revision}";
        }
        labelCopyright.Text = AssemblyCopyright;
        labelCompanyName.Text = AssemblyCompany;
        textBoxDescription.Text = AssemblyDescription;
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    #region Assembly Attribute Accessors

    private string AssemblyProduct
    {
        get
        {
            var attributes = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (attributes.Length == 0)
                return "深蓝词库转换";
            return ((AssemblyProductAttribute)attributes[0]).Product;
        }
    }

    private string AssemblyCopyright
    {
        get
        {
            var attributes = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes.Length == 0)
                return "Copyright © 2009-2024 studyzy(深蓝,曾毅)";
            return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }
    }

    private string AssemblyCompany
    {
        get
        {
            var attributes = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            if (attributes.Length == 0)
                return "";
            return ((AssemblyCompanyAttribute)attributes[0]).Company;
        }
    }

    private string AssemblyDescription
    {
        get
        {
            var attributes = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            if (attributes.Length == 0)
                return "深蓝词库转换是一款开源、免费的输入法词库转换工具。\n\n" +
                       "支持多种输入法词库格式的相互转换，包括搜狗、QQ、百度、微软拼音等主流输入法。\n\n" +
                       "项目地址：https://github.com/studyzy/imewlconverter";
            return ((AssemblyDescriptionAttribute)attributes[0]).Description;
        }
    }

    #endregion
}
