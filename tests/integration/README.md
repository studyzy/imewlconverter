# 词库转换集成测试框架

这是一个基于Shell脚本的集成测试框架，用于验证 ImeWlConverter 词库转换工具的功能正确性。

## 特点

- **零依赖**: 仅使用Shell脚本和系统自带工具（Bash、diff、AWK、Python3）
- **黑盒测试**: 不依赖被测代码的内部实现，通过CLI接口测试
- **易于维护**: Shell脚本简单直观，学习曲线低
- **跨平台**: 支持 Windows（Git Bash）、Linux、macOS
- **CI友好**: 生成JUnit XML报告，无缝集成GitHub Actions
- **真实数据**: 直接使用 `src/ImeWlConverterCoreTest/Test/` 中的真实测试文件

## 快速开始

### 前置条件

1. 已构建 ImeWlConverterCmd CLI工具：
   ```bash
   dotnet build
   ```

2. 系统要求：
   - Bash 4.0+ 或 Zsh
   - diff 工具
   - AWK
   - Python 3（用于路径规范化）

### 运行测试

```bash
# 进入测试目录
cd tests/integration

# 运行所有测试
./run-tests.sh --all

# 推荐：运行优化后的imports测试套件（v2.0）
./run-tests.sh -s imports

# 运行特定测试套件（旧版格式）
./run-tests.sh -s sougou-scel

# 运行带详细输出的测试
./run-tests.sh -s imports -v

# 运行特定标签的测试
./run-tests.sh -t basic

# 保留测试输出文件用于调试
./run-tests.sh -s imports --keep-output

# 生成JUnit XML报告（用于CI）
./run-tests.sh --all --xml
```

### 查看帮助

```bash
./run-tests.sh -h
```

## GUI 版集成测试（Windows）

除 CLI 黑盒测试外，还有两个驱动 **WinForms 图形界面** 的集成测试脚本（PowerShell + UI Automation + Win32 消息），验证用户实际操作的完整路径（填路径 → 选格式 → 配置框 → 转换 → 保存对话框）。

> 前提：Windows 交互桌面会话；脚本会操作真实鼠标键盘，运行期间请勿操作。不适用于无头 CI。

| 脚本 | 用途 |
|------|------|
| `windows-gui-matrix.ps1` | **GUI 转换矩阵**：25 条导入→导出转换路径（见 TEST-MATRIX.md 的 GUI 章节），覆盖 25 种导入格式 |
| `windows-ime-e2e.ps1` | **实机端到端**：纯汉字 → Win10微软拼音 UDP 词库，替换系统词库后断言输入法候选窗真实出词 |

```powershell
# GUI 转换矩阵（推荐先跑）
powershell -ExecutionPolicy Bypass -File tests\integration\windows-gui-matrix.ps1
powershell -ExecutionPolicy Bypass -File tests\integration\windows-gui-matrix.ps1 -List     # 仅列出用例
powershell -ExecutionPolicy Bypass -File tests\integration\windows-gui-matrix.ps1 -Only A1  # 只跑单条

# Win10 微软拼音实机验证（会临时替换系统 UDP 词库，结束自动恢复）
powershell -ExecutionPolicy Bypass -File tests\integration\windows-ime-e2e.ps1
```

共享的 UI 自动化辅助函数位于 `lib/gui-automation.ps1`。C 组用例的样本文件缺失时会自动 SKIP 并在汇总末尾列出待准备文件清单（放入 `src/ImeWlConverterCoreTest/Test/` 后自动生效）。

## 目录结构

```
tests/integration/
├── run-tests.sh              # 主测试运行器（CLI 黑盒测试）
├── lib/                      # 辅助函数库
│   ├── test-helpers.sh       # 核心测试辅助函数
│   ├── yaml-parser.sh        # YAML配置解析
│   ├── color-output.sh       # 彩色终端输出
│   ├── report-generator.sh   # 测试报告生成
│   └── gui-automation.ps1    # ✨ GUI 测试共享 UI 自动化库（Win32 + UIA）
├── test-cases/               # 测试用例目录
│   ├── imports/              # ✨ 新：统一的导入测试矩阵（v2.0，推荐）
│   ├── sougou-scel/          # 旧：搜狗拼音.scel格式测试（v1.0）
│   ├── qq-pinyin-txt/        # 旧：QQ拼音文本格式测试（v1.0）
│   ├── qq-qpyd/              # 旧：QQ拼音.qpyd格式测试（v1.0）
│   ├── qq-qcel/              # 旧：QQ拼音.qcel格式测试（v1.0）
│   └── ...                   # 其他格式测试
├── windows-gui-matrix.ps1    # ✨ GUI 转换矩阵测试（Windows，25 条转换路径）
├── windows-ime-e2e.ps1       # ✨ GUI 实机端到端测试（Win10 微软拼音候选验证）
├── test-output/              # 测试临时输出（.gitignore）
├── reports/                  # 测试报告目录
├── TEST-MATRIX.md            # 测试矩阵设计文档
└── MIGRATION-GUIDE.md        # 迁移指南
```

