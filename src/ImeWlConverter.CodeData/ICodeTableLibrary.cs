namespace ImeWlConverter.CodeData;

/// <summary>
/// 单字全码表（源自 ChineseCode.txt）：字 → 五笔86/98/新世纪、拼音串、词频。
/// </summary>
public interface ICodeTableLibrary
{
    /// <summary>获取单字的完整编码信息，未收录的字抛出 KeyNotFoundException。</summary>
    ChineseCode GetCode(char c);

    /// <summary>获取全部已收录的单字编码。</summary>
    IReadOnlyCollection<ChineseCode> GetAll();
}
