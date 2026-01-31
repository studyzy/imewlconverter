## 上下文

### 当前状态

IME WL Converter 的命令行工具 (`ImeWlConverterCmd`) 使用自定义的参数解析方式：

**代码层面**：
- `ConsoleRun.cs` (502 行) 包含手工参数解析逻辑
- `RunCommand()` 方法使用 `command.StartsWith("-x:")` 进行字符串匹配
- 每个参数都需要单独的 if 语句处理（179-408 行）
- 参数验证分散在各处，缺乏统一的错误处理
- 没有使用现代命令行解析库

**用户体验层面**：
- 使用非标准格式：`-i:scel` 而不是 `-i scel`
- 帮助信息手工维护，冗长且格式不一致
- 错误信息不够清晰
- 难以与标准 shell 工具集成（如自动补全）

### 约束

1. **兼容性约束**：
   - 必须保持 .NET 10.0 目标框架
   - 必须跨平台兼容（Windows, Linux, macOS）
   - 必须支持单文件发布和代码裁剪（PublishTrimmed）

2. **业务逻辑约束**：
   - 不改变核心转换逻辑（`MainBody.cs`、各种 Import/Export 类）
   - 保持现有的过滤器、生成器、编码系统不变
   - 保持对 20+ 种输入法格式的支持

3. **测试约束**：
   - 现有的 3 个集成测试套件必须更新
   - 单元测试（`ImeWlConverterCoreTest`）不受影响，因为它们测试核心逻辑而非 CLI

### 利益相关者

- **最终用户**：希望获得标准的 CLI 体验
- **开发者**：希望代码更易维护，更少的字符串操作
- **CI/CD 系统**：需要更新自动化脚本
- **文档维护者**：需要更新所有文档示例

## 目标 / 非目标

### 目标

1. **用户体验**：
   - 提供符合 GNU/POSIX 标准的命令行界面
   - 自动生成格式化的帮助信息
   - 清晰、可操作的错误信息
   - 支持参数自动补全（未来可能）

2. **代码质量**：
   - 移除手工字符串解析逻辑
   - 使用强类型的参数对象
   - 统一的参数验证和错误处理
   - 减少 `ConsoleRun.cs` 的复杂度

3. **可维护性**：
   - 使用成熟的命令行解析库
   - 声明式的参数定义
   - 易于添加新参数

### 非目标

1. **不改变核心业务逻辑**：
   - `MainBody` 类的转换流程保持不变
   - Import/Export 接口和实现保持不变
   - 过滤器、生成器系统保持不变

2. **不提供向后兼容模式**：
   - 完全切换到新格式，不支持旧格式并行运行
   - 检测到旧格式时仅提供迁移提示

3. **不改变发布方式**：
   - 继续支持单文件发布
   - 不改变跨平台构建流程

## 决策

### 决策 1：选择 System.CommandLine 库

**选择**：使用 `System.CommandLine` (Microsoft 官方库)

**理由**：
- ✅ Microsoft 官方维护，与 .NET 生态系统深度集成
- ✅ 现代 API 设计，支持强类型绑定
- ✅ 自动生成帮助信息和错误提示
- ✅ 支持子命令（未来可能需要）
- ✅ 良好的文档和社区支持
- ✅ 支持 .NET 10.0 和代码裁剪

**考虑过的替代方案**：

1. **CommandLineParser (Commandline/CommandLineParser)**
   - ❌ 基于属性的声明式风格，不够灵活
   - ❌ 帮助信息格式化能力较弱
   - ✅ 更成熟，使用广泛
   - **结论**：功能足够但不如 System.CommandLine 现代化

2. **手工解析保持现状**
   - ❌ 维护成本高
   - ❌ 功能有限，难以扩展
   - ❌ 错误处理不一致
   - **结论**：不符合重构目标

### 决策 2：参数结构设计

**选择**：使用强类型的 Options 类 + RootCommand 模式

