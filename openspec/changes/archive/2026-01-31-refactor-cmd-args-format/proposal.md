## 为什么

当前 CLI 使用自定义的参数格式（如 `-i:scel`、`-o:ggpy`），这与 Linux/Unix 标准的 GNU 风格参数格式不一致。这导致：

1. **用户体验差**：违背 Unix 哲学和用户习惯，与 `git`、`docker`、`npm` 等主流工具的参数风格不一致
2. **工具集成困难**：难以与 shell 脚本、自动补全、参数解析工具集成
3. **文档混乱**：帮助信息冗长且不符合标准格式（如 `--help` 输出应有明确的选项说明）
4. **维护成本高**：手工解析参数，代码中充斥 `command.StartsWith("-i:")` 等字符串操作

重构为标准 GNU 风格（`--input/-i <format>`）将提升工具的专业性和易用性。

## 变更内容

### **BREAKING**: 命令行参数格式全面改版

从当前的冒号分隔格式：
```bash
dotnet ImeWlConverterCmd.dll -i:scel ./test.scel -o:ggpy ./output.txt -c:./code.txt
```

改为标准 GNU 风格：
```bash
imewlconverter --input-format scel --output-format ggpy --output ./output.txt ./test.scel
# 或使用短选项
imewlconverter -i scel -o ggpy -O ./output.txt ./test.scel
```

### 具体改动

1. **引入命令行解析库**
   - 采用 `System.CommandLine` (Microsoft 官方库) 或 `CommandLineParser` 库
   - 提供自动生成的帮助信息、参数验证、类型转换

2. **参数格式标准化**

   | 当前格式 | 新格式（长选项） | 短选项 | 说明 |
   |---------|-----------------|--------|------|
   | `-i:<format>` | `--input-format <format>` | `-i <format>` | 输入格式 |
   | `-o:<format>` | `--output-format <format>` | `-o <format>` | 输出格式 |
   | `路径` | `<INPUT_FILES>...` | 位置参数 | 输入文件（多个） |
   | `路径` | `--output <path>` | `-O <path>` | 输出路径 |
   | `-c:<path>` | `--code-file <path>` | `-c <path>` | 编码文件 |
   | `-mc:<rules>` | `--multi-code <rules>` | `-m <rules>` | 多字词编码规则 |
   | `-ft:<filter>` | `--filter <filter>` | `-f <filter>` | 过滤条件 |
   | `-f:<format>` | `--custom-format <format>` | `-F <format>` | 自定义格式 |
   | `-r:<rank>` | `--rank-generator <type>` | `-r <type>` | 词频生成方式 |
   | `-ct:<type>` | `--code-type <type>` | `-t <type>` | 编码类型（Rime） |
   | `-os:<os>` | `--target-os <os>` | 无 | 目标操作系统 |
   | `-ld2:<enc>` | `--ld2-encoding <enc>` | 无 | Lingoes ld2 编码 |
   | `-h` | `--help` | `-h` | 帮助信息 |
   | `-v` | `--version` | `-v` | 版本信息 |

3. **改进的帮助系统**
   ```bash
   imewlconverter --help
   ```
   输出标准格式的帮助信息，包括：
   - 使用概要（USAGE）
   - 选项说明（OPTIONS）
   - 示例（EXAMPLES）
   - 支持的格式列表

4. **位置参数优化**
   - 输入文件作为位置参数，支持多文件和通配符
   - `--output` 明确指定输出路径
   - 示例：
     ```bash
     imewlconverter -i scel -o ggpy -O output.txt input1.scel input2.scel
     imewlconverter -i scel -o ggpy -O ./temp/ *.scel
     ```

5. **更新所有文档**
   - `README.md` - 更新使用示例
   - `CLAUDE.md` - 更新命令参考
   - `src/ImeWlConverterCmd/Program.cs` - 重写帮助信息
   - `src/ImeWlConverterCmd/Readme.txt` - 更新说明文档

6. **更新集成测试**
   - `tests/integration/` 下所有测试用例
   - 测试框架适配新的参数格式
   - CI/CD 工作流命令更新

7. **移除向后兼容性**
   - 不保留旧格式支持，完全切换到新格式
   - 在帮助信息中添加迁移指南

## 功能 (Capabilities)

### 新增功能
- `cmd-args-parsing`: 基于 System.CommandLine 的现代化命令行参数解析系统

### 修改功能
_无_ - 本次仅重构参数格式，不改变业务逻辑和转换功能的需求

## 影响

### 代码影响
- **src/ImeWlConverterCmd/Program.cs** - 入口点，需引入新的参数解析
- **src/ImeWlConverterCore/ConsoleRun.cs** - 核心参数处理逻辑需重构
  - 移除所有 `command.StartsWith("-x:")` 的字符串匹配
  - 使用强类型的参数对象
- **src/ImeWlConverterCmd/ImeWlConverterCmd.csproj** - 添加 NuGet 依赖

### 测试影响
- **tests/integration/test-cases/** 下所有 3 个测试套件：
  - `1-imports/test-config.yaml`
  - `2-exports/test-config.yaml`
  - `3-advanced/test-config.yaml`
- **tests/integration/lib/test-helpers.sh** - 测试辅助函数需适配新格式

### 文档影响
- **README.md** - 主要使用文档，示例全部更新
- **CLAUDE.md** - 开发者指南，命令示例更新
- **Makefile** - 注释中的示例命令
- **.github/workflows/ci.yml** - CI 中的测试命令

### 用户影响
- **BREAKING CHANGE**: 现有脚本和自动化流程需要更新
- 提供迁移指南文档说明新旧格式对照
- 在首次运行时显示迁移提示（如果可能检测到旧格式）

### 依赖影响
- 新增依赖：`System.CommandLine` (Microsoft 官方，stable)
- 目标框架保持 .NET 10.0 不变
