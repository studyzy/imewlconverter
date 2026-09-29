using ImeWlConverter.Abstractions.Contracts;

namespace ImeWlConverter.Application.Cli.Output;

/// <summary>
/// --list-formats 输出（人类可读 / JSON 双模式）。文本模式与历史输出一致。
/// </summary>
public static class FormatListWriter
{
    public static void WriteHuman(
        IReadOnlyList<IFormatImporter> importers, IReadOnlyList<IFormatExporter> exporters)
    {
        Console.WriteLine("支持的输入格式：");
        foreach (var imp in importers.OrderBy(i => i.Metadata.SortOrder))
            Console.WriteLine($"  {imp.Metadata.Id,-15} {imp.Metadata.DisplayName}");

        Console.WriteLine();
        Console.WriteLine("支持的输出格式：");
        foreach (var exp in exporters.OrderBy(e => e.Metadata.SortOrder))
            Console.WriteLine($"  {exp.Metadata.Id,-15} {exp.Metadata.DisplayName}");
    }

    public static void WriteJson(
        IReadOnlyList<IFormatImporter> importers, IReadOnlyList<IFormatExporter> exporters)
    {
        var payload = new
        {
            schema = 1,
            ok = true,
            importFormats = importers.OrderBy(i => i.Metadata.SortOrder).Select(i => new
            {
                id = i.Metadata.Id,
                name = i.Metadata.DisplayName,
                isBinary = i.Metadata.IsBinary,
                extension = i.Metadata.FileExtension,
            }),
            exportFormats = exporters.OrderBy(e => e.Metadata.SortOrder).Select(e => new
            {
                id = e.Metadata.Id,
                name = e.Metadata.DisplayName,
                isBinary = e.Metadata.IsBinary,
                extension = e.Metadata.FileExtension,
            }),
        };

        Console.Out.WriteLine(System.Text.Json.JsonSerializer.Serialize(payload, JsonOptions()));
    }

    private static System.Text.Json.JsonSerializerOptions JsonOptions() => new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
    };
}
