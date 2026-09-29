using ImeWlConverter.Abstractions.Enums;
using ImeWlConverter.Core.Helpers;

namespace ImeWlConverter.Application.Mapping;

/// <summary>
/// 编码类型映射：CLI 字符串 → CodeType，以及输出格式 → CodeType 推断。
/// </summary>
public static class CodeTypeMapper
{
    /// <summary>解析 CLI --code-type 参数；未知值返回 NoCode（由调用方决定是否视为错误）。</summary>
    public static CodeType Parse(string? codeType)
    {
        return codeType?.ToLowerInvariant() switch
        {
            "pinyin" => CodeType.Pinyin,
            "wubi" or "wubi86" => CodeType.Wubi86,
            "wubi98" => CodeType.Wubi98,
            "wubinage" or "wubi_newage" => CodeType.WubiNewAge,
            "zhengma" => CodeType.Zhengma,
            "cangjie" or "cangjie5" => CodeType.Cangjie5,
            "zhuyin" => CodeType.Zhuyin,
            "terra" or "terra_pinyin" => CodeType.TerraPinyin,
            "userdefine" or "user_define" or "custom" => CodeType.UserDefine,
            _ => CodeType.NoCode
        };
    }

    /// <summary>未显式指定编码类型时，从输出格式推断（self 格式看是否要求显示拼音）。</summary>
    public static CodeType InferFromOutputFormat(string outputFormat, string? customFormat)
    {
        // 自定义格式第 7 位为 'y' 表示要求显示拼音
        if (outputFormat == "self" && !string.IsNullOrEmpty(customFormat) &&
            customFormat.Length > 6 && customFormat[6] == 'y')
            return CodeType.Pinyin;

        return CodeTypeInference.InferFromOutputFormat(outputFormat);
    }
}
