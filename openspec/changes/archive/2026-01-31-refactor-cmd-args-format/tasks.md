# 实施任务清单

本文档列出了重构命令行参数格式的所有实施任务。任务按依赖关系和执行顺序组织。

## 1. 环境准备与依赖

- [x] 1.1 在 `src/ImeWlConverterCmd/ImeWlConverterCmd.csproj` 中添加 `System.CommandLine` NuGet 包引用（版本 2.0.0-beta4.22272.1）
- [x] 1.2 验证 NuGet 包恢复成功，运行 `dotnet restore src/ImeWlConverterCmd`
- [x] 1.3 验证包与代码裁剪（PublishTrimmed）兼容，运行 `dotnet publish -c Release`

## 2. 核心参数解析系统

- [x] 2.1 创建 `src/ImeWlConverterCore/CommandLineOptions.cs` 类，定义所有命令行选项属性
- [x] 2.2 创建 `src/ImeWlConverterCmd/CommandBuilder.cs` 类，实现 `Build()` 方法返回 `RootCommand`
- [x] 2.3 在 `CommandBuilder` 中定义 `--input-format/-i` 选项（必填）
- [x] 2.4 在 `CommandBuilder` 中定义 `--output-format/-o` 选项（必填）
- [x] 2.5 在 `CommandBuilder` 中定义 `--output/-O` 选项（必填）
- [x] 2.6 在 `CommandBuilder` 中定义输入文件位置参数（支持多文件）
- [x] 2.7 在 `CommandBuilder` 中定义 `--code-file/-c` 选项（可选）
- [x] 2.8 在 `CommandBuilder` 中定义 `--filter/-f` 选项（可选）
- [x] 2.9 在 `CommandBuilder` 中定义 `--custom-format/-F` 选项（可选）
- [x] 2.10 在 `CommandBuilder` 中定义 `--rank-generator/-r` 选项（可选）
- [x] 2.11 在 `CommandBuilder` 中定义 `--multi-code/-m` 选项（可选）
- [x] 2.12 在 `CommandBuilder` 中定义 `--code-type/-t` 选项（可选）
- [x] 2.13 在 `CommandBuilder` 中定义 `--target-os` 选项（可选）
- [x] 2.14 在 `CommandBuilder` 中定义 `--ld2-encoding` 选项（可选）
- [x] 2.15 在 `CommandBuilder` 中实现 `SetHandler`，绑定选项到 `CommandLineOptions` 对象

## 3. 重构 Program.cs

- [x] 3.1 在 `Program.cs` 的 `Main` 方法中添加旧格式检测逻辑
- [x] 3.2 实现旧格式检测：检查参数中是否包含冒号（如 `-i:`）
- [x] 3.3 添加旧格式错误提示信息，显示新旧格式对照和迁移文档链接
- [x] 3.4 修改 `Main` 方法，调用 `CommandBuilder.Build()` 获取 `RootCommand`
- [x] 3.5 调用 `rootCommand.Invoke(args)` 执行参数解析和命令处理
- [x] 3.6 移除对旧的 `ConsoleRun(args, Help)` 构造函数的调用
- [x] 3.7 保留 `Help` 方法用于显示支持的格式列表（在需要时由 CommandBuilder 调用）

## 4. 重构 ConsoleRun.cs

- [x] 4.1 修改 `ConsoleRun` 类的构造函数，移除 `string[] args` 参数
- [x] 4.2 添加新方法 `Execute(CommandLineOptions options)`，作为新的入口点
- [x] 4.3 在 `Execute` 方法中实现选项验证逻辑（格式有效性、选项组合）
- [x] 4.4 删除 `RunCommand(string command)` 方法及其所有参数解析逻辑（179-408 行）
- [x] 4.5 保留 `GetImportInterface(string format)` 方法，修改为从 `options.InputFormat` 获取
- [x] 4.6 保留 `GetExportInterface(string format)` 方法，修改为从 `options.OutputFormat` 获取
- [x] 4.7 重构过滤器配置逻辑，创建 `ConfigureFilters(string filterString)` 方法
- [x] 4.8 保留现有的过滤器解析逻辑（正则表达式匹配），移至 `ConfigureFilters` 方法
- [x] 4.9 重构编码类型配置，创建 `ConfigureCodeType(string codeType)` 方法
- [x] 4.10 重构词频生成器配置，创建 `ConfigureRankGenerator(string rankType)` 方法
- [x] 4.11 重构自定义格式解析，创建 `ConfigureCustomFormat(string format)` 方法
- [x] 4.12 修改 `Run()` 方法为 `PerformConversion()`，接受已配置的参数
- [x] 4.13 更新 `Execute` 方法，调用所有配置方法和 `PerformConversion`
- [x] 4.14 删除不再使用的私有字段（如 `beginImportFile`、`type` 枚举等）

