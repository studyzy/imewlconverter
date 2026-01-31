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
| `-r:type` | `--rank-generator <type>` | `-r <type>` |
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
-r:baidu
-r:google
-r:100
```

**新格式：**
```bash
--rank-generator baidu
# 或
-r baidu
-r google
-r 100
```

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

- [README.md](README.md) - 完整使用文档
- [CLAUDE.md](CLAUDE.md) - 开发者指南
- 运行 `imewlconverter --help` 查看完整帮助

## 🆘 需要帮助？

如果迁移遇到问题：

1. 检查本文档的示例
2. 运行 `imewlconverter --help` 查看最新用法
3. 查看 [GitHub Issues](https://github.com/studyzy/imewlconverter/issues)
4. 提交新 Issue 描述您的问题
