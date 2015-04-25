using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter
{
    internal class CoreWinFormMapping
    {
        private static IDictionary<Type, Form> imeFormMapping;
        private static IDictionary<Type, IDictionary<string, string>> formPropertityMapping;
        private object ime;

        private void InitImportFormMapping()
        {
            imeFormMapping = new Dictionary<Type, Form>();
            imeFormMapping.Add(typeof (Rime), new RimeConfigForm());
            imeFormMapping.Add(typeof (LingoesLd2), new Ld2EncodingConfigForm());
            imeFormMapping.Add(typeof (Xiaoxiao), new XiaoxiaoConfigForm());
            imeFormMapping.Add(typeof (SelfDefining), new SelfDefiningConfigForm());
            imeFormMapping.Add(typeof (UserDefinePhrase), new PhraseFormatConfigForm());
            imeFormMapping.Add(typeof (XiaoxiaoErbi), new ErbiTypeForm());
        }


        private void InitImportPropertityMapping()
        {
            formPropertityMapping = new Dictionary<Type, IDictionary<string, string>>();
            formPropertityMapping.Add(typeof (RimeConfigForm),
                new Dictionary<string, string>
                {
                    {"SelectedCodeType", "CodeType"},
                    {"SelectedOS", "OS"}
                });
            formPropertityMapping.Add(typeof (Ld2EncodingConfigForm),
                new Dictionary<string, string>
                {
                    {"SelectedEncoding", "WordEncoding"}
                });
            formPropertityMapping.Add(typeof (XiaoxiaoConfigForm),
                new Dictionary<string, string>
                {
                    {"SelectedCodeType", "CodeType"}
                });
            formPropertityMapping.Add(typeof (SelfDefiningConfigForm),
                new Dictionary<string, string>
                {
                    {"SelectedParsePattern", "UserDefiningPattern"}
                });
            formPropertityMapping.Add(typeof (PhraseFormatConfigForm),
                new Dictionary<string, string>
                {
                    {"PhraseFormat", "PhraseFormat"},
                    {"DefaultRank", "DefaultRank"}
                });
            formPropertityMapping.Add(typeof (ErbiTypeForm),
                new Dictionary<string, string>
                {
                    {"SelectedCodeType", "CodeType"}
                });
        }


        private void form_Closed(object sender, EventArgs e)
        {
            object form = sender;
            PropertyInfo[] props = form.GetType().GetProperties();
            IDictionary<string, string> propMapping = formPropertityMapping[form.GetType()];
            foreach (PropertyInfo propertyInfo in props)
            {
                if (!propMapping.ContainsKey(propertyInfo.Name))
                {
                    continue;
                }
                PropertyInfo importProp = ime.GetType().GetProperty(propMapping[propertyInfo.Name]);
                if (importProp != null)
                {
                    object value = propertyInfo.GetValue(form, null);
                    importProp.SetValue(ime, value, null);
                }
            }
        }


        public Form GetImportForm(IWordLibraryImport import)
        {
            if (imeFormMapping == null)
            {
                InitImportFormMapping();
            }
            if (formPropertityMapping == null)
            {
                InitImportPropertityMapping();
            }
            Type t = import.GetType();
            if (imeFormMapping.ContainsKey(t))
            {
                Form form = imeFormMapping[t];

                ime = import;
                form.Closed += form_Closed;

                return form;
            }
            return null;
        }

        public Form GetExportForm(IWordLibraryExport export)
        {
            if (imeFormMapping == null)
            {
                InitImportFormMapping();
            }
            if (formPropertityMapping == null)
            {
                InitImportPropertityMapping();
            }
            Type t = export.GetType();
            if (imeFormMapping.ContainsKey(t))
            {
                Form form = imeFormMapping[t];

                ime = export;
                form.Closed += form_Closed;

                return form;
            }
            return null;
        }
    }
}