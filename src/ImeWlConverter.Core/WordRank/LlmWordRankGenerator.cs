using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ImeWlConverter.Abstractions.Contracts;
using ImeWlConverter.Abstractions.Models;

namespace ImeWlConverter.Core.WordRank;

/// <summary>
/// Uses an LLM API to generate word frequency ranks.
/// </summary>
public sealed partial class LlmWordRankGenerator : IWordRankGenerator
{
    private readonly HttpClient _httpClient;
    private const int BatchSize = 50;

    private const string SystemPrompt =
        "你是一个语言专家。用户会提供一批词语，请为每个词语提供一个常用的词频评分（1-1000000 之间的整数）。评分越高表示词语越常用。";

    private const string UserPromptTemplate =
        "请为以下词语生成词频评分，仅返回 JSON 格式，Key 是词语，Value 是评分数字：\n{words}";

    private const string RequestBodyTemplate =
        "{{\"model\":{0},\"messages\":[{{\"role\":\"system\",\"content\":{1}}},{{\"role\":\"user\",\"content\":{2}}}],\"temperature\":0.3,\"response_format\":{{\"type\":\"json_object\"}}}}";

    public LlmWordRankGenerator() : this(new LlmConfig(), new HttpClient()) { }

    public LlmWordRankGenerator(LlmConfig config) : this(config, new HttpClient()) { }

    public LlmWordRankGenerator(LlmConfig config, HttpClient httpClient)
    {
        Config = config;
        _httpClient = httpClient;
    }

    public LlmConfig Config { get; set; }
    public bool ForceUse { get; set; }

    public int GenerateRank(WordEntry entry) => 0;

    public async Task<IReadOnlyList<WordEntry>> GenerateRanksAsync(
        IReadOnlyList<WordEntry> entries, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(Config.ApiKey))
            return entries;

        var result = new List<WordEntry>(entries);
        var indicesToRank = new List<int>();
        for (var i = 0; i < entries.Count; i++)
        {
            if (entries[i].Rank == 0 || ForceUse)
                indicesToRank.Add(i);
        }

        for (var i = 0; i < indicesToRank.Count; i += BatchSize)
        {
            ct.ThrowIfCancellationRequested();
            var batchIndices = indicesToRank.Skip(i).Take(BatchSize).ToList();
            var batchEntries = batchIndices.Select(idx => result[idx]).ToList();
            var ranks = await ProcessBatchAsync(batchEntries, ct);
            foreach (var idx in batchIndices)
            {
                if (ranks.TryGetValue(result[idx].Word, out var rank))
                    result[idx] = result[idx] with { Rank = rank };
            }
        }

        return result;
    }

    private async Task<Dictionary<string, int>> ProcessBatchAsync(
        List<WordEntry> batch, CancellationToken ct)
    {
        // LLM 词频失败必须显式失败而非静默降级：吞掉异常会让用户在 LLM 不可用时
        // 拿到全默认词频且毫无感知（历史 bug）。此处包装端点/模型上下文后上抛。
        try
        {
            var wordsString = string.Join("\n", batch.Select(w => w.Word));
            var userPrompt = UserPromptTemplate.Replace("{words}", wordsString);
            var requestBodyJson = BuildRequestBodyJson(userPrompt);

            var endpoint = GetFullApiEndpoint();
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Config.ApiKey);
            request.Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            return ParseRanks(responseJson);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"LLM 词频生成失败（端点: {GetFullApiEndpoint()}，模型: {Config.Model}，批次 {batch.Count} 条）: {ex.Message}", ex);
        }
    }

    internal string BuildRequestBodyJson(string userPrompt)
    {
        var modelJson = EscapeJsonString(Config.Model);
        var systemPromptJson = EscapeJsonString(SystemPrompt);
        var userPromptJson = EscapeJsonString(userPrompt);
        return string.Format(RequestBodyTemplate, modelJson, systemPromptJson, userPromptJson);
    }

    internal static string EscapeJsonString(string? value)
    {
        if (value is null) return "null";
        var sb = new StringBuilder(value.Length + 2);
        sb.Append('"');
        foreach (var c in value)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 0x20)
                    {
                        sb.Append("\\u");
                        sb.Append(((int)c).ToString("x4"));
                    }
                    else sb.Append(c);
                    break;
            }
        }
        sb.Append('"');
        return sb.ToString();
    }

    public string GetFullApiEndpoint()
    {
        var endpoint = Config.ApiEndpoint?.Trim();
        if (string.IsNullOrEmpty(endpoint)) return endpoint ?? "";
        if (endpoint.EndsWith("/v1/chat/completions") || endpoint.EndsWith("/v1/chat/completions/"))
            return endpoint;
        if (endpoint.EndsWith("/v1") || endpoint.EndsWith("/v1/"))
            return endpoint.TrimEnd('/') + "/chat/completions";
        return endpoint.TrimEnd('/') + "/v1/chat/completions";
    }

    public Dictionary<string, int> ParseRanks(string responseJson)
    {
        var result = new Dictionary<string, int>();
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var resultText = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
            if (string.IsNullOrEmpty(resultText)) return result;

            // Try JSON parse first
            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(resultText);
                if (dict != null)
                {
                    foreach (var kv in dict)
                        if (int.TryParse(kv.Value.ToString(), out var rank))
                            result[kv.Key] = rank;
                    return result;
                }
            }
            catch { /* fall through to regex */ }

            // Fallback: regex parse
            var matches = RankPattern().Matches(resultText);
            foreach (Match match in matches)
            {
                var word = match.Groups[1].Value;
                if (int.TryParse(match.Groups[2].Value, out var rank))
                    result[word] = rank;
            }
        }
        catch { /* return empty */ }
        return result;
    }

    public int ParseRank(string responseJson)
    {
        var ranks = ParseRanks(responseJson);
        return ranks.Values.FirstOrDefault();
    }

    [GeneratedRegex(@"""([^""]+)"":\s*(\d+)")]
    private static partial Regex RankPattern();
}
