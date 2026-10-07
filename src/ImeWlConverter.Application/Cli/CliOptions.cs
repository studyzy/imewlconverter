using System.CommandLine;

namespace ImeWlConverter.Application.Cli;

/// <summary>
/// System.CommandLine 选项/参数定义（仅定义，不含处理逻辑）。
/// </summary>
public static class CliOptions
{
    public static readonly Option<string> InputFormat = new(
        aliases: new[] { "--input-format", "-i" },
        description: "输入词库格式代码 (例如: scel, ggpy, qqpy, rime, bdpy)")
    { IsRequired = false };

    public static readonly Option<string> OutputFormat = new(
        aliases: new[] { "--output-format", "-o" },
        description: "输出词库格式代码 (例如: ggpy, rime, self, qqpy)")
    { IsRequired = false };

    public static readonly Option<string> OutputPath = new(
        aliases: new[] { "--output", "-O" },
        description: "输出文件路径或目录路径（目录路径以 / 结尾）")
    { IsRequired = false };

    public static readonly Argument<List<string>> InputFiles = new(
        name: "input-files",
        description: "输入词库文件路径（支持多个文件和通配符）")
    { Arity = ArgumentArity.ZeroOrMore };

    public static readonly Option<string?> Filter = new(
        aliases: new[] { "--filter", "-f" },
        description: "过滤条件 (例如: \"len:1-100|rm:eng|rm:num\")\n" +
                    "  len:1-100    - 保留字数 1-100 的词条\n" +
                    "  rank:2-9999  - 保留词频 2-9999 的词条\n" +
                    "  rm:eng       - 移除包含英文的词条\n" +
                    "  rm:num       - 移除包含数字的词条\n" +
                    "  rm:space     - 移除包含空格的词条\n" +
                    "  rm:pun       - 移除包含标点的词条");

    public static readonly Option<string?> CustomFormat = new(
        aliases: new[] { "--custom-format", "-F" },
        description: "自定义格式配置 (用于 self 格式)\n" +
                    "  格式: <顺序><拼音分隔符><字段分隔符><位置><显示>\n" +
                    "  示例: \"213 ,nyyy\" = 词语,拼音(空格分隔),词频");

    public static readonly Option<string?> CodeType = new(
        aliases: new[] { "--code-type", "-t" },
        description: "编码类型 (pinyin=拼音, wubi=五笔, zhengma=郑码, cangjie=仓颉, zhuyin=注音, userdefine=自定义)");

    public static readonly Option<string?> CodeFile = new(
        aliases: new[] { "--code-file", "-c" },
        description: "自定义编码映射表文件路径（Tab分隔，格式：汉字\\t编码）");

    public static readonly Option<string?> MultiCode = new(
        aliases: new[] { "--multi-code", "-m" },
        description: "多字词编码规则（逗号分隔）\n" +
                    "  示例: \"code_e2=p11+p12+p21+p22,code_e3=p11+p21+p31+p32,code_a4=p11+p21+p31+n11\"");

    public static readonly Option<string?> RankGenerator = new(
        aliases: new[] { "--rank-generator", "-r" },
        description: "词频生成器：指定固定词频数字，强制覆盖所有词条的词频\n" +
                    "  示例: -r 100（所有词条词频设为 100）");

    public static readonly Option<bool> ListFormats = new(
        aliases: new[] { "--list-formats" },
        description: "显示所有支持的输入法格式列表");

    public static readonly Option<bool> Json = new(
        aliases: new[] { "--json" },
        description: "以 JSON 格式输出结果（机器可读；进度不写 stderr，除非同时指定 --verbose）\n" +
                    "退出码契约: 0=成功 1=用法错误 2=输入错误 3=部分失败 4=内部错误");

    public static readonly Option<bool> Verbose = new(
        aliases: new[] { "--verbose" },
        description: "输出详细日志（--json 模式下同时显示进度）");

    public static readonly Option<string?> DictId = new(
        aliases: new[] { "--dict-id" },
        description: "导出词库编号（scel 格式内嵌的文件ID，最多6字符，默认随机生成）");

    public static readonly Option<string?> DictName = new(
        aliases: new[] { "--dict-name" },
        description: "导出词库名称（scel 格式内嵌的元数据，默认: 深蓝词库转换）");

    public static readonly Option<string?> DictCategory = new(
        aliases: new[] { "--dict-category" },
        description: "导出词库类别（scel 格式内嵌的元数据，默认: 自定义）");

    public static readonly Option<string?> DictDescription = new(
        aliases: new[] { "--dict-description" },
        description: "导出词库描述（scel 格式内嵌的元数据）");

    /// <summary>把全部选项/参数挂到根命令。</summary>
    public static void AddTo(RootCommand rootCommand)
    {
        rootCommand.AddOption(InputFormat);
        rootCommand.AddOption(OutputFormat);
        rootCommand.AddOption(OutputPath);
        rootCommand.AddArgument(InputFiles);
        rootCommand.AddOption(Filter);
        rootCommand.AddOption(CustomFormat);
        rootCommand.AddOption(CodeType);
        rootCommand.AddOption(CodeFile);
        rootCommand.AddOption(MultiCode);
        rootCommand.AddOption(RankGenerator);
        rootCommand.AddOption(ListFormats);
        rootCommand.AddOption(Json);
        rootCommand.AddOption(Verbose);
        rootCommand.AddOption(DictId);
        rootCommand.AddOption(DictName);
        rootCommand.AddOption(DictCategory);
        rootCommand.AddOption(DictDescription);
    }
}
