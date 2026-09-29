namespace ImeWlConverter.Application.Cli;

/// <summary>
/// CLI 退出码契约（对 AI 代理与脚本的公共接口，勿随意改动；变更需同步 MIGRATION.md）。
/// </summary>
public static class ExitCodes
{
    /// <summary>0 — 成功。</summary>
    public const int Success = 0;

    /// <summary>1 — 用法/参数错误（缺必填项、未知格式 ID、filter/spec 语法错）。</summary>
    public const int UsageError = 1;

    /// <summary>2 — 输入错误（文件不存在/不可读、转换失败）。</summary>
    public const int InputError = 2;

    /// <summary>3 — 部分失败（≥1 文件成功、≥1 文件失败，详见 Errors）。</summary>
    public const int PartialSuccess = 3;

    /// <summary>4 — 未捕获内部错误（默认不输出堆栈，IMEWL_DEBUG=1 时输出）。</summary>
    public const int InternalError = 4;
}
