# 技术研究: 词库转换集成测试框架

**日期**: 2026-01-25
**状态**: 完成
**目的**: 为词库转换集成测试选择合适的技术栈和实施方案

## 研究背景

用户需求明确：
- 在CI环境下验证词库转换的正确性
- 只考虑命令行验证
- 不希望使用C#编写测试（希望降低维护成本）
- 需要技术栈选型建议

项目现状：
- 已有命令行工具 `ImeWlConverterCmd`（C#编写）
- 支持20+种输入法格式
- 跨平台支持（Windows、Linux、macOS）
- 代码历史较长，存在Bug隐患

## 技术选型研究

### 1. 测试框架语言选择

#### 选项A: Shell脚本（Bash/Zsh）✅ **推荐**

**优点**：
- ✅ 无需额外安装依赖（所有平台都支持）
- ✅ 直接调用命令行工具，天然适合集成测试
- ✅ 易于在CI环境中运行（GitHub Actions原生支持）
- ✅ 学习曲线低，维护成本低
- ✅ 可以使用标准Unix工具（diff、grep等）
- ✅ 适合黑盒测试场景

**缺点**：
- ❌ 复杂逻辑处理能力有限
- ❌ 跨平台兼容性需要额外处理（Windows需要Git Bash或WSL）
- ❌ 缺乏现代测试框架的高级特性

**决策**: 选择Shell脚本作为主要实现语言

**理由**: 
1. 用户明确表示"不需要再用C#"，希望降低复杂度
2. 集成测试本质上是命令行调用和文件比较，Shell脚本完全胜任
3. CI集成简单，无需构建步骤
4. 维护成本最低

---

#### 选项B: Python + pytest

**优点**：
- ✅ 强大的测试框架生态（pytest、unittest）
- ✅ 跨平台支持好
- ✅ 丰富的文本处理和报告生成库
- ✅ 易于编写复杂的验证逻辑

**缺点**：
- ❌ 需要Python运行时（额外依赖）
- ❌ 增加项目复杂度
- ❌ 不符合用户"不想引入新语言"的期望

**决策**: 不选择

---

#### 选项C: Node.js + Jest

**优点**：
- ✅ 现代化的测试框架
- ✅ 强大的断言和报告功能
- ✅ 跨平台支持好

**缺点**：
- ❌ 需要Node.js运行时和npm依赖管理
- ❌ 引入新的技术栈
- ❌ 对于简单的文件比较测试过于复杂

**决策**: 不选择

---

#### 选项D: C# + xUnit/NUnit

**优点**：
- ✅ 与现有代码库语言一致
- ✅ 强大的.NET测试生态
- ✅ 可以深度集成到代码库

**缺点**：
- ❌ 用户明确表示"集成测试不需要再用C#"
- ❌ 需要编译步骤
- ❌ 维护成本相对较高

**决策**: 不选择

---

### 2. 文件比较策略

#### 选项A: diff命令 ✅ **推荐**

**优点**：
- ✅ 系统自带，无需安装
- ✅ 标准输出格式，易于解析
- ✅ 支持多种模式（unified diff、context diff等）
- ✅ 性能优秀

**示例**：
```bash
diff -u expected.txt actual.txt
```

**决策**: 使用diff作为主要比较工具

---

#### 选项B: 自定义比较逻辑

**优点**：
- ✅ 可以实现特定的比较规则（如忽略空行、注释等）

**缺点**：
- ❌ 需要额外开发和维护
- ❌ 可能引入Bug

**决策**: 对于特殊需求，在diff基础上增加预处理脚本

---

### 3. 测试配置格式

#### 选项A: YAML ✅ **推荐**

**优点**：
- ✅ 人类可读性好
- ✅ 支持注释
- ✅ 层次结构清晰
- ✅ 易于手工编辑

**示例**：
```yaml
test_cases:
  - name: "搜狗拼音到QQ拼音基本转换"
    input: "test-data/sougou-pinyin/basic.txt"
    source_format: "SougouPinyin"
    target_format: "QQPinyin"
    expected: "test-data/sougou-pinyin/basic.expected"
    timeout: 10
```

