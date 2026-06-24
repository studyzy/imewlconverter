using ImeWlConverter.Abstractions.Enums;

namespace ImeWlConverter.Abstractions.Options;

/// <summary>Options for code generation during conversion.</summary>
public sealed class CodeGenerationOptions
{
    /// <summary>Target code type to generate. NoCode means skip code generation.</summary>
    public CodeType TargetCodeType { get; init; } = CodeType.NoCode;

    /// <summary>Path to an external character-to-code mapping file (for UserDefine code type).</summary>
    public string? CodeFilePath { get; init; }

    /// <summary>Multi-word code format rules (for UserDefine code type).
    /// Format: "code_e2=p11+p12+p21+p22,code_e3=p11+p21+p31+p32,code_a4=p11+p21+p31+n11"</summary>
    public string? MultiCodeFormat { get; init; }

    /// <summary>Keep English characters in generated code. Default: true.</summary>
    public bool KeepEnglishInCode { get; init; } = true;

    /// <summary>Keep numbers in generated code. Default: true.</summary>
    public bool KeepNumberInCode { get; init; } = true;

    /// <summary>Keep punctuation in generated code. Default: false.</summary>
    public bool KeepPunctuationInCode { get; init; }

    /// <summary>Convert full-width characters to half-width.</summary>
    public bool ConvertFullWidth { get; init; }

    /// <summary>Translate numbers to Chinese characters in code.</summary>
    public bool TranslateNumbersToChinese { get; init; }

    /// <summary>Prefix English words with underscore in code.</summary>
    public bool PrefixEnglishWithUnderscore { get; init; }
}
