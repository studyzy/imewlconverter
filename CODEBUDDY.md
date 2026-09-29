
## 快速定位

> 这是你进入项目时最先需要知道的信息。根据任务类型，跳转到对应章节。

| 你想做什么 | 入口文件/目录 |
|-----------|-------------|
| 添加新的输入法格式支持 | `src/ImeWlConverter.Formats/{Format}/` → 创建 Importer + Exporter |
| 修改码表/字典数据（拼音、五笔、注音等） | `src/ImeWlConverter.CodeData/`（接口 + `Resources/` 嵌入码表） |
| 修改转换管道逻辑 | `src/ImeWlConverter.Core/Pipeline/ConversionPipeline.cs` |
| 修改 CLI 参数/行为 | `src/ImeWlConverterCmd/CommandBuilder.cs` |
| 修改编码生成（拼音/五笔等） | `src/ImeWlConverter.Core/CodeGeneration/Generators/` |
| 修改过滤器 | `src/ImeWlConverter.Core/Filters/` |
| 修改过滤配置 DTO | `src/ImeWlConverter.Abstractions/Options/FilterConfig.cs` |
| 修改 macOS GUI | `src/ImeWlConverterMac/ViewModels/MainWindowViewModel.cs` |
| 修改 Windows GUI | `src/IME WL Converter Win/Forms/MainForm.cs` |
| 添加/修改单元测试 | `src/ImeWlConverterCoreTest/` |
| 修改 CI 流程 | `.github/workflows/ci.yml` |
| 发布新版本 | `docs/RELEASING.md` |

## 项目一句话描述

深蓝词库转换（IME WL Converter）是一个跨平台的输入法词库格式转换工具，支持 50+ 种输入法格式之间的相互转换。

- **语言**: C# / .NET 10.0
- **测试框架**: xUnit 2.9.3
- **构建系统**: Makefile + dotnet CLI
- **版本管理**: MinVer（从 Git tag 自动生成）
- **平台**: Windows / Linux / macOS

## 日常命令

```bash
make build              # 构建所有项目
make test               # 运行单元测试 (271 个，实际以 dotnet test 输出为准)
make integration-test   # 运行集成测试 (29 个用例，需先 make build-cmd)
make lint               # 检查代码格式
make format             # 自动格式化代码
make run-cmd            # 运行 CLI 工具
make run-mac            # 运行 macOS GUI
```

## 架构概览

```
src/
├── ImeWlConverter.Abstractions/     # 接口层（零依赖）
├── ImeWlConverter.CodeData/         # 码表数据叶子（零依赖，只读线程安全表服务）
├── ImeWlConverter.Core/             # 业务服务层（转换管道、编码生成、过滤、简繁转换）
├── ImeWlConverter.Formats/          # 格式实现层（107个文件，50+种格式）
├── ImeWlConverter.SourceGenerators/ # Source Generator（编译时格式注册）
├── ImeWlConverterCmd/               # CLI 入口
├── ImeWlConverterMac/               # macOS GUI (Avalonia 11.2.3)
├── IME WL Converter Win/            # Windows GUI (WinForms)
└── ImeWlConverterCoreTest/          # xUnit 单元测试
```

### 三端统一数据流

```
┌─────────────┐   ┌─────────────┐   ┌─────────────┐
│   CLI       │   │  WinForms   │   │  macOS GUI  │
│ CommandBuild│   │  MainForm   │   │  ViewModel  │
└──────┬──────┘   └──────┬──────┘   └──────┬──────┘
       │                 │                  │
       └────────────┬────┴──────────────────┘
                    ▼
        ┌───────────────────────┐
        │  IConversionPipeline  │  (Abstractions 层接口)
        └───────────┬───────────┘
                    ▼
        ┌───────────────────────┐
        │  ConversionPipeline   │  (Core 层统一实现)
        │                       │
        │  Import → Filter →    │
        │  ChineseConvert →     │
        │  WordRank → CodeGen → │
        │  RemoveEmpty → Export │
        └───────────────────────┘
```

