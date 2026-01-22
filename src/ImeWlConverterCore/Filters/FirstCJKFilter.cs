using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Filters;

/// <summary>
///     过滤首字符非中日韩的词（实际上非常粗糙）
/// </summary>
public class FirstCJKFilter : ISingleFilter
{
    public bool ReplaceAfterCode => false;

    public bool IsKeep(WordLibrary wl)
    {
        if (string.IsNullOrEmpty(wl.Word))
            return false;

        // Use StringInfo to properly handle surrogate pairs (characters beyond BMP)
        var si = new System.Globalization.StringInfo(wl.Word);
        if (si.LengthInTextElements == 0)
            return false;

        var firstElement = si.SubstringByTextElements(0, 1);

        // Handle surrogate pairs (characters beyond BMP, like CJK Extension B-F)
        if (firstElement.Length == 2 && char.IsSurrogatePair(firstElement, 0))
        {
            // Get the Unicode code point
            var codePoint = char.ConvertToUtf32(firstElement, 0);
            // Check if it's in CJK Extension B-F range (U+20000 to U+2FFFF)
            if (codePoint >= 0x20000 && codePoint <= 0x2FFFF)
                return true;
        }

        // Check BMP CJK characters
        var c = firstElement[0];
        return c >= 0x2e80 && c <= 0x9fff;
    }
}