**注意**：推荐使用新的 `imports` 测试套件（v2.0），它采用测试矩阵方法，更简洁高效。详见 [MIGRATION-GUIDE.md](MIGRATION-GUIDE.md)。

## 测试用例配置

每个测试套件在其目录下包含一个 `test-config.yaml` 文件，定义测试用例：

```yaml
test_suite:
  name: "搜狗拼音 .scel 格式转换测试"
  description: "验证搜狗拼音细胞词库(.scel)格式的转换功能"

test_cases:
  - name: "唐诗300首转换为文本"
    description: "基础转换功能测试"
    enabled: true
    timeout: 30
    input:
      file: "../../../../src/ImeWlConverterCoreTest/Test/唐诗300首【官方推荐】.scel"
      format: "sougou"
    output:
      format: "text"
    expected:
      file: "tangshi300-to-text.expected"
    tags: ["basic", "sougou", "text-output"]
```

### 配置字段说明

- **test_suite**: 测试套件元数据
  - `name`: 套件名称（中文）
  - `description`: 套件描述

- **test_cases**: 测试用例列表
  - `name`: 测试用例名称
  - `description`: 测试用例描述
  - `enabled`: 是否启用（true/false）
  - `timeout`: 超时时间（秒）
  - `input.file`: 输入文件路径（支持相对路径）
  - `input.format`: 输入格式（如 sougou、qq、baidu等）
  - `output.format`: 输出格式（如 text、sougou、rime等）
  - `expected.file`: 期望输出文件路径
  - `tags`: 标签列表，用于过滤测试

## 添加新测试用例

详见 [docs/adding-tests.md](docs/adding-tests.md) 获取详细指南。

### 快速步骤

1. **选择或创建测试套件目录**:
   ```bash
   mkdir -p test-cases/new-format
   ```

2. **使用现有测试文件**（推荐）:
   - 在 `test-config.yaml` 中使用相对路径引用 `src/ImeWlConverterCoreTest/Test/` 中的文件
   - 例如：`file: "../../../../src/ImeWlConverterCoreTest/Test/example.scel"`

3. **生成预期输出**:
   ```bash
   # 在仓库根目录执行
   dotnet run --project src/ImeWlConverterCmd -- \
     -i scel -o ggpy -O tests/integration/test-cases/new-format/example.expected \
     src/ImeWlConverterCoreTest/Test/example.scel
   ```

4. **创建配置文件**:
   ```bash
   # 复制模板并修改
   cp test-cases/sougou-scel/test-config.yaml test-cases/new-format/test-config.yaml
   # 编辑 test-config.yaml，更新测试用例信息
   ```

5. **运行测试验证**:
   ```bash
   ./run-tests.sh -s new-format
   ```

6. **添加文档**:
   ```bash
   # 创建 README.md 说明测试数据来源和用途
   vim test-cases/new-format/README.md
   ```

## 测试报告

### 终端报告

测试完成后，会显示如下格式的报告：

```
============================================================
测试报告
============================================================
测试套件: 搜狗拼音 .scel 格式转换测试

通过: 2 / 2 (100.0%)
失败: 0
跳过: 0
总用时: 5.2秒

============================================================
```

### JUnit XML报告

使用 `--xml` 参数可生成JUnit格式的XML报告，适用于CI系统：

```bash
./run-tests.sh --all --xml
# 报告生成在: reports/test-results.xml
```

## CI集成

### GitHub Actions

- 工作流文件: `.github/workflows/integration-tests.yml`
- 触发条件: push / pull_request / 手动触发
- 运行平台: Ubuntu、macOS、Windows
- 产物: `reports/test-results.xml`（JUnit XML）

每次推送到主分支或创建Pull Request时，会自动：
1. 构建项目
2. 运行完整测试套件
3. 生成测试报告
4. 上传测试结果

### 本地模拟CI环境

```bash
# 模拟CI环境运行测试
CI=true ./run-tests.sh --all --xml

# 检查生成的XML报告
cat reports/test-results.xml
```

## 故障排查

### 常见问题

1. **找不到 ImeWlConverterCmd**
   - 确保已构建项目：`dotnet build`
   - 检查可执行文件路径：`ls src/ImeWlConverterCmd/bin/Release/net*/ImeWlConverterCmd*`

2. **测试失败但diff显示无差异**
   - 可能是行尾符问题（CRLF vs LF）
   - 使用 `--keep-output` 保留输出文件，手动检查
   - 使用十六进制编辑器查看文件：`hexdump -C file`

3. **测试超时**
   - 检查 `timeout` 配置是否足够
   - 对于大文件，建议设置 `timeout: 60` 或更高

4. **编码问题**
   - 确保输入文件编码正确（UTF-8、GBK等）
   - 检查系统locale设置：`locale`

5. **路径问题**
   - 相对路径从 `test-config.yaml` 所在目录开始解析
   - 使用 `-v` 参数查看实际使用的绝对路径

6. **XML报告格式校验失败**
   - 确认 `xmllint` 可用（macOS: `brew install libxml2`）
   - 临时跳过校验可移除本地 `xmllint` 命令

### 调试技巧

