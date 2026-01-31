# 功能规范: 命令行参数解析系统 (cmd-args-parsing)

**变更**: refactor-cmd-args-format
**创建时间**: 2026-01-31
**状态**: 草稿

## 概述

本规范定义了 IME WL Converter 命令行工具的标准化参数解析系统。系统必须采用 GNU 风格的命令行参数格式，提供清晰、一致、符合 Unix 哲学的用户界面。

## 新增需求

### 需求：系统必须支持 GNU 风格的长选项格式
系统必须接受 `--option-name value` 格式的参数，其中选项名使用小写字母和连字符分隔多个单词。

#### 场景：使用长选项指定输入格式
- **当** 用户执行 `imewlconverter --input-format scel input.scel`
- **那么** 系统必须识别输入格式为 `scel`

#### 场景：使用长选项指定输出格式和路径
- **当** 用户执行 `imewlconverter --input-format scel --output-format ggpy --output result.txt input.scel`
- **那么** 系统必须将 input.scel 从搜狗格式转换为谷歌拼音格式并输出到 result.txt

### 需求：系统必须支持 POSIX 风格的短选项格式
系统必须为常用选项提供单字母短选项别名，格式为 `-o value`。

#### 场景：使用短选项进行快速转换
- **当** 用户执行 `imewlconverter -i scel -o ggpy -O output.txt input.scel`
- **那么** 系统必须正确解析并执行转换

#### 场景：混合使用长短选项
- **当** 用户执行 `imewlconverter --input-format scel -o ggpy -O output.txt input.scel`
- **那么** 系统必须正确解析所有选项

### 需求：系统必须支持位置参数作为输入文件
系统必须接受一个或多个位置参数作为输入文件路径，而不需要选项前缀。

#### 场景：单个输入文件
- **当** 用户执行 `imewlconverter -i scel -o ggpy -O output.txt input.scel`
- **那么** 系统必须将 input.scel 作为输入文件

#### 场景：多个输入文件
- **当** 用户执行 `imewlconverter -i scel -o ggpy -O output.txt file1.scel file2.scel file3.scel`
- **那么** 系统必须处理所有三个输入文件并合并转换结果

#### 场景：使用通配符的输入文件
- **当** 用户执行 `imewlconverter -i scel -o ggpy -O output.txt *.scel`
- **那么** 系统必须处理当前目录下所有 .scel 文件

### 需求：系统必须提供标准的帮助信息
系统必须在用户使用 `--help` 或 `-h` 时显示格式化的帮助信息。

#### 场景：显示完整帮助信息
- **当** 用户执行 `imewlconverter --help`
- **那么** 系统必须显示包含以下内容的帮助信息：
  - USAGE（使用概要）
  - OPTIONS（所有可用选项及其说明）
  - EXAMPLES（常用示例）
  - 支持的格式列表

#### 场景：显示版本信息
- **当** 用户执行 `imewlconverter --version` 或 `imewlconverter -v`
- **那么** 系统必须显示版本号和构建信息

### 需求：系统必须验证参数的完整性
系统必须在执行转换前验证所有必需参数已提供，并在缺失时显示清晰的错误信息。

#### 场景：缺少输入格式参数
- **当** 用户执行 `imewlconverter input.scel`（未指定 -i）
- **那么** 系统必须显示错误信息："Error: Required option '--input-format' is missing"
- **并且** 系统必须显示使用提示

#### 场景：缺少输出格式参数
- **当** 用户执行 `imewlconverter -i scel input.scel`（未指定 -o）
- **那么** 系统必须显示错误信息："Error: Required option '--output-format' is missing"

#### 场景：缺少输入文件
- **当** 用户执行 `imewlconverter -i scel -o ggpy -O output.txt`（未提供输入文件）
- **那么** 系统必须显示错误信息："Error: No input files specified"

### 需求：系统必须验证参数值的有效性
系统必须验证选项值是否在允许的范围内，并在无效时提供清晰的错误信息。

#### 场景：无效的输入格式
- **当** 用户执行 `imewlconverter -i invalid-format input.scel`
- **那么** 系统必须显示错误信息："Error: Unknown input format 'invalid-format'"
- **并且** 系统必须列出所有支持的格式

#### 场景：无效的过滤条件格式
- **当** 用户执行 `imewlconverter -i scel -o ggpy -f "invalid" input.scel`
- **那么** 系统必须显示错误信息说明正确的过滤条件格式

### 需求：系统必须支持完整的选项集合
系统必须支持以下选项，每个选项都必须有长格式和（适用时）短格式。

#### 场景：输入格式选项
- **当** 用户使用 `--input-format <format>` 或 `-i <format>`
- **那么** 系统必须识别输入文件的格式代码（如 scel, ggpy, qqpy 等）

#### 场景：输出格式选项
- **当** 用户使用 `--output-format <format>` 或 `-o <format>`
- **那么** 系统必须识别目标输出格式代码

#### 场景：输出路径选项
- **当** 用户使用 `--output <path>` 或 `-O <path>`
- **那么** 系统必须将转换结果写入指定路径

#### 场景：编码文件选项
- **当** 用户使用 `--code-file <path>` 或 `-c <path>`
- **那么** 系统必须加载指定的编码映射文件

#### 场景：过滤条件选项
- **当** 用户使用 `--filter <conditions>` 或 `-f <conditions>`
- **那么** 系统必须应用指定的过滤条件（如 "len:1-100|rm:eng"）

