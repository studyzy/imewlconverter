namespace ImeWlConverter.CodeData;

/// <summary>
/// Represents the encoding information for a single Chinese character.
/// </summary>
public readonly record struct ChineseCode(
    string Code,
    char Word,
    string Wubi86,
    string Wubi98,
    string WubiNewAge,
    string Pinyins,
    double Freq
);
