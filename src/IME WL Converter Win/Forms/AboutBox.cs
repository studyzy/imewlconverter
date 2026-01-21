/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)

 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.

 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.

 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Studyzy.IMEWLConverter;

internal partial class AboutBox : Form
{
    public AboutBox()
    {
        InitializeComponent();
        Text = "关于 深蓝词库转换";
        labelProductName.Text = AssemblyProduct;
        labelVersion.Text = string.Format("版本 {0}", AssemblyVersion);
        labelCopyright.Text = AssemblyCopyright;
        labelCompanyName.Text = AssemblyCompany;
        textBoxDescription.Text = AssemblyDescription;
    }

    private void okButton_Click(object sender, EventArgs e)
    {
        Close();
    }

    #region 程序集属性访问器

    // 从当前执行的程序集获取版本号（WinForm 程序集）
    // 优先使用 InformationalVersion，其次是 FileVersion，最后是 AssemblyVersion
    public string AssemblyVersion
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            // 优先使用 AssemblyInformationalVersionAttribute（包含完整版本信息）
            var infoVersion = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;
            
            if (!string.IsNullOrWhiteSpace(infoVersion))
            {
                return infoVersion;
            }
            
            // 其次使用 AssemblyFileVersionAttribute
            var fileVersion = assembly
                .GetCustomAttribute<AssemblyFileVersionAttribute>()?
                .Version;
            
            if (!string.IsNullOrWhiteSpace(fileVersion))
            {
                return fileVersion;
            }
            
            // 最后使用 Assembly.GetName().Version
            var assemblyVersion = assembly.GetName().Version;
            if (assemblyVersion != null && assemblyVersion.ToString() != "0.0.0.0")
            {
                return assemblyVersion.ToString();
            }
            
            // 如果所有方法都失败，尝试使用 ConstantString.VERSION 作为后备
            if (!string.IsNullOrWhiteSpace(ConstantString.VERSION) && 
                ConstantString.VERSION != "0.0.0.0")
            {
                return ConstantString.VERSION;
            }
            
            return "1.0.0.0";
        }
    }

    public string AssemblyDescription
    {
        get
        {
            var attributes = Assembly
                .GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            if (attributes.Length == 0) return "";
            return ((AssemblyDescriptionAttribute)attributes[0]).Description;
        }
    }

    public string AssemblyProduct
    {
        get
        {
            var attributes = Assembly
                .GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (attributes.Length == 0) return "";
            return ((AssemblyProductAttribute)attributes[0]).Product;
        }
    }

    public string AssemblyCopyright
    {
        get
        {
            var attributes = Assembly
                .GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes.Length == 0) return "";
            return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }
    }

    public string AssemblyCompany
    {
        get
        {
            var attributes = Assembly
                .GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            if (attributes.Length == 0) return "";
            return ((AssemblyCompanyAttribute)attributes[0]).Company;
        }
    }

    #endregion
}
