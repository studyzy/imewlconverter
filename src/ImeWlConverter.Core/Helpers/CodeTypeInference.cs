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
            // QQ 输入法自定义短语的编码列是字面编码（QQ 拼音/QQ 五笔共用同一格式），
            // 原样保留，避免把五笔短语强制重生成拼音
            "qqdy" => CodeType.UserDefinePhrase,
            "word" => CodeType.NoCode,
            _ => CodeType.Pinyin
        };
    }
}
