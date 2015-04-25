using System.Text;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter
{
    public interface IWordLibraryImport
    {
        int CountWord { get; set; }
        int CurrentStatus { get; set; }
        bool IsText { get; }
        CodeType CodeType { get; }
        //int DefaultRank { get; set; }
        WordLibraryList Import(string path);
        WordLibraryList ImportLine(string str);
        //Form ImportConfigForm { get; }
    }

    public interface IWordLibraryTextImport : IWordLibraryImport
    {
        Encoding Encoding { get; }
        WordLibraryList ImportText(string text);
    }

    public interface IWordLibraryExport
    {
        Encoding Encoding { get; }
        CodeType CodeType { get; }
        string Export(WordLibraryList wlList);
        string ExportLine(WordLibrary wl);
        //Form ExportConfigForm { get; }
    }

    public interface IMultiCodeType
    {
        CodeType CodeType { get; }
    }

    public interface IStreamPrepare
    {
        void Prepare();
    }
}