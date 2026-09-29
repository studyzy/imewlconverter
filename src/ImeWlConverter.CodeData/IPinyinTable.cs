namespace ImeWlConverter.CodeData;

/// <summary>
/// 拼音表服务：单字/词组拼音查询与声调处理（不含音调 + 带音调数字后缀两套数据）。
/// </summary>
public interface IPinyinTable
{
    /// <summary>获得一个字的默认拼音（不含声调）。英文字母转小写、数字原样返回。</summary>
    /// <exception cref="KeyNotFoundException">查不到该字的拼音时抛出。</exception>
    string GetDefaultPinyin(char c);

    /// <summary>逐字获得一个词的默认拼音（不含声调），跳过 BMP 之外的字符。</summary>
    IList<string> GetDefaultPinyin(string word);

    /// <summary>获得单个字的所有拼音（不含声调）。</summary>
    IList<string> GetPinyinOfChar(char c);

    /// <summary>判断一个字是否多音字。</summary>
    bool IsMultiPinyinWord(char c);

    /// <summary>给出一个字和一个不含音调的拼音，返回带音调数字后缀的拼音（如 zhong → zhong1）。</summary>
    string AddTone(char c, string barePinyin);

    /// <summary>判断给出的词和逐字拼音是否有效（每个拼音都在对应字的拼音列表中）。</summary>
    bool ValidatePinyin(string word, IList<string> pinyin);
}
