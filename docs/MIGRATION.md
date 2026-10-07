# 命令行参数迁移指南

本文档帮助您从旧的参数格式迁移到新的 GNU 风格参数格式。

## 📋 概述

从 3.0.0 版本开始，IME WL Converter 采用标准的 GNU 风格命令行参数格式，替代了之前的冒号分隔格式。

**这是一个 BREAKING CHANGE**，需要更新现有脚本和命令。

## 🔄 快速对照表

| 旧格式 | 新格式（长选项） | 新格式（短选项） |
|--------|-----------------|-----------------|
| `-i:scel` | `--input-format scel` | `-i scel` |
| `-o:ggpy` | `--output-format ggpy` | `-o ggpy` |
| 路径作为参数 | `--output <path>` | `-O <path>` |
| 路径作为参数 | `<input-files>...`（位置参数） | 同左 |
| `-c:path` | `--code-file <path>` | `-c <path>` |
| `-f:spec` | `--custom-format <spec>` | `-F <spec>` |
| `-ft:filter` | `--filter <filter>` | `-f <filter>` |
| `-r:type` | `--rank-generator <number>` | `-r <number>`（仅数字；baidu/google 已移除） |
| `-ct:type` | `--code-type <type>` | `-t <type>` |
| `-os:os` | `--target-os <os>` | （仅长选项） |
| `-mc:rules` | `--multi-code <rules>` | `-m <rules>` |
| `-ld2:enc` | `--ld2-encoding <enc>` | （仅长选项） |
| `-h` | `--help` | `-h` |
| `-v` | `--version` | `-v` |

## 📝 迁移示例

### 基本转换

**旧格式：**
```bash
dotnet ImeWlConverterCmd.dll -i:scel input.scel -o:ggpy output.txt
```

**新格式：**
```bash
# 使用长选项
imewlconverter --input-format scel --output-format ggpy --output output.txt input.scel

# 使用短选项（推荐）
imewlconverter -i scel -o ggpy -O output.txt input.scel
```

### 多文件转换

**旧格式：**
```bash
dotnet ImeWlConverterCmd.dll -i:scel ./test.scel ./a.scel -o:ggpy ./gg.txt
```

**新格式：**
```bash
imewlconverter -i scel -o ggpy -O output.txt test.scel a.scel
```

### 批量转换到目录

**旧格式：**
```bash
dotnet ImeWlConverterCmd.dll -i:scel ./test/*.scel -o:ggpy ./temp/*
```

**新格式：**
```bash
imewlconverter -i scel -o ggpy -O ./temp/ *.scel
```

注意：输出目录路径需要以 `/` 结尾。

### 使用过滤器

**旧格式：**
```bash
-ft:"len:1-100|rank:2-9999|rm:eng|rm:num"
```

**新格式：**
```bash
--filter "len:1-100|rank:2-9999|rm:eng|rm:num"
# 或
-f "len:1-100|rank:2-9999|rm:eng|rm:num"
```

### 自定义格式和编码文件

**旧格式：**
```bash
dotnet ImeWlConverterCmd.dll -i:qpyd ./a.qpyd -o:self ./zy.txt "-f:213, nyyn" -c:./code.txt
```

**新格式：**
```bash
imewlconverter -i qpyd -o self -O zy.txt -F "213, nyyn" -c code.txt a.qpyd
```

### 使用词频生成器

**旧格式：**
```bash
-r:100
```

**新格式：**
```bash
--rank-generator 100
# 或
-r 100
```

指定固定词频数字（如 `-r 100`）时，所有词条的词频会被强制覆盖为该数字。

> ⚠️ 旧版的 `-r:baidu` / `-r:google` 在线词频生成器已移除，不再支持；
> 传入非数字值会报 `invalid-rank` 错误（退出码 1）。

### Rime 输出配置

**旧格式：**
```bash
-ct:pinyin -os:macos
```

**新格式：**
```bash
--code-type pinyin --target-os macos
# 或
-t pinyin --target-os macos
```

## 🔍 关键变化说明

### 0. 退出码与 JSON 输出契约（新增）

自本次重构起，CLI 提供稳定的退出码契约（变更会记录在本文件）：

| 退出码 | 含义 |
|--------|------|
| 0 | 成功 |
| 1 | 用法/参数错误（缺必填项、未知格式 ID、filter/格式 spec 语法错） |
| 2 | 输入错误（文件不存在/不可读、转换失败）——**此前所有错误一律为 1** |
| 3 | 部分失败（≥1 文件成功、≥1 文件失败，明细见错误输出）——**此前为 1** |
| 4 | 未捕获内部错误（默认不输出堆栈，`IMEWL_DEBUG=1` 时输出）——**此前为 1** |

**脚本兼容提示**：旧脚本通常只判断 `退出码 != 0`，不受影响；若依赖"非 0 即 1"的精确值，请改为 `!= 0` 判断。

