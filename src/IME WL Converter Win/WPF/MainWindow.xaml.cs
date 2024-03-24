using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Studyzy.IMEWLConverter.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadTitle();
            LoadImeList();
        }

        private readonly IDictionary<string, IWordLibraryExport> exports =
            new Dictionary<string, IWordLibraryExport>();
        private readonly IDictionary<string, IWordLibraryImport> imports =
            new Dictionary<string, IWordLibraryImport>();

        #region Init


        private void LoadImeList()
        {
            Assembly assembly = GetType().Assembly;
            Type[] d = assembly.GetTypes();
            var cbxImportItems = new List<ComboBoxShowAttribute>();
            var cbxExportItems = new List<ComboBoxShowAttribute>();

            foreach (Type type in d)
            {
                if (
                    type.Namespace != null
                    && type.Namespace.StartsWith("Studyzy.IMEWLConverter.IME")
                )
                {
                    object[] att = type.GetCustomAttributes(typeof(ComboBoxShowAttribute), false);
                    if (att.Length > 0)
                    {
                        var cbxa = att[0] as ComboBoxShowAttribute;
                        Debug.WriteLine(cbxa.Name);
                        Debug.WriteLine(cbxa.Index);
                        if (type.GetInterface("IWordLibraryImport") != null)
                        {
                            Debug.WriteLine("Import!!!!" + type.FullName);
                            cbxImportItems.Add(cbxa);
                            imports.Add(
                                cbxa.Name,
                                assembly.CreateInstance(type.FullName) as IWordLibraryImport
                            );
                        }
                        if (type.GetInterface("IWordLibraryExport") != null)
                        {
                            Debug.WriteLine("Export!!!!" + type.FullName);
                            cbxExportItems.Add(cbxa);
                            exports.Add(
                                cbxa.Name,
                                assembly.CreateInstance(type.FullName) as IWordLibraryExport
                            );
                        }
                    }
                }
            }
            cbxImportItems.Sort((a, b) => a.Index - b.Index);
            cbxExportItems.Sort((a, b) => a.Index - b.Index);
            CbxFrom.Items.Clear();
            foreach (ComboBoxShowAttribute comboBoxShowAttribute in cbxImportItems)
            {
                CbxFrom.Items.Add(comboBoxShowAttribute.Name);
            }
            CbxTo.Items.Clear();
            foreach (ComboBoxShowAttribute comboBoxShowAttribute in cbxExportItems)
            {
                CbxTo.Items.Add(comboBoxShowAttribute.Name);
            }
        }

        private void LoadTitle()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            this.Title = "深蓝词库转换" + v.Major + "." + v.Minor;
        }
        #endregion


        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            this.TxbFilePath.Text = "Hello";
        }
    }
}