**设计原则**：三端（CLI、WinForms、macOS GUI）共用同一个 `ConversionPipeline` 底层转换引擎，只在用户交互层不同。

- CLI 通过 `CommandBuilder` 解析参数构建 `ConversionRequest`
- WinForms 通过 `MainForm` 用户操作构建 `ConversionRequest`
- macOS GUI 通过 `MainWindowViewModel` 构建 `ConversionRequest`
- 三端共享 `FilterConfig`（`Abstractions/Options/`）、`ConversionOptions`、`IProgress<ProgressInfo>`

---

## 深入：项目层次

以下按从外到内的顺序描述各层，你只需要读到与任务相关的层即可。

### Layer 1: 前端入口

#### CLI (`src/ImeWlConverterCmd/`)

| 文件 | 职责 |
|------|------|
| `Program.cs` | 入口，旧参数格式检测，调用 CommandBuilder |
| `CommandBuilder.cs` | System.CommandLine 定义所有 CLI 选项，构建 `ConversionRequest`，调用 `ConversionPipeline` |

#### macOS GUI (`src/ImeWlConverterMac/`)

| 文件 | 职责 |
|------|------|
| `App.axaml.cs` | 应用入口，创建 DI 容器，注入 ViewModel |
| `ViewModels/MainWindowViewModel.cs` | MVVM ViewModel，构建 `ConversionRequest`，调用 `IConversionPipeline` |
| `Views/FilterConfigWindow.axaml.cs` | 过滤配置 UI，使用共享 `FilterConfig` DTO |

#### Windows GUI (`src/IME WL Converter Win/`)

| 文件 | 职责 |
|------|------|
| `Program.cs` | 应用入口，创建 DI 容器 |
| `Forms/MainForm.cs` | 主窗口，构建 `ConversionRequest`，调用 `IConversionPipeline` |
| `Forms/FilterConfigForm.cs` | 过滤配置 UI，使用共享 `FilterConfig` DTO |

### Layer 2: 转换管道 (`src/ImeWlConverter.Core/Pipeline/`)

| 文件 | 职责 |
|------|------|
| `ConversionPipeline.cs` | 薄编排层：导入 → 委托 EntryTransformationService → 导出；合并/逐文件两种模式 |
| `EntryTransformationService.cs` | 词条五阶段处理（Filter→ChineseConvert→WordRank→CodeGen→RemoveEmpty），两条导出路径共用 |
| `FilterPipelineFactory.cs` + `FilterModules/` | 过滤器装配（模块注册制，开闭原则） |
| `FilterPipeline.cs` | 过滤执行器（单条过滤 → 变换 → 批量过滤） |
| `CodePredicates.cs` | 空编码判定谓词（两条路径共用） |
| `SelfDefiningCodeSource.cs` | 自定义码表加载（ISelfDefiningCodeSource 实现） |

`ConversionPipeline` 支持的能力：
- **合并导出** / **逐文件导出**（`MergeToOneFile` 选项）
- **文件输出** / **Stream 输出**（GUI 先预览再保存）
- **从 FilterConfig 经 FilterPipelineFactory 模块化构建 FilterPipeline**
- **IProgress 细粒度进度报告**
- **CancellationToken 取消支持**
- **逐文件错误捕获与结构化累积**（`ConversionResult.Errors`）

### Layer 3: 格式实现 (`src/ImeWlConverter.Formats/`)

每个格式拆分为独立的 Importer 和 Exporter，通过 `[FormatPlugin]` 属性标记，Source Generator 自动注册到 DI。

| 基类 | 用途 |
|------|------|
| `TextFormatImporter` | 文本格式导入（逐行解析） |
| `TextFormatExporter` | 文本格式导出（逐行生成） |
| `BinaryFormatImporter` | 二进制格式导入 |

### Layer 4: 核心基础设施 (`src/ImeWlConverter.Core/`)

