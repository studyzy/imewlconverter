[path]
/Users/devinzeng/Code/studyzy/imewlconverter/tests/integration/docs/adding-tests.md
[instructions]
I will add a Chinese guide describing how to add new integration test cases in under 10 minutes.
[code_edit]
# 添加集成测试用例指南（<10分钟）

本指南帮助你在 10 分钟内完成一个新的集成测试用例。

## 1. 选择或创建测试套件目录

优先复用已有套件（如 `test-cases/1-imports`、`test-cases/2-exports`、`test-cases/3-advanced`）。

如果是新格式，创建目录：

```bash
mkdir -p test-cases/new-format
```

## 2. 准备输入数据

建议复用 `src/ImeWlConverterCoreTest/Test/` 中的真实测试文件，保证数据可靠：

```yaml
input:
  file: "../../../../src/ImeWlConverterCoreTest/Test/示例词库.scel"
  format: "sougou"
```

## 3. 生成预期输出

在仓库根目录执行：

```bash
dotnet run --project src/ImeWlConverterCmd -- \
  -i:src/ImeWlConverterCoreTest/Test/示例词库.scel \
  -o:tests/integration/test-cases/new-format/example.expected \
  -f:sougou -t:text
```

## 4. 编写 `test-config.yaml`

可从已有套件复制模板：

```bash
cp test-cases/1-imports/test-config.yaml test-cases/new-format/test-config.yaml
```

编辑后至少包含：

- `test_suite.name`
- `test_suite.description`
- `test_cases[]`（每条用例包含 `input`、`output`、`expected`）

## 5. 添加说明文档

在套件目录创建 `README.md`，说明：

- 测试场景
- 数据来源
- 维护注意事项

## 6. 运行验证

```bash
./run-tests.sh -s new-format
```

若失败可加 `-v` 获取详细日志：

```bash
./run-tests.sh -s new-format -v
```

## 常见建议

- 文件路径使用相对路径，避免硬编码绝对路径
- 输出文件统一放在 `expected/` 目录
- 遇到编码问题时优先检查输入文件编码
