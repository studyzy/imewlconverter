namespace ImeWlConverter.CodeData;

/// <summary>
/// 超音（超音速写）码表服务（源自 ChaoyinCodeMapping.txt）。
/// </summary>
public interface IChaoyinTable
{
    /// <summary>获得一个拼音（可含音调数字后缀）对应的超音编码；查不到返回 null。</summary>
    string? GetChaoyin(string pinyin);

    /// <summary>按超音取码规则获得一个词（逐字拼音列表）的超音编码。</summary>
    string GetWordChaoyin(IList<string> pinyins);
}