#### 场景：自定义格式选项
- **当** 用户使用 `--custom-format <spec>` 或 `-F <spec>`
- **那么** 系统必须使用指定的自定义格式定义

#### 场景：词频生成器选项
- **当** 用户使用 `--rank-generator <type>` 或 `-r <type>`
- **那么** 系统必须使用指定的词频生成方式（baidu, google, 或固定数字）

#### 场景：多字词编码规则选项
- **当** 用户使用 `--multi-code <rules>` 或 `-m <rules>`
- **那么** 系统必须应用指定的多字词编码生成规则

#### 场景：编码类型选项（Rime）
- **当** 用户使用 `--code-type <type>` 或 `-t <type>`
- **那么** 系统必须为 Rime 输出设置指定的编码类型（pinyin, wubi, zhengma）

#### 场景：目标操作系统选项
- **当** 用户使用 `--target-os <os>`
- **那么** 系统必须生成适配指定操作系统的输出（windows, macos, linux）

#### 场景：Lingoes ld2 编码选项
- **当** 用户使用 `--ld2-encoding <encoding>`
- **那么** 系统必须使用指定编码解析 Lingoes ld2 文件

### 需求：系统必须支持批量输出模式
系统必须支持将多个输入文件转换到指定目录，保持原文件名。

#### 场景：批量转换到目录
- **当** 用户执行 `imewlconverter -i scel -o ggpy -O ./output/ file1.scel file2.scel`
- **那么** 系统必须在 ./output/ 目录下创建 file1.txt 和 file2.txt

#### 场景：输出路径以斜杠结尾表示目录
- **当** 用户指定的输出路径以 `/` 结尾
- **那么** 系统必须将其识别为目录路径并进行批量转换

### 需求：系统禁止使用冒号分隔的旧格式
系统禁止接受旧的 `-i:format` 格式参数，必须引导用户使用新格式。

#### 场景：检测到旧格式参数
- **当** 用户执行 `imewlconverter -i:scel input.scel`
- **那么** 系统必须显示错误信息："Error: Old parameter format detected. Please use '--input-format scel' or '-i scel' instead."
- **并且** 系统必须提供迁移指南链接

### 需求：系统必须提供清晰的错误信息
所有错误信息必须包含问题描述、正确用法示例、以及相关文档链接。

#### 场景：参数解析错误显示详细信息
- **当** 参数解析失败
- **那么** 系统必须显示：
  - 错误类型（Missing required option, Invalid value, etc.）
  - 具体的错误位置（哪个选项/参数）
  - 正确的用法示例
  - 提示用户使用 `--help` 获取完整帮助

### 需求：系统必须支持组合使用多个选项
系统必须正确处理多个选项的组合使用，并验证选项之间的依赖关系。

#### 场景：自定义格式需要格式规范
- **当** 用户使用 `-o self` 输出为自定义格式但未提供 `-F`
- **那么** 系统必须显示警告或使用默认格式规范

#### 场景：编码文件与自定义编码类型配合
- **当** 用户同时使用 `-c code.txt` 和 `-t pinyin`
- **那么** 系统必须正确应用自定义编码文件和编码类型

### 需求：系统必须提供使用示例
帮助信息必须包含常见使用场景的完整示例。

#### 场景：帮助信息中的示例部分
- **当** 用户查看 `--help` 输出的 EXAMPLES 部分
- **那么** 系统必须显示至少以下示例：
  - 基本转换（单个文件）
  - 批量转换（多个文件到目录）
  - 使用过滤条件
  - 使用自定义编码文件
  - 使用词频生成器

## 修改需求

_无_ - 这是全新的参数解析系统，不修改现有功能的行为逻辑

## 移除需求

### 需求：冒号分隔的参数格式
**Reason**: 旧的 `-i:format` 格式不符合 GNU/POSIX 标准，用户体验差，难以与标准工具集成。新的 GNU 风格格式更清晰、更标准。

**Migration**:
- 将 `-i:format` 改为 `--input-format format` 或 `-i format`
- 将 `-o:format` 改为 `--output-format format` 或 `-o format`
- 将 `-c:path` 改为 `--code-file path` 或 `-c path`
- 将 `-f:spec` 改为 `--custom-format spec` 或 `-F spec`
- 将 `-ft:filter` 改为 `--filter filter` 或 `-f filter`
- 将 `-r:type` 改为 `--rank-generator type` 或 `-r type`
- 将 `-ct:type` 改为 `--code-type type` 或 `-t type`
- 将 `-os:os` 改为 `--target-os os`
- 将 `-mc:rules` 改为 `--multi-code rules` 或 `-m rules`
- 将 `-ld2:enc` 改为 `--ld2-encoding enc`

完整迁移示例：
```bash
# 旧格式
dotnet ImeWlConverterCmd.dll -i:scel input.scel -o:ggpy output.txt

# 新格式
imewlconverter --input-format scel --output-format ggpy --output output.txt input.scel
# 或使用短选项
imewlconverter -i scel -o ggpy -O output.txt input.scel
```

## 技术约束

- 必须使用 `System.CommandLine` 库（Microsoft 官方）或 `CommandLineParser` 库实现参数解析
- 必须支持 .NET 10.0 及以上版本
- 必须保持跨平台兼容性（Windows, Linux, macOS）
- 参数解析性能必须可忽略不计（< 50ms）

## 兼容性

- **BREAKING CHANGE**: 完全移除对旧参数格式的支持
- 不提供向后兼容模式
- 在检测到旧格式时提供清晰的迁移指引