## 5. 帮助信息和错误处理

- [x] 5.1 在 `CommandBuilder` 中添加 `RootCommand` 的描述文本
- [x] 5.2 为每个选项添加详细的 `description` 参数
- [x] 5.3 实现 `--list-formats` 选项，显示所有支持的格式列表
- [x] 5.4 在 `--list-formats` 处理器中调用现有的格式枚举逻辑
- [x] 5.5 添加 EXAMPLES 部分到帮助信息（使用 System.CommandLine 的 API）
- [x] 5.6 实现自定义错误处理器，捕获无效格式代码并显示支持的格式列表
- [x] 5.7 实现选项组合验证（如 `-o self` 需要 `-F`），在错误时显示清晰提示
- [x] 5.8 测试 `--help` 输出格式和内容的完整性

## 6. 单元测试（可选但推荐）

- [ ] 6.1 创建 `src/ImeWlConverterCoreTest/CommandLineOptionsTest.cs` 测试类
- [ ] 6.2 添加测试：验证基本转换命令的参数解析
- [ ] 6.3 添加测试：验证多文件输入的参数解析
- [ ] 6.4 添加测试：验证所有可选参数的解析
- [ ] 6.5 添加测试：验证必填参数缺失时的错误处理
- [ ] 6.6 添加测试：验证无效格式代码的错误处理
- [ ] 6.7 添加测试：验证旧格式检测逻辑
- [ ] 6.8 运行单元测试，确保所有测试通过

## 7. 手工验证

- [x] 7.1 测试基本转换命令：`imewlconverter -i scel -o ggpy -O output.txt input.scel`
- [x] 7.2 测试多文件输入：`imewlconverter -i scel -o ggpy -O output.txt file1.scel file2.scel`
- [x] 7.3 测试批量输出到目录：`imewlconverter -i scel -o ggpy -O ./output/ *.scel`
- [x] 7.4 测试过滤条件：`imewlconverter -i scel -o ggpy -O out.txt -f "len:1-100|rm:eng" input.scel`
- [x] 7.5 测试自定义格式：`imewlconverter -i scel -o self -O out.txt -F "213, nyyn" input.scel`
- [x] 7.6 测试编码文件：`imewlconverter -i scel -o self -O out.txt -c code.txt input.scel`
- [x] 7.7 测试词频生成器：`imewlconverter -i scel -o ggpy -O out.txt -r baidu input.scel`
- [x] 7.8 测试 `--help` 显示完整帮助信息
- [x] 7.9 测试 `--version` 显示版本信息
- [x] 7.10 测试 `--list-formats` 显示格式列表
- [x] 7.11 测试缺少必填参数的错误提示
- [x] 7.12 测试无效格式代码的错误提示
- [x] 7.13 测试旧格式检测和迁移提示

## 8. 文档更新

- [x] 8.1 创建 `MIGRATION.md` 迁移指南文档在根目录
- [x] 8.2 在 `MIGRATION.md` 中添加新旧格式完整对照表
- [x] 8.3 在 `MIGRATION.md` 中添加常用命令的迁移示例
- [x] 8.4 在 `MIGRATION.md` 中添加集成测试更新指引
- [x] 8.5 更新 `README.md`：快速开始部分的命令示例
- [x] 8.6 更新 `README.md`：集成测试运行命令示例（第 76-84 行）
- [x] 8.7 更新 `README.md`：所有其他出现命令行参数的地方
- [x] 8.8 更新 `CLAUDE.md`：常用命令部分（第 11-65 行）
- [x] 8.9 更新 `CLAUDE.md`：CLI 使用示例部分
- [x] 8.10 更新 `CLAUDE.md`：调试技巧部分的命令
- [x] 8.11 更新 `src/ImeWlConverterCmd/Readme.txt` 文件中的所有示例
- [x] 8.12 验证所有文档中的命令示例可以运行

## 9. 集成测试更新

