using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Studyzy.IMEWLConverter.Entities;
using Studyzy.IMEWLConverter.IME;

namespace Studyzy.IMEWLConverter
{
    class CoreWinFormMapping
    {
        private static IDictionary<Type, Form> imeFormMapping;
        private static IDictionary<Type, IDictionary<string, string>> formPropertityMapping;

        private void InitImportFormMapping()
        {
            imeFormMapping = new Dictionary<Type, Form>();
            imeFormMapping.Add(typeof (Rime), new RimeConfigForm());
            imeFormMapping.Add(typeof(LingoesLd2),new Ld2EncodingConfigForm());
            imeFormMapping.Add(typeof(Xiaoxiao),new XiaoxiaoConfigForm());
            imeFormMapping.Add(typeof(SelfDefining),new SelfDefiningConfigForm());
            imeFormMapping.Add(typeof(UserDefinePhrase),new PhraseFormatConfigForm());
            imeFormMapping.Add(typeof(XiaoxiaoErbi),new ErbiTypeForm());
        }


        private void InitImportPropertityMapping()
        {
            formPropertityMapping = new Dictionary<Type, IDictionary<string, string>>();
            formPropertityMapping.Add(typeof(RimeConfigForm), 
                new Dictionary<string, string>() { 
                { "SelectedCodeType", "CodeType" }, 
                { "SelectedOS", "OS" } });
            formPropertityMapping.Add(typeof(Ld2EncodingConfigForm),
                new Dictionary<string, string>()
                {
                    {"SelectedEncoding","WordEncoding"}
                });
            formPropertityMapping.Add(typeof(XiaoxiaoConfigForm),
                new Dictionary<string, string>()
                {
                    {"SelectedCodeType","CodeType"}
                });
            formPropertityMapping.Add(typeof(SelfDefiningConfigForm),
                new Dictionary<string, string>()
                {
                    {"SelectedParsePattern","UserDefiningPattern"}
                });
            formPropertityMapping.Add(typeof (PhraseFormatConfigForm),
                new Dictionary<string, string>()
                {
                    {"PhraseFormat","PhraseFormat"},
                    {"DefaultRank","DefaultRank"}
                });
            formPropertityMapping.Add(typeof(ErbiTypeForm),
                new Dictionary<string, string>()
                {
                    {"SelectedCodeType","CodeType"}
                });
        }

        private object ime;
       

        private void form_Closed(object sender, EventArgs e)
        {
            var form = sender;
            var props = form.GetType().GetProperties();
           var propMapping= formPropertityMapping[form.GetType()];
            foreach (PropertyInfo propertyInfo in props)
            {
                if (!propMapping.ContainsKey(propertyInfo.Name))
                {
                    continue;
                }
                var importProp = ime.GetType().GetProperty(propMapping[ propertyInfo.Name]);
                if (importProp != null)
                {
                   var value= propertyInfo.GetValue(form,null);
                    importProp.SetValue(ime,value,null);
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
            var t = import.GetType();
            if (imeFormMapping.ContainsKey(t))
            {
                var form = imeFormMapping[t];

                this.ime = import;
                form.Closed += new EventHandler(form_Closed);

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
            var t = export.GetType();
            if (imeFormMapping.ContainsKey(t))
            {
                var form = imeFormMapping[t];

                this.ime = export;
                form.Closed += new EventHandler(form_Closed);

                return form;
            }
            return null;
        }
        
    }
}
