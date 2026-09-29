using System.Text.Json;
using System.Text.Json.Serialization;
using ImeWlConverter.Abstractions.Contracts;

namespace ImeWlConverter.Application.Cli.Output;

/// <summary>
/// --json 机器可读输出。
/// 契约（schema=1）：
///   成功: { "schema":1, "ok":true,  "result": { imported, exported, filtered, outputs[], errors[] } }
///   失败: { "schema":1, "ok":false, "error": { code, target?, message } }
/// error.code ∈ missing-option|unknown-format|invalid-filter|invalid-spec|input-not-found|conversion-failed|internal-error
/// </summary>
public static class JsonOutputWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
    };

    public static void WriteSuccess(ConversionResult result, string? outputPath)
    {
        var payload = new
        {
            schema = 1,
            ok = true,
            result = new
            {
                imported = result.ImportedCount,
                exported = result.ExportedCount,
                filtered = result.FilteredCount,
                outputs = outputPath is null ? Array.Empty<object>() : new[] { new { path = outputPath, entries = result.ExportedCount } },
                errors = result.Errors.Select(e => new { file = e.FilePath, message = e.Message, exceptionType = e.ExceptionType })
                    .ToList(),
            },
        };
        Console.Out.WriteLine(JsonSerializer.Serialize(payload, Options));
    }

    public static void WriteError(CliError error)
    {
        var payload = new
        {
            schema = 1,
            ok = false,
            error = new
            {
                code = error.Code,
                target = error.Target,
                message = error.Message,
            },
        };
        Console.Out.WriteLine(JsonSerializer.Serialize(payload, Options));
    }

    public static void WriteInternalError(string message)
    {
        WriteError(new CliError(CliError.InternalError, message));
    }
}
