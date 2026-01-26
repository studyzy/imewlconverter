# 数据模型: 词库转换集成测试框架

**日期**: 2026-01-25
**状态**: 完成
**目的**: 定义测试框架中的数据结构和文件格式

## 概述

集成测试框架使用文件系统作为主要存储，数据模型包括：
1. **测试用例配置**（YAML格式）
2. **测试数据文件**（词库文件）
3. **测试报告**（终端文本 + JUnit XML）

---

## 1. 测试用例配置 (TestCase)

### 结构定义

```yaml
# test-config.yaml
# 测试套件配置文件

suite_name: "搜狗拼音格式测试"
description: "验证搜狗拼音格式到其他格式的转换"
maintainer: "studyzy"
created_at: "2026-01-25"

# 测试用例列表
test_cases:
  # 基本转换测试
  - name: "搜狗拼音到QQ拼音 - 基本词条"
    description: "测试基本词条的正确转换"
    enabled: true
    timeout: 10  # 秒
    input:
      file: "basic.txt"
      format: "SougouPinyin"
    output:
      format: "QQPinyin"
      expected: "basic-to-qq.expected"
    tags:
      - basic
      - pinyin
    
  # 边界情况测试
  - name: "搜狗拼音到QQ拼音 - 特殊字符"
    description: "测试特殊字符、生僻字、长词条"
    enabled: true
    timeout: 15
    input:
      file: "boundary.txt"
      format: "SougouPinyin"
    output:
      format: "QQPinyin"
      expected: "boundary-to-qq.expected"
    tags:
      - boundary
      - pinyin
    
  # 编码测试
  - name: "搜狗拼音到QQ拼音 - GBK编码"
    description: "测试GBK编码文件的处理"
    enabled: true
    timeout: 10
    input:
      file: "gbk-encoded.txt"
      format: "SougouPinyin"
      encoding: "GBK"  # 可选，默认UTF-8
    output:
      format: "QQPinyin"
      expected: "gbk-to-qq.expected"
      encoding: "UTF-8"
    tags:
      - encoding
      - pinyin
    
  # 大文件测试
  - name: "搜狗拼音到QQ拼音 - 大型词库"
    description: "测试大文件（1000+词条）的转换性能"
    enabled: true
    timeout: 30
    input:
      file: "large.txt"
      format: "SougouPinyin"
    output:
      format: "QQPinyin"
      expected: "large-to-qq.expected"
    tags:
      - performance
      - pinyin
    
  # 错误处理测试
  - name: "搜狗拼音 - 格式错误处理"
    description: "测试格式错误的文件是否能正确报错"
    enabled: true
    timeout: 5
    expect_failure: true  # 标记此测试预期会失败
    input:
      file: "malformed.txt"
      format: "SougouPinyin"
    output:
      format: "QQPinyin"
    tags:
      - error-handling
```

### 字段说明

#### 套件级别 (Suite Level)

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `suite_name` | string | 是 | 测试套件名称 |
| `description` | string | 是 | 套件描述 |
| `maintainer` | string | 否 | 维护者 |
| `created_at` | date | 否 | 创建日期 |
| `test_cases` | array | 是 | 测试用例列表 |

#### 测试用例级别 (Test Case Level)

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `name` | string | 是 | 测试用例名称（唯一） |
| `description` | string | 是 | 测试描述 |
| `enabled` | boolean | 否 | 是否启用（默认true） |
| `timeout` | integer | 否 | 超时时间（秒，默认30） |
| `expect_failure` | boolean | 否 | 是否预期失败（默认false） |
| `input` | object | 是 | 输入配置 |
| `output` | object | 是 | 输出配置 |
| `tags` | array | 否 | 标签列表 |

#### 输入配置 (Input)

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `file` | string | 是 | 输入文件路径（相对于test-cases目录） |
| `format` | string | 是 | 源格式名称 |
| `encoding` | string | 否 | 文件编码（默认UTF-8） |

#### 输出配置 (Output)

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `format` | string | 是 | 目标格式名称 |
| `expected` | string | 条件必填 | 预期输出文件路径（expect_failure=false时必填） |
| `encoding` | string | 否 | 输出编码（默认UTF-8） |

---

## 2. 测试数据文件 (TestData)

### 文件组织