| 目录 | 内容 |
|------|------|
| `CodeGeneration/Generators/` | 编码生成器（拼音、五笔86/98/新世纪、郑码、仓颉、注音、超音、二笔x4） |
| `Filters/` | IWordFilter / IWordTransform / IBatchFilter 实现（12 种） |
| `Helpers/` | 文件操作、编码检测、拼音字典、HTTP |
| `Language/` | 纯 .NET 简繁转换（基于 OpenCC 映射表） |
| `WordRank/` | 默认词频生成器 |
| `Resources/` | 嵌入式码表 + 简繁映射文件 |

### Layer 5: 抽象层 (`src/ImeWlConverter.Abstractions/`)

纯接口和 DTO，零依赖。供所有层引用。

| 子目录 | 关键类型 |
|--------|---------|
| `Contracts/` | IFormatImporter, IFormatExporter, IConversionPipeline, ICodeGenerator, IWordFilter, IWordTransform, IBatchFilter |
| `Models/` | WordEntry (sealed record), WordCode, FormatMetadata, ProgressInfo |
| `Options/` | ConversionOptions, FilterConfig, FilterOptions, CodeGenerationOptions, ImportOptions, ExportOptions |
| `Results/` | Result\<T\>, ImportResult, ExportResult, ConversionRequest, ConversionResult |
| `Enums/` | CodeType, SortType, PinyinType, ChineseConversionMode |

---

## 常见任务操作指南

### 添加新的输入法格式

```bash
# 1. 在 Formats 项目中创建目录和文件
mkdir src/ImeWlConverter.Formats/MyFormat/
```

```csharp
// 2. 创建 Importer（注意：必须是 partial 类，且不要手写 Metadata 属性——由 Source Generator 生成）
[FormatPlugin("myf", "我的格式", 500)]
public sealed partial class MyFormatImporter : TextFormatImporter
{
    protected override Encoding FileEncoding => new UTF8Encoding(false);
    protected override bool IsContentLine(string line) =>
        !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"); // 可选：跳过注释/空行
    protected override IEnumerable<WordEntry> ParseLine(string line) { /* 解析逻辑 */ }
}
```

```csharp
// 3. 创建 Exporter（同样不手写 Metadata）
[FormatPlugin("myf", "我的格式", 500)]
public sealed partial class MyFormatExporter : TextFormatExporter
{
    protected override Encoding FileEncoding => new UTF8Encoding(false);
    protected override string? FormatEntry(WordEntry entry) { /* 导出逻辑，返回 null 跳过该词条 */ }
}
```

Source Generator 会自动完成两件事：
- 将带 `[FormatPlugin]` 的类注册到 DI 容器（无需手动注册）
- 为每个类生成 `Metadata` 属性（从 `[FormatPlugin]` 参数与基类链自动推断 `IsBinary` 等，**手写 Metadata 会与生成代码冲突**）

完整真实示例见 `src/ImeWlConverter.Formats/Rime/RimeImporter.cs`。

### 修改过滤逻辑

过滤器位于 `src/ImeWlConverter.Core/Filters/`，实现接口：

```csharp
public interface IWordFilter { bool ShouldKeep(WordEntry entry); }
public interface IWordTransform { WordEntry? Transform(WordEntry entry); }
public interface IBatchFilter { IReadOnlyList<WordEntry> Filter(IReadOnlyList<WordEntry> entries); }
```

`FilterConfig`（`Abstractions/Options/FilterConfig.cs`）定义了所有过滤选项，`ConversionPipeline` 内部的 `BuildFilterPipeline()` 方法将其转换为 `FilterPipeline` 实例。

CLI 通过 `--filter` 参数启用过滤：`-f "len:2-10|rm:eng|rm:num"`

### 修改编码生成

编码生成器位于 `src/ImeWlConverter.Core/CodeGeneration/Generators/`，实现 `ICodeGenerator` 接口。通过 DI 注册，由 `CodeGenerationService` 根据 `CodeType` 选择对应生成器。

### 修改转换管道行为