```bash
# 查看详细执行日志
./run-tests.sh -s sougou-scel -v

# 保留测试输出文件
./run-tests.sh -s sougou-scel --keep-output
# 输出在: test-output/<suite-name>/<test-name>.actual

# 手动运行转换命令
dotnet run --project ../../src/ImeWlConverterCmd -- \
  -i scel -o ggpy -O test-output.txt test-input.scel

# 比较输出差异
diff -u expected.txt actual.txt
```

## 性能基准

### 预期性能指标

- **单个测试**（文件<1MB）: <10秒
- **完整测试套件**（10+种格式、30+用例）: <5分钟
- **大文件测试**（诗词名句大全.scel ~3MB）: <60秒

### 性能优化建议

1. **并行执行**: 未来可考虑实现测试并行执行
2. **增量测试**: 仅运行受影响的测试套件
3. **缓存机制**: 缓存转换结果用于重复测试

## 设计决策

### 为什么选择Shell脚本？

1. **黑盒测试特性**: 测试框架不需要访问被测代码的内部实现
2. **零依赖**: 不需要安装额外的测试框架或运行时
3. **跨平台**: Shell脚本在所有主流操作系统上都可运行
4. **简单直观**: 易于理解和维护，学习曲线低
5. **CI友好**: 可直接在CI环境中运行，无需额外配置

### 数据复用策略

本测试框架直接引用 `src/ImeWlConverterCoreTest/Test/` 中的测试文件，而不是复制到集成测试目录。

**优点**：
- **单一数据源**: 避免数据不一致
- **自动同步**: 单元测试数据更新时，集成测试自动使用最新数据
- **减少仓库体积**: 避免重复存储相同文件
- **维护简单**: 只需维护一份测试数据

**实现方式**：
- 在 `test-config.yaml` 中使用相对路径：`../../../../src/ImeWlConverterCoreTest/Test/文件名`
- YAML解析器自动规范化路径为绝对路径
- 详见各测试套件的 README 了解具体路径配置

## 测试覆盖范围

### ✨ 推荐：imports测试套件（v2.0）

**设计理念**：一个测试用例 = 一条转换路径（输入格式A → 输出格式B）

当前支持的转换路径：

| 测试ID | 输入格式 | → | 输出格式 | 词条数 | 状态 |
|--------|---------|---|---------|--------|------|
| T1 | 搜狗.scel | → | 搜狗文本 | 3,563 | ✅ |
| T2 | QQ拼音.txt | → | 搜狗文本 | 4 | ✅ |
| T3 | QQ拼音.qpyd | → | 搜狗文本 | 1,657 | ✅ |
| T4 | QQ拼音.qcel | → | 搜狗文本 | 4,675 | ✅ |
| T6 | Rime | → | 搜狗文本 | 17 | ✅ |
| T9 | 纯汉字 | → | 搜狗文本 | 59 | ✅ |

**优势**：
- 🎯 **测试目的明确**：每个测试清楚表示一条转换路径
- 📝 **统一配置**：单一配置文件，易于维护
- ⚡ **高效执行**：~1.7秒完成6个测试
- 🔧 **易于扩展**：添加新格式只需一个配置条目

详见：[imports/README.md](test-cases/imports/README.md) 和 [TEST-MATRIX.md](TEST-MATRIX.md)

### 旧版：按格式分组的测试套件（v1.0）

保留用于向后兼容：

- [x] 搜狗拼音细胞词库(.scel)格式
  - 基本转换测试（3563词条）
  - 大文件性能测试（342179词条）- 跳过
- [x] QQ拼音（文本、qpyd、qcel格式）
  - 文本格式（4词条）
  - 英文词条（2词条）
  - 分类词库qpyd（1657词条）
  - 分类词库qcel（4675词条，含英文）
- [ ] 百度拼音（bdict格式）- 待实施
- [ ] Rime输入法 - v2.0已支持
- [ ] 灵格斯词典（ld2格式）- 待实施
- [ ] 编码测试（GBK、UTF-8等）- 待实施

**总测试用例数**：
- v2.0 (imports): 6个测试用例
- v1.0 (legacy): 6个测试用例  
- 总计：12个（包含重复）

## 贡献指南

### 添加新格式支持

1. 在 `test-cases/` 下创建新目录
2. 准备测试数据和预期输出
3. 创建 `test-config.yaml` 配置文件
4. 添加 `README.md` 说明
5. 运行测试验证
6. 提交Pull Request

### 代码规范

- 遵循 [Google Shell Style Guide](https://google.github.io/styleguide/shellguide.html)
- 使用 shellcheck 进行静态检查
- 所有注释和文档使用中文
- 函数名使用下划线分隔：`function_name()`
- 变量使用大写：`VARIABLE_NAME`

## 许可证

本测试框架遵循与主项目相同的许可证。

## 相关资源

- [项目主页](../../README.md)
- [添加测试指南](docs/adding-tests.md)
- [故障排查指南](docs/troubleshooting.md)
- [ImeWlConverter 文档](https://github.com/studyzy/imewlconverter)

## 联系方式

如有问题或建议，请提交 Issue 或 Pull Request。