```
test-cases/
└── sougou-pinyin/
    ├── README.md              # 测试说明文档
    ├── test-config.yaml       # 测试配置
    ├── basic.txt              # 基本测试输入
    ├── basic-to-qq.expected   # 预期输出（QQ拼音）
    ├── basic-to-baidu.expected  # 预期输出（百度拼音）
    ├── boundary.txt           # 边界测试输入
    ├── boundary-to-qq.expected
    ├── large.txt              # 大文件测试
    ├── large-to-qq.expected
    ├── gbk-encoded.txt        # GBK编码测试
    ├── gbk-to-qq.expected
    └── malformed.txt          # 格式错误测试
```

### 命名约定

- **输入文件**: `{test-type}.txt`（如 `basic.txt`, `boundary.txt`）
- **预期输出**: `{test-type}-to-{target-format}.expected`（如 `basic-to-qq.expected`）
- **配置文件**: `test-config.yaml`（固定名称）
- **说明文档**: `README.md`（固定名称）

### 文件大小建议

| 文件类型 | 推荐大小 | 词条数量 | 用途 |
|---------|---------|---------|------|
| basic | <10KB | 5-20 | 基本功能测试 |
| boundary | <50KB | 20-100 | 边界情况测试 |
| large | <1MB | 1000-5000 | 性能测试 |
| error | 任意 | 任意 | 错误处理测试 |

---

## 3. 测试报告 (TestReport)

### 3.1 终端输出格式

```
==========================================
词库转换集成测试报告
==========================================
测试套件: 搜狗拼音格式测试
开始时间: 2026-01-25 14:30:00
==========================================

[1/5] ✓ 搜狗拼音到QQ拼音 - 基本词条
      耗时: 2.3秒
      状态: PASS

[2/5] ✗ 搜狗拼音到QQ拼音 - 特殊字符
      耗时: 3.1秒
      状态: FAIL
      错误: 输出文件与预期不符
      差异详情:
        第15行: 预期 "你好 ni hao"
                实际 "您好 nin hao"
        第28行: 预期存在
                实际缺失

[3/5] ✓ 搜狗拼音到QQ拼音 - GBK编码
      耗时: 2.8秒
      状态: PASS

[4/5] ✓ 搜狗拼音到QQ拼音 - 大型词库
      耗时: 15.2秒
      状态: PASS

[5/5] ✓ 搜狗拼音 - 格式错误处理
      耗时: 0.5秒
      状态: PASS (预期失败)

==========================================
测试总结
==========================================
总计: 5 个测试
通过: 4 (80%)
失败: 1 (20%)
跳过: 0
总耗时: 23.9秒
==========================================
```

### 3.2 JUnit XML格式

```xml
<?xml version="1.0" encoding="UTF-8"?>
<testsuites name="词库转换集成测试" tests="5" failures="1" errors="0" time="23.9">
  <testsuite name="搜狗拼音格式测试" tests="5" failures="1" errors="0" time="23.9" timestamp="2026-01-25T14:30:00">
    
    <testcase name="搜狗拼音到QQ拼音 - 基本词条" 
              classname="sougou-pinyin.basic" 
              time="2.3">
    </testcase>
    
    <testcase name="搜狗拼音到QQ拼音 - 特殊字符" 
              classname="sougou-pinyin.boundary" 
              time="3.1">
      <failure type="AssertionError" message="输出文件与预期不符">
第15行: 预期 "你好 ni hao"
        实际 "您好 nin hao"
第28行: 预期存在
        实际缺失
      </failure>
    </testcase>
    
    <testcase name="搜狗拼音到QQ拼音 - GBK编码" 
              classname="sougou-pinyin.encoding" 
              time="2.8">
    </testcase>
    
    <testcase name="搜狗拼音到QQ拼音 - 大型词库" 
              classname="sougou-pinyin.performance" 
              time="15.2">
    </testcase>
    
    <testcase name="搜狗拼音 - 格式错误处理" 
              classname="sougou-pinyin.error-handling" 
              time="0.5">
    </testcase>
    
  </testsuite>
</testsuites>
```

### 报告文件存储

```
reports/
├── latest.txt           # 最新的终端输出报告
├── latest.xml           # 最新的JUnit XML报告
└── history/             # 历史报告（可选）
    ├── 2026-01-25_143000.txt
    ├── 2026-01-25_143000.xml
    ├── 2026-01-24_091500.txt
    └── 2026-01-24_091500.xml
```