**架构**：
```csharp
// 新文件：CommandLineOptions.cs
public class CommandLineOptions
{
    public string InputFormat { get; set; }
    public string OutputFormat { get; set; }
    public string OutputPath { get; set; }
    public List<string> InputFiles { get; set; }
    public string CodeFile { get; set; }
    public string Filter { get; set; }
    public string CustomFormat { get; set; }
    public string RankGenerator { get; set; }
    public string MultiCode { get; set; }
    public string CodeType { get; set; }
    public string TargetOS { get; set; }
    public string Ld2Encoding { get; set; }
}

// 新文件：CommandBuilder.cs
public class CommandBuilder
{
    public static RootCommand Build()
    {
        var rootCommand = new RootCommand("IME WL Converter - Dictionary converter for Input Method Editors");

        // 定义所有选项
        var inputFormatOption = new Option<string>(
            aliases: new[] { "--input-format", "-i" },
            description: "Input dictionary format (e.g., scel, ggpy, qqpy)"
        ) { IsRequired = true };

        // ... 其他选项定义

        rootCommand.AddOption(inputFormatOption);
        // ...

        rootCommand.SetHandler((CommandLineOptions opts) => {
            // 调用现有的转换逻辑
        });

        return rootCommand;
    }
}
```

**理由**：
- ✅ 清晰分离参数定义和业务逻辑
- ✅ 易于测试（可以单独测试参数解析）
- ✅ 自动类型转换和验证
- ✅ IDE 友好（强类型支持智能提示）

### 决策 3：保留 ConsoleRun 类但重构其职责

**选择**：将 `ConsoleRun.cs` 从"参数解析器"转变为"业务协调器"

**重构方案**：

**Before** (当前 502 行)：
```csharp
public class ConsoleRun
{
    // 参数解析 (179-408 行)
    private CommandType RunCommand(string command) { ... }

    // 业务逻辑协调 (128-157 行)
    public void Run() { ... }

    // 格式查找
    private IWordLibraryImport GetImportInterface(string format) { ... }
    private IWordLibraryExport GetExportInterface(string format) { ... }
}
```

**After** (预计 ~250 行)：
```csharp
public class ConversionCoordinator  // 重命名或保持 ConsoleRun
{
    // 移除所有参数解析代码

    // 保留业务协调逻辑
    public void Execute(CommandLineOptions options)
    {
        // 验证选项组合
        ValidateOptions(options);

        // 设置 Import/Export
        var import = GetImportInterface(options.InputFormat);
        var export = GetExportInterface(options.OutputFormat);

        // 设置过滤器
        ConfigureFilters(options.Filter);

        // 执行转换
        PerformConversion(import, export, options);
    }

    // 保留格式查找逻辑（已有，不变）
    private IWordLibraryImport GetImportInterface(string format) { ... }
    private IWordLibraryExport GetExportInterface(string format) { ... }
}
```

**理由**：
- ✅ 职责单一：只负责协调业务逻辑
- ✅ 代码行数减少约 50%
- ✅ 保持向现有 `MainBody` 类的调用方式
- ✅ 易于测试（可以直接传入 Options 对象）

### 决策 4：错误处理策略

**选择**：分层错误处理

1. **参数验证层** (System.CommandLine 自动处理)：
   - 必填参数检查
   - 参数类型验证
   - 自动显示错误和用法提示

2. **业务验证层** (ConversionCoordinator)：
   - 格式代码有效性检查
   - 选项组合验证（如 `-o self` 需要 `-F`）
   - 文件存在性检查

3. **转换执行层** (MainBody，保持现状)：
   - 文件解析错误
   - 转换逻辑错误

**错误信息格式**：
```
Error: Unknown input format 'invalid'

Supported formats:
  scel    - Sougou Pinyin (.scel)
  ggpy    - Google Pinyin
  qqpy    - QQ Pinyin
  ...

Usage:
  imewlconverter --input-format <format> [options] <INPUT_FILES>...

For more information, run:
  imewlconverter --help
```

### 决策 5：迁移策略

**选择**：Big Bang 迁移 + 检测提示

**实施方案**：
1. 不提供双模式支持（新旧格式并存）
2. 在检测到旧格式时提供清晰的错误信息和迁移指引
3. 提供迁移文档（MIGRATION.md）