**决策**: 使用YAML作为测试用例配置格式

**工具**: 使用`yq`或简单的`grep`/`awk`解析（避免引入Python/Ruby依赖）

---

#### 选项B: JSON

**优点**：
- ✅ 机器解析容易（jq工具）
- ✅ 标准格式

**缺点**：
- ❌ 不支持注释
- ❌ 人类可读性较差

**决策**: 作为备选方案，可用于生成测试报告

---

### 4. 测试报告格式

#### 选项A: 彩色终端输出 + JUnit XML ✅ **推荐**

**终端输出示例**：
```
✓ [PASS] 搜狗拼音 → QQ拼音 (基本转换) - 2.3s
✗ [FAIL] 搜狗拼音 → 百度拼音 (边界情况) - 1.8s
  差异: 第15行，预期"你好"，实际"您好"
✓ [PASS] QQ五笔 → 搜狗五笔 (编码测试) - 3.1s

总计: 3个测试，2个通过，1个失败
总耗时: 7.2秒
```

**JUnit XML示例**（用于CI集成）：
```xml
<testsuite tests="3" failures="1" time="7.2">
  <testcase name="搜狗拼音到QQ拼音基本转换" time="2.3"/>
  <testcase name="搜狗拼音到百度拼音边界情况" time="1.8">
    <failure>差异: 第15行，预期"你好"，实际"您好"</failure>
  </testcase>
</testsuite>
```

**决策**: 同时生成两种格式
- 彩色终端输出：便于本地调试
- JUnit XML：便于CI系统解析和展示

---

### 5. CI集成策略

#### GitHub Actions工作流设计

**触发条件**：
- 每次push到main分支
- 每次创建Pull Request
- 每天定时运行（检测依赖项变化导致的问题）

**工作流步骤**：
```yaml
name: 集成测试

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]
  schedule:
    - cron: '0 2 * * *'  # 每天凌晨2点

jobs:
  integration-tests:
    strategy:
      matrix:
        os: [ubuntu-latest, macos-latest, windows-latest]
    
    runs-on: ${{ matrix.os }}
    
    steps:
      - uses: actions/checkout@v4
      
      - name: 设置 .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: 构建CLI工具
        run: dotnet build src/ImeWlConverterCmd/ImeWlConverterCmd.csproj
      
      - name: 运行集成测试
        run: bash tests/integration/run-tests.sh
      
      - name: 上传测试报告
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-reports-${{ matrix.os }}
          path: tests/integration/reports/
```

---

### 6. 测试数据管理

#### 决策: 测试数据版本化存储

**目录结构**：
```
tests/integration/test-cases/
├── sougou-pinyin/
│   ├── README.md           # 测试说明
│   ├── test-config.yaml    # 测试配置
│   ├── basic.txt           # 基本测试（5-10个词条）
│   ├── basic.expected      # 预期输出
│   ├── boundary.txt        # 边界情况（特殊字符、生僻字）
│   ├── boundary.expected
│   ├── large.txt           # 大文件测试（1000+词条）
│   └── large.expected
```

**数据特征**：
- 测试文件小型化（<100KB），确保快速执行
- 覆盖典型场景和边界情况
- 每个文件都有对应的README说明
- 所有文件使用UTF-8编码，避免编码问题

---

## 最佳实践研究

### 1. Shell脚本最佳实践

