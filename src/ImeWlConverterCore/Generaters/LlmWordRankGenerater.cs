using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Studyzy.IMEWLConverter.Entities;

namespace Studyzy.IMEWLConverter.Generaters;

public class LlmWordRankGenerater : IWordRankGenerater
{
    private static readonly HttpClient client = new HttpClient();
    private const int BatchSize = 50;

    private const string SystemPrompt = "你是一个语言专家。用户会提供一批词语，请为每个词语提供一个常用的词频评分（1-1000000 之间的整数）。评分越高表示词语越常用。";
    private const string UserPromptTemplate = "请为以下词语生成词频评分，仅返回 JSON 格式，Key 是词语，Value 是评分数字：\n{words}";

    public LlmWordRankGenerater()
    {
    }

    public LlmWordRankGenerater(LlmConfig config)
    {
        Config = config;
    }

    public LlmConfig Config { get; set; } = new LlmConfig();

    public bool ForceUse { get; set; }

    public int GetRank(string word)
    {
        // 保持单词查询接口，但在批量调用时不再使用它
        var list = new WordLibraryList { new WordLibrary { Word = word } };
        GenerateRank(list);
        return list[0].Rank;
    }

    public void GenerateRank(WordLibraryList wordLibraryList, Action<int, int> progressCallback = null)
    {
        if (string.IsNullOrWhiteSpace(Config.ApiKey))
        {
            return;
        }

        var wordsToRank = wordLibraryList.Where(w => w.Rank == 0 || ForceUse).ToList();
        int processedCount = 0;
        int totalToProcess = wordsToRank.Count;

        for (int i = 0; i < totalToProcess; i += BatchSize)
        {
            var batch = wordsToRank.Skip(i).Take(BatchSize).ToList();
            ProcessBatch(batch);
            processedCount += batch.Count;
            progressCallback?.Invoke(processedCount, totalToProcess);
        }
    }

    private void ProcessBatch(List<WordLibrary> batch)
    {
        try
        {
            var wordsString = string.Join("\n", batch.Select(w => w.Word));
            var userPrompt = UserPromptTemplate.Replace("{words}", wordsString);

            var requestBody = new
            {
                model = Config.Model,
                messages = new[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.3,
                response_format = new { type = "json_object" } // 要求返回 JSON
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, Config.ApiEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Config.ApiKey);
            request.Content = content;

            var response = client.Send(request);
            response.EnsureSuccessStatusCode();

            var responseJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var ranks = ParseRanks(responseJson);

            foreach (var wordLibrary in batch)
            {
                if (ranks.TryGetValue(wordLibrary.Word, out var rank))
                {
                    wordLibrary.Rank = rank;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LLM Batch Process Error: {ex.Message}");
        }
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

            if (string.IsNullOrEmpty(resultText))
            {
                return result;
            }

            // 尝试直接解析 JSON
            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(resultText);
                if (dict != null)
                {
                    foreach (var kv in dict)
                    {
                        if (int.TryParse(kv.Value.ToString(), out var rank))
                        {
                            result[kv.Key] = rank;
                        }
                    }
                    return result;
                }
            }
            catch
            {
                // 解析 JSON 失败，回退到正则
            }

            // 使用正则提取所有 "key": value 模式
            var matches = Regex.Matches(resultText, @"""([^""]+)"":\s*(\d+)");
            foreach (Match match in matches)
            {
                var word = match.Groups[1].Value;
                if (int.TryParse(match.Groups[2].Value, out var rank))
                {
                    result[word] = rank;
                }
            }
        }
        catch
        {
            // Ignore
        }
        return result;
    }

    public int ParseRank(string responseJson)
    {
        var ranks = ParseRanks(responseJson);
        return ranks.Values.FirstOrDefault();
    }
}
