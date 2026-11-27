<div align="center">

# 深蓝词库转换

[![Stars](https://img.shields.io/github/stars/studyzy/imewlconverter)](https://github.com/studyzy/imewlconverter/stargazers)
![Actions Check](https://github.com/studyzy/imewlconverter/actions/workflows/ci.yml/badge.svg)
![License](https://img.shields.io/github/license/studyzy/imewlconverter)
![Repo size](https://img.shields.io/github/repo-size/studyzy/imewlconverter)
[![Code Count](https://tokei.rs/b1/github/studyzy/imewlconverter)](https://github.com/studyzy/imewlconverter)

[![Join the chat](https://badges.gitter.im/studyzy/imewlconverter.svg)](https://gitter.im/studyzy/imewlconverter?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Release](https://badgen.net/github/release/studyzy/imewlconverter)](https://github.com/studyzy/imewlconverter/releases)
![Downloads](https://badgen.net/github/assets-dl/studyzy/imewlconverter)

一款输入法词库转换软件，支持 58 种词库格式（56 种可导入、49 种可导出）

</div>

本程序支持批量转换（一次拖拽多个词库文件，或者按住 Ctrl 选择多个文件），支持命令行模式（在命令行下使用 -h 命令查看帮助），支持 Windows、Linux、macOS。

## 快速安装（命令行工具）

### 前置要求

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)（运行 `dotnet --version` 确认已安装）

### 从源码安装

```bash
# 1. 克隆仓库
git clone https://github.com/studyzy/imewlconverter.git
cd imewlconverter

# 2. 构建命令行工具
dotnet build src/ImeWlConverterCmd

# 3. 验证安装（查看帮助）
dotnet src/ImeWlConverterCmd/bin/Debug/net10.0/ImeWlConverterCmd.dll --help
```

构建完成后，命令行工具位于 `src/ImeWlConverterCmd/bin/Debug/net10.0/ImeWlConverterCmd.dll`。

通过 `dotnet <dll路径>` 方式运行，也可以使用 `dotnet run` 简化调用：

```bash
# 方式一：直接运行 DLL
dotnet src/ImeWlConverterCmd/bin/Debug/net10.0/ImeWlConverterCmd.dll --help

# 方式二：使用 dotnet run（自动构建并运行，注意 -- 分隔符）
dotnet run --project src/ImeWlConverterCmd -- --help
```

### 使用 Makefile（推荐）

项目提供了 Makefile 简化构建流程：

```bash
git clone https://github.com/studyzy/imewlconverter.git
cd imewlconverter
make build-cmd                    # Debug 模式构建
# 或
DOTNET_CONFIG=Release make build-cmd  # Release 模式构建
```

## 支持列表

> 以下按输入法工具归类列出主要支持对象。完整的格式代码、显示名与导入/导出能力以下方「格式代码速查表」为准，也可运行 `--list-formats` 查看当前版本实际注册的格式。

PC 端：

- Chinese-pyim(Linux)
- [FIT 输入法(Mac)](https://github.com/studyzy/imewlconverter/wiki/FIT)
- libpinyin(Linux)
- [MacOS 自带简体拼音](https://github.com/studyzy/imewlconverter/wiki/MacPlist)
- [QQ 拼音（文本词库和 qpyd 格式分类词库）](https://github.com/studyzy/imewlconverter/wiki/QQ_Pinyin_Win)
- QQ 输入法自定义短语（自定义短语 .ini，编码=排序位置,词语）
- [QQ 五笔](https://github.com/studyzy/imewlconverter/wiki/QQ_Wubi)
- [Rime 输入法(Linux 中州韻、Windows 小狼毫、Mac OS 鼠鬚管)](https://github.com/studyzy/imewlconverter/wiki/Rime)
- [Win10 微软拼音](https://github.com/studyzy/imewlconverter/wiki/Win10Ms_Pinyin)
- [Win10 微软五笔](https://github.com/studyzy/imewlconverter/wiki/Win10Ms_Wubi)
- [百度拼音 PC（文本词库、bdict 格式）](https://github.com/studyzy/imewlconverter/wiki/Baidu_PC)
- [必应输入法](https://github.com/studyzy/imewlconverter/wiki/Engkoo)
- 仓颉平台
- [谷歌拼音](https://github.com/studyzy/imewlconverter/wiki/Google_Pinyin)
- [极点五笔](https://github.com/studyzy/imewlconverter/wiki/Jidian)
- [极点郑码](https://github.com/studyzy/imewlconverter/wiki/Jidian)
- [灵格斯词库 ld2](https://github.com/studyzy/imewlconverter/wiki/Lingoes_Ld2)
- [拼音加加](https://github.com/studyzy/imewlconverter/wiki/Pinyin_Jiajia)
- [青简输入法（TSV 词库）](https://qingjian.app/docs/settings/dictionaries)
- 青简词库（.qj 二进制格式）
- 手心输入法
- [搜狗拼音（文本词库、Bin 格式备份词库和 scel 格式细胞词库）](https://github.com/studyzy/imewlconverter/wiki/Sougou_Pinyin)
- 搜狗用户自定义短语（仅导出）
- [搜狗五笔](https://github.com/studyzy/imewlconverter/wiki/Sougou_Wubi)（「五笔编码 词语」文本词库，对应 `wb86` / `wb98` / `wbnewage`）
- [微软拼音 2010](https://github.com/studyzy/imewlconverter/wiki/Ms_Pinyin)
- [小小输入法（拼音、五笔、郑码、二笔）](https://github.com/studyzy/imewlconverter/wiki/Xiaoxiao)
- [小鸭五笔](https://github.com/studyzy/imewlconverter/wiki/Xiaoya_Wubi)
- [新浪拼音](https://github.com/studyzy/imewlconverter/wiki/Sina_Pinyin)
- [雅虎奇摩输入法（注音）](https://github.com/studyzy/imewlconverter/wiki/Yahoo)
- [紫光拼音（文本词库和 uwl 格式分类词库）](https://github.com/studyzy/imewlconverter/wiki/Ziguang_Pinyin)
- 自定义格式
- Win10 微软拼音（自学习词汇）
- QQ 拼音英文
- 百度拼音备份词库 bin
- 二笔输入法（仅导出）
- 超音速录
- Emoji（仅导入）
- Fcitx5 / libime 拼音词库（文本与二进制）
- 水杉输入法（全拼）
- 极点五笔 .mb 文件（仅导入）
- Rime UserDb 用户词典（仅导入）
- 无拼音纯汉字

手机端：

- QQ 手机拼音
- 百度手机拼音（文本词库和 bcd 格式，支持导入导出）
- 百度手机英文
- 谷歌拼音输入法
- [Gboard user_dict_3_3](https://github.com/studyzy/imewlconverter/wiki/GBoard_Binary)
- Gboard 文本词库
- 讯飞输入法

该软件支持以下输入法编码方法（`CodeType`）：

- 仓颉（五代）
- 二笔（超强二笔、青松二笔、现代二笔、隐形二笔）
- 拼音（全拼、双拼）
- 五笔（86、98、新世纪）
- 郑码
- 注音
- 超音（超音速录）
- 自定义

## 命令行使用

### Windows GUI 程序的命令行模式

Windows 版 `深蓝词库转换.exe` 支持双模式：

- **双击运行**（无参数）：打开图形界面
- **带参数运行**：作为命令行工具使用

```bat
深蓝词库转换.exe --help
深蓝词库转换.exe -i scel -o qqpy -O output.txt input.scel
```

> 注意：GUI 程序为窗口子系统，输出通过附着父控制台实现。交互式命令行（cmd、PowerShell、Windows Terminal）下输出可见，但**不支持输出重定向和管道**（`深蓝词库转换.exe --help > help.txt` 会得到空文件）。需要重定向/管道/脚本调用的场景请使用 `ImeWlConverterCmd.exe`。

### 基本语法

```bash
# 使用 dotnet 运行（将 <dll路径> 替换为实际的 DLL 路径）
dotnet <dll路径> -i <输入格式> -o <输出格式> -O <输出文件> <输入文件>

# 如果设置了别名或使用 dotnet run
imewlconverter -i <输入格式> -o <输出格式> -O <输出文件> <输入文件>
```

### 格式代码速查表

下表列出当前代码注册的全部 58 个格式代码（56 个可导入、49 个可导出），按格式代码排序：

| 格式代码 | 说明 | 文件扩展名 | 支持导入 | 支持导出 |
|---------|------|-----------|:-------:|:-------:|
| `bcd` | 百度手机 bcd 词库 | .bcd | ✅ | ✅ |
| `bdict` | 百度分类词库 bdict | .bdict | ✅ | ❌ |
| `bdpy` | 百度拼音文本格式 | .txt | ✅ | ✅ |
| `bdpybin` | 百度拼音备份词库 bin | .bin | ✅ | ❌ |
| `bdsj` | 百度手机 / Mac 版百度拼音 | .txt | ✅ | ✅ |
| `bdsje` | 百度手机英文 | .txt | ✅ | ✅ |
| `bing` | 必应输入法 | .txt | ✅ | ✅ |
| `cjpt` | 仓颉平台 | .txt | ✅ | ✅ |
| `cysl` | 超音速录 | .txt | ✅ | ✅ |
| `dy` | 搜狗用户自定义短语 | .txt | ❌ | ✅ |
| `emoji` | Emoji | .txt | ✅ | ❌ |
| `erbi` | 二笔输入法 | .txt | ❌ | ✅ |
| `fit` | FIT 输入法 (Mac) | .txt | ✅ | ✅ |
| `gboard` | Gboard 文本词库 | .txt | ✅ | ✅ |
| `gboardbin` | Gboard user_dict_3_3 | .dict | ✅ | ✅ |
| `ggpy` | 谷歌拼音 | .txt | ✅ | ✅ |
| `ifly` | 讯飞输入法 | .txt | ✅ | ✅ |
| `jd` | 极点五笔 | .txt | ✅ | ✅ |
| `jdmb` | 极点五笔 .mb 文件 | .mb | ✅ | ❌ |
| `jdzm` | 极点郑码 | .txt | ✅ | ✅ |
| `ld2` | 灵格斯词库 ld2 | .ld2 | ✅ | ❌ |
| `libimebin` | Fcitx5 / libime 二进制拼音词库 | .dict | ✅ | ✅ |
| `libimetxt` | Fcitx5 / libime 文本拼音词库 | .txt | ✅ | ✅ |
| `libpy` | libpinyin (Linux) | .txt | ✅ | ✅ |
| `mspy` | 微软拼音 2010 | .dctx | ✅ | ✅ |
| `plist` | macOS 系统拼音 | .plist | ✅ | ✅ |
| `pyim` | Chinese-pyim (Linux) | .txt | ✅ | ✅ |
| `pyjj` | 拼音加加 | .txt | ✅ | ✅ |
| `qcel` | QQ 分类词库 qcel | .qcel | ✅ | ❌ |
| `qingjiantsv` | 青简 TSV | .tsv | ✅ | ✅ |
| `qj` | 青简词库（.qj 二进制） | .qj | ✅ | ✅ |
| `qpyd` | QQ 分类词库 qpyd | .qpyd | ✅ | ❌ |
| `qqdy` | QQ 输入法自定义短语（.ini，编码=排序位置,词语） | .ini | ✅ | ✅ |
| `qqpy` | QQ 拼音文本格式 | .txt | ✅ | ✅ |
| `qqpye` | QQ 拼音英文 | .txt | ✅ | ✅ |
| `qqsj` | QQ 手机拼音 | .txt | ✅ | ✅ |
| `qqwb` | QQ 五笔 | .txt | ✅ | ✅ |
| `rime` | Rime 输入法 | .yaml | ✅ | ✅ |
| `rimedb` | Rime UserDb 用户词典 | .txt | ✅ | ❌ |
| `scel` | 搜狗拼音细胞词库 | .scel | ✅ | ✅ |
| `self` | 自定义格式 | .txt | ✅ | ✅ |
| `sgpy` | 搜狗拼音文本格式 | .txt | ✅ | ✅ |
| `sgpybin` | 搜狗拼音备份词库 | .bin | ✅ | ✅ |
| `sspy` | 水杉输入法全拼词库（设置 → 词库 → 全拼 → 批量导入） | .txt | ✅ | ✅ |
| `sxpy` | 手心输入法 | .txt | ✅ | ✅ |
| `uwl` | 紫光拼音分类词库 uwl | .uwl | ✅ | ❌ |
| `wb86` | 五笔 86 版 | .txt | ✅ | ✅ |
| `wb98` | 五笔 98 版 | .txt | ✅ | ✅ |
| `wbnewage` | 五笔新世纪版 | .txt | ✅ | ✅ |
| `win10mspy` | Win10 微软拼音（用户自定义短语） | .dat | ✅ | ✅ |
| `win10mspyss` | Win10 微软拼音（自学习词汇） | .dat | ✅ | ✅ |
| `win10mswb` | Win10 微软五笔（用户自定义短语） | .dat | ✅ | ✅ |
| `word` | 无拼音纯汉字 | .txt | ✅ | ✅ |
| `xiaoxiao` | 小小输入法 | .txt | ✅ | ✅ |
| `xlpy` | 新浪拼音 | .txt | ✅ | ✅ |
| `xywb` | 小鸭五笔 | .txt | ✅ | ✅ |
| `yahoo` | 雅虎奇摩输入法（注音） | .txt | ✅ | ✅ |
| `zgpy` | 华宇紫光拼音 | .txt | ✅ | ✅ |

> 「文件扩展名」为该格式的典型文件名提示；代码中未显式声明扩展名的格式默认注册为 `.txt`，实际输入文件不受此限制。运行 `--list-formats` 可查看所安装版本实际注册的格式。

### 常用示例

**搜狗 scel 细胞词库转搜狗拼音 txt 格式**：

```bash
dotnet ImeWlConverterCmd.dll -i scel -o sgpy -O output.txt input.scel
```

**搜狗 scel 细胞词库转谷歌拼音格式**：

```bash
dotnet ImeWlConverterCmd.dll -i scel -o ggpy -O output.txt input.scel
```

**搜狗 scel 细胞词库转 Rime 格式**：

```bash
dotnet ImeWlConverterCmd.dll -i scel -o rime -O output.yaml input.scel
```

**搜狗拼音 txt 转百度手机 bcd 词库**：

```bash
dotnet ImeWlConverterCmd.dll -i sgpy -o bcd -O output.bcd input.txt
```

> bcd 导出目前写入常规词条区：缺少可映射拼音、含代理对汉字或无法编码符号的词条会被跳过。

**多文件转换**：

```bash
dotnet ImeWlConverterCmd.dll -i scel -o ggpy -O output.txt file1.scel file2.scel file3.scel
```

**批量转换到目录**（输出目录以 `/` 结尾）：

```bash
dotnet ImeWlConverterCmd.dll -i scel -o ggpy -O ./output/ *.scel
```

**使用过滤器**：

```bash
dotnet ImeWlConverterCmd.dll -i scel -o ggpy -O output.txt -f "len:1-100|rm:eng|rm:num" input.scel
```

**自定义 scel 词库元数据**（导出为 scel 格式时，可自定义词库编号、名称、类别和描述）：

```bash
dotnet ImeWlConverterCmd.dll -i scel -o scel \
  --dict-id 888888 \
  --dict-name "我的词库" \
  --dict-category "IT科技" \
  --dict-description "自定义描述" \
  -O output.scel input.scel
```

> 说明：这些选项仅影响带内嵌元数据的导出格式（目前为搜狗 `.scel`、百度手机 `.bcd` 与青简 `.qj`）：`.scel` 写入编号、名称、类别和描述；`bcd` 写入编号、名称和描述，类别字段暂不写入；`.qj` 仅使用名称。未指定的字段使用默认值（名称：深蓝词库转换，类别：自定义，编号：自动生成）。

**输出双拼词库**（`--shuangpin`，在编码生成之后把全拼替换为双拼码）：

```bash
dotnet ImeWlConverterCmd.dll -i scel -o gboard --shuangpin xiaohe -O output.txt input.scel
```

可用方案：`full`（全拼，默认）、`xiaohe`、`ziranma`、`microsoft`/`ms`、`abc`、`ziguang`、`pyjj`、`xingkong`、`daniu`、`xiaolang`，也可直接传 `Shuangpin.txt` 表头的中文名（如 `小鹤双拼`）。Windows GUI 在选择 Gboard / Win10 微软拼音等目标格式时弹出的「编码方案」下拉框、macOS GUI 主窗口的「双拼方案」下拉框等价于该选项。

`--shuangpin` 只作用于拼音编码；若 `-t/--code-type` 或目标格式推断出五笔/郑码等非拼音编码，命令会直接报错而不是静默忽略。

注意双拼码不可逆（多个全拼可能映射到同一双拼码），本工具不做无损反解。若需要全拼词库，请从**不带编码**的源重新转换，例如 `-i word`（无拼音纯汉字）配合 `-t pinyin`，或 `-i gboard`（该格式导入的编码被视为用户自定义，管道会按汉字重新生成全拼）。若源格式把编码标为拼音（如 `self`），已有编码会被保留，`-t pinyin` 不会强制覆盖。

**查看帮助和格式列表**：

```bash
dotnet ImeWlConverterCmd.dll --help
dotnet ImeWlConverterCmd.dll --list-formats
```

### 完整使用示例（从安装到转换）

以下示例展示从克隆仓库到完成词库转换的完整流程：

```bash
# 1. 克隆并构建
git clone https://github.com/studyzy/imewlconverter.git
cd imewlconverter
dotnet build src/ImeWlConverterCmd

# 2. 将搜狗 scel 词库转为搜狗拼音 txt 格式
dotnet src/ImeWlConverterCmd/bin/Debug/net10.0/ImeWlConverterCmd.dll \
  -i scel -o sgpy -O 唐诗300首.txt \
  "src/ImeWlConverterCoreTest/Test/唐诗300首【官方推荐】.scel"

# 3. 将搜狗 scel 词库转为 Rime 格式
dotnet src/ImeWlConverterCmd/bin/Debug/net10.0/ImeWlConverterCmd.dll \
  -i scel -o rime -O 唐诗300首.yaml \
  "src/ImeWlConverterCoreTest/Test/唐诗300首【官方推荐】.scel"
```

也可以使用 `dotnet run` 方式运行（无需关心 DLL 路径）：

```bash
cd imewlconverter
dotnet run --project src/ImeWlConverterCmd -- \
  -i scel -o sgpy -O 唐诗300首.txt \
  "src/ImeWlConverterCoreTest/Test/唐诗300首【官方推荐】.scel"
```

### 重要说明

从 v3.4.0 开始，命令行参数格式已更新为 GNU 风格。如果您使用的是旧格式（如 `-i:scel`），请参阅 [MIGRATION.md](docs/MIGRATION.md) 进行迁移。

## 集成测试

本项目包含完整的集成测试框架，确保各种输入法格式之间的转换正确性。

### 快速运行测试

```bash
# 推荐：Makefile 目标会先构建 CLI，再以同一配置运行集成测试
make integration-test
```

也可手动执行，注意构建配置与测试脚本必须一致（`run-tests.sh` 通过 `DOTNET_CONFIG` 选择 `bin/<配置>/net10.0/`，默认 `Debug`）：

```bash
# Debug（默认）
dotnet build src/ImeWlConverterCmd
cd tests/integration && ./run-tests.sh --all

# Release：构建用 -c Release，测试脚本用 DOTNET_CONFIG=Release 选同一产物
dotnet build src/ImeWlConverterCmd -c Release
cd tests/integration && DOTNET_CONFIG=Release ./run-tests.sh --all
```

### 测试覆盖范围

当前已实现：
- ✅ 导入测试（多种输入格式 → 统一CSV格式）
- ✅ 导出测试（统一CSV格式 → 多种输出格式）
- ✅ 高级功能测试（过滤、编码、大文件性能）
- ✅ GUI 转换矩阵测试（Windows，驱动 WinForms 界面执行 38 条导入→导出转换路径及词库合并、文件分割等工具用例，共 43 个用例，需交互桌面会话）
- ✅ 实机端到端测试（Win10 微软拼音候选验证）

详细的测试矩阵与数据来源见 `tests/integration/TEST-MATRIX.md`。

```powershell
# Windows GUI 转换矩阵测试（需交互桌面会话）
powershell -ExecutionPolicy Bypass -File tests\integration\windows-gui-matrix.ps1
```

### 更多信息

- [集成测试完整文档](tests/integration/README.md)
- [测试用例配置规范](specs/001-integration-tests/contracts/test-case-schema.yaml)
- [测试数据来源](src/ImeWlConverterCoreTest/Test/)

## Star History

[![Star History Chart](https://star-history.dera.page/svg?repos=studyzy/imewlconverter&type=Date)](https://star-history.dera.page/#studyzy/imewlconverter&Date)