- [x] 9.2 更新 `tests/integration/lib/test-helpers.sh` 中的 `run_converter()` 函数
- [x] 9.3 修改命令构建逻辑：从 `-i:format` 改为 `-i format -o format -O output`
- [x] 9.4 更新 `tests/integration/test-cases/1-imports/test-config.yaml`：修改所有测试用例的命令格式
- [x] 9.5 更新 `tests/integration/test-cases/2-exports/test-config.yaml`：修改所有测试用例的命令格式
- [x] 9.6 更新 `tests/integration/test-cases/3-advanced/test-config.yaml`：修改所有测试用例的命令格式
- [x] 9.7 运行单个测试验证：`./run-tests.sh -s 1-imports`
- [x] 9.8 运行单个测试验证：`./run-tests.sh -s 2-exports`
- [x] 9.9 运行单个测试验证：`./run-tests.sh -s 3-advanced`
- [x] 9.10 运行完整测试套件：`./run-tests.sh --all`
- [x] 9.11 验证所有测试通过，无失败用例
- [x] 9.12 在详细模式下运行测试，检查输出：`./run-tests.sh --all -v`

## 10. CI/CD 配置更新

- [x] 10.1 更新 `.github/workflows/ci.yml`：检查是否有硬编码的命令行参数
- [x] 10.2 更新 `Makefile` 中的注释示例（如果有命令行参数示例）
- [x] 10.3 更新 `Makefile` 中的 `regenerate-exports-expected` 目标的命令格式
- [x] 10.4 更新 `.vscode/launch.json` 中的 `commandLineArgs` 配置
- [x] 10.5 更新 `src/ImeWlConverterCmd/Properties/launchSettings.json` 中的调试参数
- [x] 10.6 本地运行 CI 等效命令验证：`make build-all test integration-test`
- [ ] 10.7 提交更改后验证 GitHub Actions CI 通过

## 11. 版本和发布准备

- [ ] 11.1 在 `CHANGELOG.md`（如果存在）中添加 BREAKING CHANGE 条目
- [ ] 11.2 确认版本号遵循语义化版本（如 v3.0.0，因为是 BREAKING CHANGE）
- [ ] 11.3 创建 Git tag 前的检查清单
- [ ] 11.4 准备发布说明草稿，突出 BREAKING CHANGE 和迁移指引
- [ ] 11.5 验证发布包大小变化（预期 +100-200KB）
- [ ] 11.6 在发布说明中添加 `MIGRATION.md` 链接

## 12. 代码清理和优化

- [x] 12.1 移除 `ConsoleRun.cs` 中不再使用的 `using` 语句
- [x] 12.2 移除 `Program.cs` 中不再使用的代码和注释
- [x] 12.3 运行代码格式化：`dotnet format src/ImeWlConverterCmd`
- [x] 12.4 运行代码格式化：`dotnet format src/ImeWlConverterCore`
- [x] 12.5 检查编译警告，修复或抑制适当的警告
- [x] 12.6 检查 TODO 和 FIXME 注释，处理必要的项目
- [x] 12.7 验证没有引入新的代码分析警告

## 13. 最终验证

- [ ] 13.1 在 Windows 上构建并测试（如果适用）
- [ ] 13.2 在 Linux 上构建并测试
- [x] 13.3 在 macOS 上构建并测试
- [x] 13.4 验证单文件发布工作正常：`dotnet publish -c Release`
- [x] 13.5 验证发布的二进制文件可以运行
- [x] 13.6 测试发布的二进制文件的所有主要功能
- [x] 13.7 运行完整的单元测试套件：`make test`
- [x] 13.8 运行完整的集成测试套件：`make integration-test`
- [ ] 13.9 在 CI 环境中验证所有构建和测试通过
- [ ] 13.10 代码审查：检查所有变更符合设计文档

## 14. 回滚准备

- [ ] 14.1 创建当前工作分支的备份
- [ ] 14.2 文档化回滚步骤（如果需要）
- [ ] 14.3 确保 Git 历史清晰，可以干净地 revert
- [ ] 14.4 准备热修复计划（如果发现关键问题）

---

## 验收标准

所有任务完成后，以下条件必须满足：

✅ **功能性**：
- 所有命令行参数使用 GNU 风格格式
- `--help` 显示完整、格式化的帮助信息
- 错误信息清晰且包含使用示例
- 旧格式会被检测并显示迁移提示

✅ **质量**：
- 所有单元测试通过（如果添加）
- 所有集成测试通过（100% 通过率）
- CI/CD 流程成功
- 无新增的编译警告

✅ **文档**：
- `MIGRATION.md` 完整准确
- `README.md` 和 `CLAUDE.md` 示例全部更新
- 所有文档中的命令可运行

✅ **性能**：
- 参数解析时间 < 10ms
- 发布包大小增加 < 300KB
- 无性能回归

✅ **兼容性**：
- Windows, Linux, macOS 三平台测试通过
- 单文件发布工作正常
- 代码裁剪兼容