新增 `--json` 输出（机器可读，供 AI 代理/脚本消费）：

```jsonc
// 成功（stdout）：
{ "schema": 1, "ok": true,
  "result": { "imported": 1200, "exported": 1180, "filtered": 20,
              "outputs": [{ "path": "out.txt", "entries": 1180 }],
              "errors": [] } }
// 失败：
{ "schema": 1, "ok": false,
  "error": { "code": "unknown-format", "target": "--input-format", "message": "未知的输入格式: xxx" } }
```

`error.code` 枚举：`missing-option` / `unknown-format` / `invalid-filter` / `invalid-spec` / `invalid-rank` / `input-not-found` / `conversion-failed` / `internal-error`。
`--list-formats --json` 输出 `{ importFormats[], exportFormats[] }`（含 id/name/isBinary/extension）。
`--json` 模式下进度不写 stderr（除非同时加 `--verbose`）。

### 1. 位置参数顺序

**旧格式**：输入文件和输出文件混在选项中
```bash
-i:scel input1.scel input2.scel -o:ggpy output.txt
```

**新格式**：输入文件作为位置参数放在最后，输出用 `-O` 明确指定
```bash
-i scel -o ggpy -O output.txt input1.scel input2.scel
```

### 2. 参数值分隔

**旧格式**：使用冒号 `:` 分隔选项和值
```bash
-i:scel
```

**新格式**：使用空格分隔选项和值
```bash
-i scel
```

### 3. 自定义格式选项

由于 `-f` 现在用于过滤器（原 `-ft:`），自定义格式改用 `-F`：

```bash
# 旧: -f:213, nyyn
# 新: -F "213, nyyn"
```

### 4. 帮助信息

新格式提供更详细、格式化的帮助信息：

```bash
imewlconverter --help
```

查看所有支持的格式：

```bash
imewlconverter --list-formats
```

## 🔧 更新脚本

### Shell 脚本示例

**旧脚本：**
```bash
#!/bin/bash
for file in *.scel; do
    dotnet ImeWlConverterCmd.dll -i:scel "$file" -o:ggpy "${file%.scel}.txt"
done
```

**新脚本：**
```bash
#!/bin/bash
for file in *.scel; do
    imewlconverter -i scel -o ggpy -O "${file%.scel}.txt" "$file"
done
```

### Python 脚本示例

**旧代码：**
```python
import subprocess

subprocess.run([
    "dotnet", "ImeWlConverterCmd.dll",
    "-i:scel", "input.scel",
    "-o:ggpy", "output.txt"
])
```

**新代码：**
```python
import subprocess

subprocess.run([
    "imewlconverter",
    "-i", "scel",
    "-o", "ggpy",
    "-O", "output.txt",
    "input.scel"
])
```

## 🧪 更新集成测试

如果您有使用旧格式的测试脚本，需要更新命令构建逻辑。

### 测试框架更新示例

**旧格式：**
```bash
CMD="dotnet ImeWlConverterCmd.dll -i:$INPUT_FORMAT -o:$OUTPUT_FORMAT"
```

**新格式：**
```bash
CMD="imewlconverter -i $INPUT_FORMAT -o $OUTPUT_FORMAT -O $OUTPUT_PATH"
```

完整示例见 `tests/integration/lib/test-helpers.sh`。

## ⚠️ 常见问题

### Q: 旧格式还能用吗？

**A:** 不能。新版本完全移除了对旧格式的支持。运行旧格式命令时会显示清晰的错误提示和迁移指引。

### Q: 如何快速检查是否使用了旧格式？

**A:** 如果命令中包含 `-i:`、`-o:`、`-c:` 等冒号分隔的参数，就是旧格式。运行时会立即收到错误提示。

### Q: 批量转换的 `*` 通配符还能用吗？

**A:** 能用，但语法略有不同：

```bash
# 旧: -i:scel *.scel -o:ggpy ./output/*
# 新: -i scel -o ggpy -O ./output/ *.scel
```

注意输出目录需要以 `/` 结尾，输入文件作为位置参数。

### Q: 如何在 CI/CD 中更新？

**A:** 搜索您的 CI 配置文件（如 `.github/workflows/*.yml`、`Makefile`、`.gitlab-ci.yml`）中的旧格式参数，按照本指南更新。

## 📚 其他资源

- [README.md](../README.md) - 完整使用文档
- [CODEBUDDY.md](../CODEBUDDY.md) - 开发者指南
- 运行 `imewlconverter --help` 查看完整帮助

## 🆘 需要帮助？

如果迁移遇到问题：

1. 检查本文档的示例
2. 运行 `imewlconverter --help` 查看最新用法
3. 查看 [GitHub Issues](https://github.com/studyzy/imewlconverter/issues)
4. 提交新 Issue 描述您的问题