**检测逻辑**：
```csharp
// 在 Program.cs 中早期检测
if (args.Any(a => a.Contains(":")))
{
    Console.WriteLine("Error: Old parameter format detected.");
    Console.WriteLine();
    Console.WriteLine("The parameter format has changed. Please update your command:");
    Console.WriteLine("  Old: -i:scel input.scel -o:ggpy output.txt");
    Console.WriteLine("  New: -i scel -o ggpy -O output.txt input.scel");
    Console.WriteLine();
    Console.WriteLine("See MIGRATION.md for complete guide.");
    return 1;
}
```

**理由**：
- ✅ 实施简单，不需要维护两套解析逻辑
- ✅ 强制用户迁移，避免长期技术债
- ✅ 明确的迁移提示降低用户困惑
- ❌ 短期内可能影响现有自动化脚本（但有清晰提示）

### 决策 6：帮助信息设计

**选择**：分层帮助系统

1. **简短用法** (`--help`)：
   - Synopsis
   - 选项列表（带简短描述）
   - 常用示例（3-5 个）

2. **格式列表** (`--list-formats`)：
   - 所有支持的输入/输出格式
   - 格式代码和完整名称对照

3. **详细示例** (在帮助信息的 EXAMPLES 部分)：
   - 基本转换
   - 批量转换
   - 使用过滤器
   - 自定义编码

**实现**：
```csharp
rootCommand.AddOption(new Option<bool>(
    "--list-formats",
    "Show all supported dictionary formats"
));

// System.CommandLine 会自动处理 --help
```

## 风险 / 权衡

### 风险 1：破坏现有自动化脚本
**影响**：用户的脚本、CI/CD 流程需要更新
**缓解措施**：
- 提供详细的迁移文档（MIGRATION.md）
- 在错误信息中显示新旧格式对照
- 在发布说明中突出标记 BREAKING CHANGE
- 考虑提供转换脚本帮助迁移集成测试

### 风险 2：System.CommandLine 库的体积
**影响**：可能增加发布包大小
**缓解措施**：
- System.CommandLine 支持代码裁剪（已测试）
- 单文件发布时额外增加约 100-200KB（可接受）
- 性能影响可忽略（参数解析 < 10ms）

### 风险 3：复杂参数解析的迁移
**影响**：`-f:` 自定义格式、`-ft:` 过滤条件等复杂参数需要仔细处理
**缓解措施**：
- 保留原有的解析逻辑（如 `ParsePattern` 类）
- 只改变参数获取方式，不改变解析逻辑
- 为复杂参数增加额外验证和示例

### 风险 4：测试更新工作量
**影响**：3 个集成测试套件共 10+ 个测试用例需要更新
**缓解措施**：
- 创建测试命令转换工具
- 更新测试框架的命令构建逻辑（`test-helpers.sh`）
- 一次性批量更新所有 `test-config.yaml`

### 权衡 1：灵活性 vs. 一致性
**选择**：优先一致性
- ✅ 强制标准格式，即使某些场景略显冗长
- ✅ 不提供"快捷方式"或"简化模式"
- 理由：长期维护价值高于短期便利

### 权衡 2：向后兼容 vs. 代码简洁
**选择**：不向后兼容
- ✅ 代码更简洁，没有历史包袱
- ❌ 用户需要一次性更新
- 理由：清晰的迁移路径比双模式共存更健康

### 权衡 3：单文件大小 vs. 现代化
**选择**：接受轻微的体积增加
- ✅ 使用现代库带来的开发效率提升
- ✅ 100-200KB 增加在现代环境中可接受
- 理由：用户体验和代码质量优先

## 迁移计划

### 阶段 1：核心重构（代码层）

**任务**：
1. 添加 `System.CommandLine` NuGet 包
2. 创建 `CommandLineOptions.cs` 类
3. 创建 `CommandBuilder.cs` 类
4. 重构 `Program.cs`：
   - 调用 `CommandBuilder.Build()`
   - 添加旧格式检测
5. 重构 `ConsoleRun.cs`：
   - 移除所有参数解析代码
   - 保留业务协调逻辑
   - 改为接受 `CommandLineOptions` 对象

**验证**：
- 手工测试基本转换命令
- 验证帮助信息显示正确
- 验证错误信息清晰可读

### 阶段 2：文档更新

**任务**：
1. 创建 `MIGRATION.md` 迁移指南
2. 更新 `README.md`：
   - 所有命令示例
   - 快速开始部分