- **编排与导出**：`src/ImeWlConverter.Core/Pipeline/ConversionPipeline.cs`（薄层）
- **词条阶段处理**：`EntryTransformationService.cs`，7 步流程：

1. **Import** — 逐文件导入，累积所有 WordEntry（ConversionPipeline）
2. **Filter → ChineseConvert → WordRank → CodeGen → RemoveEmpty** — EntryTransformationService.ApplyAsync（合并模式调一次，逐文件模式每文件一次）
3. **Export** — 导出到文件或 Stream（ConversionPipeline）

### 修改过滤逻辑（注册制）

- 过滤器实现位于 `src/ImeWlConverter.Core/Filters/`，装配位于 `Pipeline/FilterPipelineFactory.cs` + `Pipeline/FilterModules/`
- **新增过滤器**：实现 `IFilterModule`（或往现有模块加一行）→ 加入 `DefaultFilterModules.Create()` 清单即可，无需修改管道

---

## 项目约定

### 禁止操作

- **禁止手动修改版本号**（由 MinVer 从 Git tag 生成，配置在 `src/Directory.Build.props`）
- **禁止使用运行时反射注册格式**（Source Generator 自动注册）
- **禁止硬编码路径分隔符**（用 `Path.Combine()`）
- **禁止在 GUI 项目中重复实现转换逻辑**（统一使用 `IConversionPipeline`）

### DI 注册模式

三端使用相同的 DI 注册方式：

```csharp
var services = new ServiceCollection();
services.AddAllFormats();           // Source Generator 生成的格式注册
services.AddImeWlConverterCore();   // 管道、编码生成器、简繁转换、词频生成器
var sp = services.BuildServiceProvider();

// 获取管道
var pipeline = sp.GetRequiredService<IConversionPipeline>();
```

### 编码处理

项目处理多种字符编码（UTF-8、GBK、GB2312、Big5、Unicode）。.NET 10 已内置 CodePages 支持，但部分旧代码仍调用：
```csharp
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
```

### 跨平台注意事项

- CLI 工具为 framework-dependent（需 .NET 运行时）
- macOS app bundle 为 self-contained（包含运行时）
- Windows GUI 为 WinForms（仅 Windows）
- 集成测试支持 Linux、macOS、Windows (Git Bash)

### 测试

- **单元测试**: xUnit 2.9.3，位于 `src/ImeWlConverterCoreTest/`
- **集成测试**: shell 脚本框架，位于 `tests/integration/`
- 测试并行执行（`xunit.runner.json` 中 `parallelizeTestCollections: true`；Phase 2 已消灭静态可变状态，码表全部走 `ImeWlConverter.CodeData` 的线程安全只读表）
- **禁新增 static 可变字典/集合**——需要码表数据时注入 `ICodeTableLibrary`/`IPinyinTable` 等接口（见 `src/ImeWlConverter.CodeData/`）
- `[Fact(Skip = "...")]` 标记按需运行的慢速测试

### CI/CD

GitHub Actions (`.github/workflows/ci.yml`)：
1. Lint（格式检查，快速失败）
2. Build + 单元测试 (Ubuntu)
3. 多平台构建 (Win x64/x86, Linux x64/arm64, macOS x64/arm64)
4. 集成测试 (Linux + macOS)

---

## 关键决策记录

| 决策 | 原因 |
|------|------|
| 三端统一使用 ConversionPipeline | 消除重复转换逻辑，保证行为一致 |
| FilterConfig 放在 Abstractions 层 | 三端共享过滤配置 DTO，避免各端重复定义 |
| ConversionPipeline 支持 Stream 输出 | GUI 需要先预览内容再决定是否保存 |
| CLI 用 FormatRegistrar 显式注册 | 消除反射，支持 AOT/Trimming |
| Source Generator 注册新格式 | 添加格式只需加 `[FormatPlugin]` 属性 |
| 测试并行执行 | Phase 2 建立码表数据层（CodeData），静态可变字典全部改为 DI 只读表，竞态根因消除 |
| sealed record 实体 | 不可变性保证，不会意外修改中间状态 |
