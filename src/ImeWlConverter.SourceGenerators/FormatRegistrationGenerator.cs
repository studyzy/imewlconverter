namespace ImeWlConverter.SourceGenerators;

using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class FormatRegistrationGenerator : IIncrementalGenerator
{
    private const string ImporterInterface = "ImeWlConverter.Abstractions.Contracts.IFormatImporter";
    private const string ExporterInterface = "ImeWlConverter.Abstractions.Contracts.IFormatExporter";
    private const string BinaryImporterBase = "BinaryFormatImporter";
    private const string TextImporterBase = "TextFormatImporter";
    private const string TextExporterBase = "TextFormatExporter";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var formatClasses = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "ImeWlConverter.Abstractions.FormatPluginAttribute",
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => GetFormatInfo(ctx))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!.Value);

        var collected = formatClasses.Collect();

        context.RegisterSourceOutput(collected, static (spc, formats) =>
        {
            if (formats.IsEmpty) return;

            // Generate DI registration
            spc.AddSource("FormatRegistry.g.cs",
                SourceText.From(GenerateRegistrationCode(formats), Encoding.UTF8));

            // Generate Metadata property for each format class
            foreach (var format in formats)
            {
                var source = GenerateMetadataCode(format);
                var hintName = format.ClassName + ".Metadata.g.cs";
                spc.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
            }
        });
    }

    private static FormatInfo? GetFormatInfo(GeneratorAttributeSyntaxContext context)
    {
        var symbol = context.TargetSymbol as INamedTypeSymbol;
        if (symbol == null) return null;

        var attr = context.Attributes.FirstOrDefault();
        if (attr == null) return null;

        // Read constructor arguments
        var id = attr.ConstructorArguments.Length > 0
            ? attr.ConstructorArguments[0].Value?.ToString() ?? ""
            : "";
        var displayName = attr.ConstructorArguments.Length > 1
            ? attr.ConstructorArguments[1].Value?.ToString() ?? ""
            : "";
        var sortOrder = attr.ConstructorArguments.Length > 2
            ? (int)(attr.ConstructorArguments[2].Value ?? 100)
            : 100;

        // Read named arguments (IsBinary, FileExtension)
        bool? isBinaryExplicit = null;
        string fileExtension = null;
        foreach (var named in attr.NamedArguments)
        {
            if (named.Key == "IsBinary")
                isBinaryExplicit = (bool?)named.Value.Value;
            else if (named.Key == "FileExtension")
                fileExtension = named.Value.Value?.ToString();
        }

        // Detect interfaces
        var isImporter = symbol.AllInterfaces.Any(i => i.ToDisplayString() == ImporterInterface);
        var isExporter = symbol.AllInterfaces.Any(i => i.ToDisplayString() == ExporterInterface);

        // Detect IsBinary from base class chain
        var isBinary = isBinaryExplicit ?? InheritsFromBinaryImporter(symbol);

        // Detect if base class has abstract Metadata (needs override keyword)
        var needsOverride = InheritsFromAbstractBase(symbol);

        // Get namespace
        var ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? ""
            : symbol.ContainingNamespace.ToDisplayString();

        return new FormatInfo(
            symbol.ToDisplayString(),
            symbol.Name,
            ns,
            id,
            displayName,
            sortOrder,
            isImporter,
            isExporter,
            isBinary,
            needsOverride,
            fileExtension ?? ".txt");
    }

    private static bool InheritsFromBinaryImporter(INamedTypeSymbol symbol)
    {
        var current = symbol.BaseType;
        while (current != null)
        {
            if (current.Name == BinaryImporterBase)
                return true;
            current = current.BaseType;
        }
        return false;
    }

    private static bool InheritsFromAbstractBase(INamedTypeSymbol symbol)
    {
        var current = symbol.BaseType;
        while (current != null)
        {
            if (current.Name == TextImporterBase ||
                current.Name == TextExporterBase ||
                current.Name == BinaryImporterBase)
                return true;
            current = current.BaseType;
        }
        return false;
    }

    private static string GenerateRegistrationCode(ImmutableArray<FormatInfo> formats)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using ImeWlConverter.Abstractions.Contracts;");
        sb.AppendLine();
        sb.AppendLine("namespace ImeWlConverter.Formats;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Auto-generated format registration. Discovered " + formats.Length + " format plugins.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class FormatRegistry");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>Register all discovered format importers and exporters.</summary>");
        sb.AppendLine("    public static IServiceCollection AddAllFormats(this IServiceCollection services)");
        sb.AppendLine("    {");

        foreach (var format in formats)
        {
            if (format.IsImporter)
                sb.AppendLine($"        services.AddSingleton<IFormatImporter, {format.FullTypeName}>();");
            if (format.IsExporter)
                sb.AppendLine($"        services.AddSingleton<IFormatExporter, {format.FullTypeName}>();");
        }

        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateMetadataCode(FormatInfo format)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(format.Namespace))
        {
            sb.AppendLine($"namespace {format.Namespace};");
            sb.AppendLine();
        }

        var modifier = format.NeedsOverride ? "override " : "";
        sb.AppendLine($"partial class {format.ClassName}");
        sb.AppendLine("{");
        sb.AppendLine($"    public {modifier}ImeWlConverter.Abstractions.Models.FormatMetadata Metadata {{ get; }} =");
        sb.AppendLine($"        new(\"{Escape(format.Id)}\", \"{Escape(format.DisplayName)}\", {format.SortOrder}, " +
                      $"SupportsImport: {Bool(format.IsImporter)}, SupportsExport: {Bool(format.IsExporter)}, " +
                      $"IsBinary: {Bool(format.IsBinary)}, FileExtension: \"{Escape(format.FileExtension)}\");");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string Bool(bool value) => value ? "true" : "false";

    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private readonly struct FormatInfo
    {
        public string FullTypeName { get; }
        public string ClassName { get; }
        public string Namespace { get; }
        public string Id { get; }
        public string DisplayName { get; }
        public int SortOrder { get; }
        public bool IsImporter { get; }
        public bool IsExporter { get; }
        public bool IsBinary { get; }
        public bool NeedsOverride { get; }
        public string FileExtension { get; }

        public FormatInfo(string fullTypeName, string className, string ns,
            string id, string displayName, int sortOrder,
            bool isImporter, bool isExporter, bool isBinary, bool needsOverride,
            string fileExtension)
        {
            FullTypeName = fullTypeName;
            ClassName = className;
            Namespace = ns;
            Id = id;
            DisplayName = displayName;
            SortOrder = sortOrder;
            IsImporter = isImporter;
            IsExporter = isExporter;
            IsBinary = isBinary;
            NeedsOverride = needsOverride;
            FileExtension = fileExtension;
        }
    }
}
