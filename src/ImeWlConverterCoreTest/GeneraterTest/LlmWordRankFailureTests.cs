#nullable enable
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ImeWlConverter.Abstractions.Models;
using ImeWlConverter.Core.WordRank;
using Xunit;

namespace Studyzy.IMEWLConverter.Tests.GeneraterTest;

/// <summary>
/// LlmWordRankGenerator 错误处理表征：
/// LLM 词频生成失败必须显式抛错（带端点/模型上下文），
/// 不得静默降级为默认词频（历史 bug：catch 后返回空字典，用户毫无感知）。
/// </summary>
public class LlmWordRankGeneratorTests
{
    private static LlmWordRankGenerator CreateWithHandler(HttpStatusCode status, string responseBody)
    {
        var handler = new StubHandler(status, responseBody);
        return new LlmWordRankGenerator(
            new LlmConfig
            {
                ApiEndpoint = "https://llm.example.com/v1",
                ApiKey = "test-key",
                Model = "test-model",
            },
            new HttpClient(handler));
    }

    [Fact]
    public async Task GenerateRanks_HttpError_ThrowsWithEndpointContext()
    {
        var generator = CreateWithHandler(HttpStatusCode.InternalServerError, "{}");
        var entries = new List<WordEntry> { new() { Word = "测试", Rank = 0 } };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => generator.GenerateRanksAsync(entries));
        Assert.Contains("llm.example.com", ex.Message);
        Assert.Contains("test-model", ex.Message);
    }

    [Fact]
    public async Task GenerateRanks_Success_AppliesRanks()
    {
        var body = """{"choices":[{"message":{"content":"{\"测试\": 123}"}}]}""";
        var generator = CreateWithHandler(HttpStatusCode.OK, body);
        var entries = new List<WordEntry> { new() { Word = "测试", Rank = 0 } };

        var result = await generator.GenerateRanksAsync(entries);

        Assert.Equal(123, result[0].Rank);
    }

    [Fact]
    public async Task GenerateRanks_EmptyApiKey_SkipsLlm()
    {
        // 无 ApiKey 时不调用 LLM，词条原样返回（不抛错）
        var generator = new LlmWordRankGenerator(
            new LlmConfig { ApiEndpoint = "https://llm.example.com/v1", ApiKey = "" },
            new HttpClient(new StubHandler(HttpStatusCode.InternalServerError, "{}")));
        var entries = new List<WordEntry> { new() { Word = "测试", Rank = 0 } };

        var result = await generator.GenerateRanksAsync(entries);

        Assert.Equal(0, result[0].Rank);
    }

    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
        }
    }
}
