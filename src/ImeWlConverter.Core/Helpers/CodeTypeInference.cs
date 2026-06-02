using ImeWlConverter.Abstractions.Enums;

namespace ImeWlConverter.Core.Helpers;

/// <summary>
/// Infers the appropriate code type from the output format when not explicitly specified.
/// </summary>
public static class CodeTypeInference
{
    /// <summary>
    /// Infers the target code type based on the output format ID.
    /// Used when the user does not explicitly select a code type.
    /// </summary>
    public static CodeType InferFromOutputFormat(string outputFormatId)
    {
        return outputFormatId switch
        {
            "wb86" => CodeType.Wubi86,
            "wb98" => CodeType.Wubi98,
            "wbnewage" => CodeType.WubiNewAge,
            "jd" or "qqwb" or "xywb" => CodeType.Wubi86,
            "jdzm" => CodeType.Zhengma,
            "cjpt" => CodeType.Cangjie5,
            "word" => CodeType.NoCode,
            _ => CodeType.Pinyin
        };
    }
}
