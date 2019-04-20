using System.IO;
using System.Reflection;

public class Resource4Test
{
    public static string NoPinyinWordOnly
    {
        get
        {
            return GetResourceContent("NoPinyinWordOnly.txt");
        }
    }
    public static string GooglePinyin
    {
        get
        {
            return GetResourceContent("GooglePinyin.txt");
        }
    }

    public static string PinyinJiajia
    {
        get
        {
            return GetResourceContent("PinyinJiajia.txt");
        }
    }


    private static string GetResourceContent(string fileName)
    {
        string file;
        var assembly = typeof(Resource4Test).GetTypeInfo().Assembly;

        using (var stream = assembly.GetManifestResourceStream("ImeWlConverterCoreTest.Resources." + fileName))
        {
            using (var reader = new StreamReader(stream, true))
            {
                file = reader.ReadToEnd();
            }
        }
        return file;
    }
}