3. 更新 `CLAUDE.md`：
   - 常用命令部分
   - 所有示例
4. 更新 `Program.cs` 中的帮助信息
5. 更新 `src/ImeWlConverterCmd/Readme.txt`

**验证**：
- 文档中的所有示例可运行
- 新旧格式对照表准确

### 阶段 3：集成测试更新

**任务**：
1. 更新 `tests/integration/lib/test-helpers.sh`：
   - 修改 `run_converter()` 函数
   - 更新命令构建逻辑
2. 更新所有测试配置：
   - `1-imports/test-config.yaml`
   - `2-exports/test-config.yaml`
   - `3-advanced/test-config.yaml`
3. 创建参数转换脚本（可选）

**验证**：
- 运行 `./run-tests.sh --all`
- 所有测试通过

### 阶段 4：CI/CD 更新

**任务**：
1. 更新 `.github/workflows/ci.yml`
2. 更新 `Makefile` 中的示例注释
3. 更新 `.vscode/launch.json` 启动配置

**验证**：
- CI 流程运行成功
- 构建产物正常

### 回滚策略

如果发现重大问题：

1. **代码回滚**：
   - Git revert 所有变更
   - 恢复原有的 ConsoleRun.cs

2. **测试回滚**：
   - Git 保留测试的历史版本
   - 可快速恢复

3. **发布说明**：
   - 如果已发布，发布补丁版本说明回滚原因

## 实现细节

### 文件结构变更

**新增文件**：
```
src/ImeWlConverterCmd/
├── CommandLineOptions.cs       (新)
├── CommandBuilder.cs            (新)
└── MIGRATION.md                 (新，或放在根目录)
```

**修改文件**：
```
src/ImeWlConverterCmd/
├── Program.cs                   (大幅修改)
├── ImeWlConverterCmd.csproj     (添加 NuGet 引用)
└── Readme.txt                   (更新示例)

src/ImeWlConverterCore/
└── ConsoleRun.cs                (重构，减少 ~250 行)
```

### 依赖变更

**ImeWlConverterCmd.csproj**：
```xml
<ItemGroup>
    <PackageReference Include="UTF.Unknown" Version="2.5.1"/>
    <PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1"/>
</ItemGroup>
```

**版本选择**：
- System.CommandLine 2.0.0-beta4 是当前最稳定的版本
- 支持 .NET 6+ (包括 .NET 10.0)
- 支持代码裁剪

### 性能影响

**预期影响**：
- 参数解析时间：< 10ms (可忽略)
- 内存开销：< 1MB (可忽略)
- 启动时间：几乎无变化
- 发布包大小：+100-200KB

### 测试策略

**单元测试** (可选，建议添加)：
```csharp
// 测试参数解析
[Test]
public void TestCommandLineOptions_BasicConversion()
{
    var args = new[] { "-i", "scel", "-o", "ggpy", "-O", "out.txt", "in.scel" };
    var opts = CommandBuilder.Parse(args);
    Assert.AreEqual("scel", opts.InputFormat);
    Assert.AreEqual("ggpy", opts.OutputFormat);
}
```

**集成测试**：
- 现有的 shell 脚本测试覆盖所有场景
- 更新测试配置即可，无需新增测试

## 开放问题

### 问题 1：是否需要 shell 补全脚本？
**背景**：System.CommandLine 支持生成 bash/zsh/powershell 补全脚本
**决策待定**：是否在此次重构中包含？
**建议**：作为后续优化项，不阻塞当前重构

### 问题 2：是否重命名 ConsoleRun 类？
**选项**：
- 保持 `ConsoleRun`（向后兼容类名）
- 重命名为 `ConversionCoordinator`（更准确）
**建议**：保持 `ConsoleRun`，减少改动范围

### 问题 3：是否需要配置文件支持？
**背景**：复杂参数（如过滤条件、自定义格式）可能适合从配置文件读取
**决策待定**：是否支持 `--config config.json`？
**建议**：作为后续增强，不在此次重构范围内

### 问题 4：MIGRATION.md 放在哪里？
**选项**：
- 根目录 `MIGRATION.md`（更显眼）
- `src/ImeWlConverterCmd/MIGRATION.md`（更集中）
**建议**：根目录，因为影响所有用户