参考：[Google Shell Style Guide](https://google.github.io/styleguide/shellguide.html)

**关键原则**：
- 使用`set -euo pipefail`（严格模式）
- 所有变量使用双引号
- 函数使用snake_case命名
- 全局变量使用UPPER_CASE
- 提供清晰的错误消息
- 每个函数前加注释说明

**示例**：
```bash
#!/usr/bin/env bash
set -euo pipefail

# 运行单个测试用例
# 参数:
#   $1 - 输入文件路径
#   $2 - 源格式
#   $3 - 目标格式
#   $4 - 预期输出文件路径
# 返回: 0=成功, 1=失败
run_single_test() {
    local input_file="$1"
    local source_format="$2"
    local target_format="$3"
    local expected_file="$4"
    
    # 实现...
}
```

---

### 2. 跨平台兼容性处理

**Windows兼容性**：
- 检测运行环境（Git Bash vs WSL vs Linux）
- 路径处理使用相对路径
- 换行符统一为LF（使用`.gitattributes`）

**示例**：
```bash
# 检测操作系统
detect_os() {
    case "$(uname -s)" in
        Linux*)     OS=Linux;;
        Darwin*)    OS=Mac;;
        CYGWIN*|MINGW*|MSYS*) OS=Windows;;
        *)          OS=Unknown;;
    esac
    echo "$OS"
}
```

---

### 3. 测试隔离和清理

**原则**：
- 每个测试使用独立的临时目录
- 测试完成后自动清理
- 支持`--keep-output`参数保留输出（用于调试）

**示例**：
```bash
# 清理函数
cleanup() {
    if [[ "${KEEP_OUTPUT:-0}" -eq 0 ]]; then
        rm -rf "$TEST_OUTPUT_DIR"
    fi
}
trap cleanup EXIT
```

---

## 实施建议

### MVP（最小可行产品）优先级

**阶段1（P1 - 2周）**：
1. ✅ 创建基础测试运行器脚本
2. ✅ 实现YAML配置解析
3. ✅ 实现diff比较和报告生成
4. ✅ 添加2-3个基础测试用例

**阶段2（P2 - 1周）**：
5. ✅ 扩展到5种格式，每种4个测试用例
6. ✅ 添加彩色输出和详细报告
7. ✅ 优化性能和错误处理

**阶段3（P3 - 1周）**：
8. ✅ 集成到GitHub Actions
9. ✅ 添加测试趋势分析
10. ✅ 完善文档

---

## 风险评估

| 风险 | 可能性 | 影响 | 缓解措施 |
|------|--------|------|----------|
| Windows环境兼容性问题 | 中 | 中 | 在GitHub Actions中测试所有平台，提供WSL使用说明 |
| YAML解析复杂度 | 低 | 低 | 使用简单的grep/awk解析，避免复杂嵌套 |
| 测试数据维护成本 | 中 | 中 | 提供清晰的README和添加指南，自动化测试数据生成 |
| 性能问题（大量测试） | 低 | 低 | 支持并行执行（后续优化） |

---

## 替代方案记录

### 不选择的方案及原因

1. **BATS (Bash Automated Testing System)**
   - 优点：专业的Bash测试框架
   - 缺点：引入额外依赖，增加复杂度
   - 决策：自定义脚本更轻量，满足需求

2. **Make + Makefile**
   - 优点：广泛使用，支持依赖管理
   - 缺点：语法复杂，不适合复杂的测试逻辑
   - 决策：Shell脚本更灵活

3. **Docker容器化测试**
   - 优点：完全隔离的测试环境
   - 缺点：增加CI运行时间，增加复杂度
   - 决策：当前不需要，可作为后续优化

---

## 参考资料

- [Google Shell Style Guide](https://google.github.io/styleguide/shellguide.html)
- [GitHub Actions 文档](https://docs.github.com/en/actions)
- [JUnit XML 格式规范](https://llg.cubic.org/docs/junit/)
- [diff 命令手册](https://man7.org/linux/man-pages/man1/diff.1.html)
- [YAML 规范](https://yaml.org/spec/1.2.2/)

---

## 结论

**最终技术栈**：
- **测试脚本语言**: Bash/Zsh (Shell脚本)
- **配置格式**: YAML
- **文件比较**: diff命令
- **报告格式**: 彩色终端 + JUnit XML
- **CI平台**: GitHub Actions
- **测试数据**: 文件系统 + Git版本控制

**优势**：
✅ 零额外依赖（利用系统自带工具）  
✅ 简单易维护（Shell脚本直观）  
✅ 快速执行（无编译步骤）  
✅ 易于扩展（添加新测试用例只需配置）  
✅ 完美适配CI环境  

**下一步**：进入阶段1设计，创建数据模型和API合同（测试用例配置格式）。
