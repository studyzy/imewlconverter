namespace ImeWlConverter.Formats.LibIMEText;

using System.Text;

/// <summary>
/// fcitx5 <c>fcitx::stringutils</c> 中 <c>escapeForValue</c> / <c>consumeMaybeEscapedValue</c> 的 C# 移植。
///
/// libime 文本词库（<c>loadText</c> / <c>saveText</c>）用它们读写词面，
/// 因此词面中若含空白、引号或反斜杠会被引号包裹并按 fcitx 规则转义。
/// 参考 fcitx5 <c>src/lib/fcitx-utils/stringutils.cpp</c> 与 <c>macros.h</c> 的 <c>FCITX_WHITESPACE</c>。
/// </summary>
internal static class LibimeTextEscaping
{
    /// <summary>fcitx <c>FCITX_WHITESPACE</c>：需要在值中转义、并作为 token 分隔符的空白字符。</summary>
    private const string Whitespace = "\f\n\r\t\v ";

    /// <summary>需要引号包裹的字符：全部 <see cref="Whitespace"/> 加上引号与反斜杠。</summary>
    private static readonly char[] NeedEscape = ['\f', '\r', '\t', '\v', ' ', '"', '\\', '\n'];

    /// <summary>
    /// 对应 fcitx <c>escapeForValue</c>：若字符串含空白/引号/反斜杠则用引号包裹并转义。
    /// </summary>
    public static string EscapeValue(string str)
    {
        var needEscape = str.IndexOfAny(NeedEscape) >= 0;
        var sb = new StringBuilder(str.Length + 2);
        if (needEscape)
            sb.Append('"');

        foreach (var c in str)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '"': sb.Append("\\\""); break;
                case '\n': sb.Append("\\n"); break;
                case '\f': sb.Append("\\f"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                case '\v': sb.Append("\\v"); break;
                default: sb.Append(c); break;
            }
        }

        if (needEscape)
            sb.Append('"');
        return sb.ToString();
    }

    /// <summary>
    /// 按 libime <c>loadTextImpl</c> 的方式把一行切分为若干 token：
    /// 反复调用 <see cref="ConsumeMaybeEscapedValue"/> 直到剩余输入为空。
    /// </summary>
    public static List<string> Tokenize(string line)
    {
        var tokens = new List<string>();
        var input = line.AsSpan();
        while (!input.IsEmpty)
        {
            var consumed = ConsumeMaybeEscapedValue(ref input, out var value);
            // 与 libime 一致：只有真正消费到内容（可能解码为空串）才计入 token。
            if (consumed > 0)
                tokens.Add(value);
        }

        return tokens;
    }

    /// <summary>
    /// 对应 fcitx <c>consumeMaybeEscapedValue(input, FCITX_WHITESPACE, &amp;output)</c>：
    /// 跳过前导空白；若以引号开头则读到下一个未转义的引号（并按转义表还原），
    /// 否则读到下一个空白。返回本次消费的字符数。
    /// </summary>
    private static int ConsumeMaybeEscapedValue(ref ReadOnlySpan<char> input, out string value)
    {
        var start = FindFirstNotOf(input, Whitespace);
        if (start < 0)
        {
            input = ReadOnlySpan<char>.Empty;
            value = "";
            return 0;
        }

        input = input[start..];

        if (input[0] == '"')
        {
            var sb = new StringBuilder();
            var escaping = false;
            var end = 0;
            for (var i = 1; i < input.Length; i++)
            {
                var c = input[i];
                if (!escaping)
                {
                    if (c == '\\')
                        escaping = true;
                    else if (c == '"')
                        end = i + 1;
                    else
                        sb.Append(c);
                }
                else
                {
                    // 非法转义序列按普通字符处理（与 fcitx 一致）。
                    sb.Append(Unescape(c));
                    escaping = false;
                }

                if (end != 0)
                    break;
            }

            if (end > 0)
            {
                var consumed = end;
                input = input[consumed..];
                value = sb.ToString();
                return consumed;
            }
        }

        // 未加引号，或引号没有闭合：读到下一个空白。
        var stop = FindFirstOf(input, Whitespace, 1);
        var unquoted = stop < 0 ? input : input[..stop];
        input = stop < 0 ? ReadOnlySpan<char>.Empty : input[stop..];
        value = unquoted.ToString();
        return unquoted.Length;
    }

    private static char Unescape(char c) => c switch
    {
        '\\' => '\\',
        '"' => '"',
        'n' => '\n',
        'f' => '\f',
        'r' => '\r',
        't' => '\t',
        'v' => '\v',
        _ => c,
    };

    private static int FindFirstNotOf(ReadOnlySpan<char> input, string set)
    {
        for (var i = 0; i < input.Length; i++)
            if (!set.Contains(input[i]))
                return i;
        return -1;
    }

    private static int FindFirstOf(ReadOnlySpan<char> input, string set, int start)
    {
        for (var i = start; i < input.Length; i++)
            if (set.Contains(input[i]))
                return i;
        return -1;
    }
}
