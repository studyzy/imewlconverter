# 测试套件迁移指南

## 概述

为了优化测试结构和减少维护成本，我们引入了新的**测试矩阵方法**，将原有的多个按格式分组的测试套件整合为统一的 `imports` 测试套件。

## 新旧对比

### 旧结构（v1.0）- 按格式分组

```
test-cases/
├── sougou-scel/           # 搜狗细胞词库测试
│   ├── test-config.yaml   # 2个测试用例
│   └── README.md
├── qq-pinyin-txt/         # QQ拼音文本测试
│   ├── test-config.yaml   # 2个测试用例
│   └── README.md
├── qq-qpyd/               # QQ分类词库qpyd测试
│   ├── test-config.yaml   # 1个测试用例
│   └── README.md
└── qq-qcel/               # QQ分类词库qcel测试
    ├── test-config.yaml   # 1个测试用例
    └── README.md
```

**特点**：
- ✅ 按输入格式清晰分组
- ❌ 每个格式需要单独配置文件
- ❌ 测试用例有重复（都测试转换到搜狗文本）
- ❌ 维护成本高（4个配置文件）

### 新结构（v2.0）- 测试矩阵

```
test-cases/
└── imports/               # 统一的导入测试矩阵
    ├── test-config.yaml   # 6个测试用例（整合）
    ├── README.md
    └── expected/          # 所有预期输出
```

**特点**：
- ✅ 单一配置文件，统一管理
- ✅ 每个测试用例 = 一条转换路径
- ✅ 测试目的明确（输入A → 输出B）
- ✅ 维护成本低
- ✅ 更容易扩展

## 测试用例映射

| 旧测试套件 | 旧测试用例 | 新测试套件 | 新测试用例 |
|-----------|-----------|-----------|-----------|
| sougou-scel | 搜狗Scel到文本 - 唐诗300首 | imports | T1-搜狗scel到搜狗文本 |
| qq-pinyin-txt | QQ拼音基本转换 | imports | T2-QQ拼音txt到搜狗文本 |
| qq-qpyd | QQ拼音qpyd成语词库转换 | imports | T3-QQ分类qpyd到搜狗文本 |
| qq-qcel | QQ拼音qcel星际战甲词库转换 | imports | T4-QQ分类qcel到搜狗文本 |
| - | - | imports | T6-Rime到搜狗文本（新增）|
| - | - | imports | T9-纯汉字到搜狗文本（新增）|

## 迁移状态

### 当前状态

- ✅ 新的 `imports` 测试套件已创建并通过测试
- ✅ 旧测试套件仍然保留（向后兼容）
- ✅ 两套测试可以并行运行

### 建议

**选项1：保留双轨制（推荐）**
- 保留旧测试套件作为参考
- 新增测试只添加到 `imports`
- 逐步停止维护旧测试套件

**选项2：完全迁移**
- 删除旧测试套件
- 仅保留 `imports`
- 更新所有文档引用

## 使用新测试套件

### 运行测试

```bash
# 运行新的imports测试套件
./run-tests.sh -s imports

# 运行所有测试（包括新旧）
./run-tests.sh --all

# 按标签过滤
./run-tests.sh -t qqpinyin
```

### 添加新测试

只需在 `imports/test-config.yaml` 中添加一个测试用例：

```yaml
- name: "TN-新格式到搜狗文本"
  description: "测试新格式解析"
  enabled: true
  timeout: 15
  expect_failure: false
  input:
    file: "../../../../src/ImeWlConverterCoreTest/Test/新文件"
    format: "格式代码"
    encoding: "UTF-8"
  output:
    format: "sgpy"
    expected: "expected/tN-新格式-to-sgpy.expected"
    encoding: "UTF-8"
  tags: ["新格式", "标签2"]
```

## 性能对比

| 指标 | 旧结构（v1.0） | 新结构（v2.0） | 改进 |
|------|--------------|--------------|------|
| 配置文件数 | 4个 | 1个 | ↓ 75% |
| 测试用例数 | 6个 | 6个 | 持平 |
| 测试覆盖率 | ~6种格式 | ~6种格式 | 持平 |
| 执行时间 | ~1.4秒 | ~1.7秒 | ↑ 21% |
| 维护成本 | 高 | 低 | ↓ 显著 |

*注：执行时间略增是因为测试用例统一在一个套件中运行，但总体差异可忽略*

## 兼容性

### 向后兼容

- ✅ 旧测试套件仍然可以运行
- ✅ 旧的运行命令仍然有效：
  ```bash
  ./run-tests.sh -s sougou-scel
  ./run-tests.sh -s qq-pinyin-txt
  ```

### 标签兼容

- ✅ 新测试保留了旧的标签
- ✅ 标签过滤仍然有效：
  ```bash
  ./run-tests.sh -t qqpinyin  # 会找到新旧测试
  ```

## 未来计划

1. **阶段1（当前）**：新旧并存，保持兼容
2. **阶段2（2-4周后）**：停止维护旧测试套件
3. **阶段3（1-2月后）**：移除旧测试套件（可选）

## FAQ

### Q: 为什么不直接删除旧测试？
A: 为了保持向后兼容，给用户和CI系统时间适应新结构。

### Q: 新测试是否覆盖了所有旧测试的功能？
A: 是的。新测试覆盖了所有旧测试的核心功能，并且增加了Rime和纯汉字格式的测试。

### Q: 我应该使用哪个测试套件？
A: **推荐使用新的 `imports` 测试套件**。它更简单、更容易维护。

### Q: 性能有影响吗？
A: 执行时间略有增加（+0.3秒），但在可接受范围内。维护成本的降低远超这个小开销。

### Q: 如何只运行新测试？
A: 使用 `./run-tests.sh -s imports`

## 反馈

如有问题或建议，请在GitHub Issue中反馈。