---

## 4. 测试执行状态 (TestExecution)

### 内存中的测试状态

虽然不持久化到文件，但脚本运行时维护以下状态：

```bash
# 全局变量
TOTAL_TESTS=0        # 总测试数
PASSED_TESTS=0       # 通过数
FAILED_TESTS=0       # 失败数
SKIPPED_TESTS=0      # 跳过数
START_TIME=""        # 开始时间
END_TIME=""          # 结束时间

# 单个测试状态
TEST_NAME=""         # 当前测试名称
TEST_STATUS=""       # PASS/FAIL/SKIP
TEST_START_TIME=""   # 测试开始时间
TEST_DURATION=""     # 测试耗时（秒）
TEST_ERROR_MSG=""    # 错误消息（如果失败）
TEST_DIFF_OUTPUT=""  # diff输出（如果有差异）
```

---

## 5. 支持的输入法格式代码

### 格式代码映射表

根据`ImeWlConverterCmd`支持的格式，定义标准格式代码：

| 格式代码 | 完整名称 | 说明 |
|---------|---------|------|
| `SougouPinyin` | 搜狗拼音（文本） | 搜狗拼音文本格式词库 |
| `SougouPinyinScel` | 搜狗拼音（Scel） | 搜狗拼音细胞词库格式 |
| `QQPinyin` | QQ拼音 | QQ拼音文本格式 |
| `BaiduPinyin` | 百度拼音 | 百度拼音文本格式 |
| `GooglePinyin` | 谷歌拼音 | 谷歌拼音格式 |
| `SougouWubi` | 搜狗五笔 | 搜狗五笔格式 |
| `QQWubi` | QQ五笔 | QQ五笔格式 |
| `Rime` | Rime输入法 | 中州韵/小狼毫/鼠须管 |
| `Win10MsPinyin` | Win10微软拼音 | Windows 10 微软拼音 |
| `MacOSPinyin` | macOS自带拼音 | macOS系统拼音 |

*完整列表见: `ImeWlConverterCmd --help`*

---

## 6. 数据验证规则

### 配置文件验证

测试运行前验证：
- ✅ `test-config.yaml`存在且格式正确
- ✅ 所有`enabled=true`的测试用例引用的输入文件存在
- ✅ 所有`enabled=true`且`expect_failure=false`的测试用例引用的预期输出文件存在
- ✅ `timeout`值合理（1-300秒）
- ✅ 格式代码有效（在支持的格式列表中）

### 测试数据验证

- ✅ 输入文件非空
- ✅ 输入文件编码与声明一致
- ✅ 预期输出文件非空（对于非错误处理测试）

---

## 7. 扩展性设计

### 添加新测试用例的步骤

1. 在`test-cases/{format}/`目录下创建输入文件
2. 使用真实的转换工具生成预期输出文件
3. 在`test-config.yaml`中添加测试用例配置
4. 运行测试验证

### 添加新格式的步骤

1. 创建`test-cases/{new-format}/`目录
2. 创建`README.md`说明文档
3. 创建`test-config.yaml`配置文件
4. 准备至少4个测试用例（basic、boundary、encoding、error）
5. 在主测试运行器中注册新格式

---

## 8. 数据关系图

```
TestSuite
  ├─ suite_name
  ├─ description
  └─ TestCase[] (1..N)
       ├─ name
       ├─ description
       ├─ Input
       │   ├─ file ──→ TestDataFile
       │   ├─ format
       │   └─ encoding
       ├─ Output
       │   ├─ format
       │   ├─ expected ──→ ExpectedOutputFile
       │   └─ encoding
       └─ TestResult
            ├─ status (PASS/FAIL/SKIP)
            ├─ duration
            ├─ error_message
            └─ diff_output
```

---

## 总结

数据模型设计特点：
- ✅ **简单**: 使用YAML和文本文件，无需数据库
- ✅ **灵活**: 易于添加新测试用例和格式
- ✅ **可追溯**: 测试数据版本化存储在Git中
- ✅ **标准化**: 使用JUnit XML格式，兼容CI工具
- ✅ **可维护**: 清晰的目录结构和命名约定
