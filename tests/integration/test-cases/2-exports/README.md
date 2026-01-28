# 第2类测试:导出测试

## 测试目标

验证统一的词库数据(搜狗拼音文本格式)能正确导出为**各种输入法格式**。

## 设计理念

### 为什么需要导出测试?

导出测试是词库转换工具的**核心输出能力**验证:
- ✅ **生成能力**: 验证能正确生成各种输入法格式
- ✅ **数据完整性**: 确保拼音、词语、词频信息在导出过程中不丢失
- ✅ **格式兼容性**: 生成的文件能被对应的输入法正确识别
- ✅ **往返一致性**: 对于二进制格式,通过往返测试验证数据保真度

### 统一源数据的优势

所有测试使用**同一个源数据文件**:
- 📝 **公平对比**: 所有格式使用相同的输入数据
- 🔍 **易于分析**: 问题定位时只需关注格式转换逻辑
- 📊 **标准基准**: 200个词条覆盖常见拼音模式
- ⚡ **高效测试**: 避免准备多份测试数据

### 往返测试设计

对于二进制格式(scel、qpyd、qcel),采用**往返测试**:
```
源数据(sgpy) → 导出二进制格式 → 导入回sgpy → 比较是否一致
```

优势:
- ✅ 验证导出和导入的双向能力
- ✅ 确保数据在转换过程中的完整性
- ✅ 无需手动检查二进制文件内容

## 测试矩阵

### 文本格式导出

| 测试ID | 目标格式 | 编码 | 词条数 | 状态 |
|--------|---------|------|--------|------|
| E01 | 搜狗拼音文本 | UTF-8 | 200 | ✅ 基准测试 |
| E02 | QQ拼音文本 | UTF-16LE | 200 | ✅ |
| E03 | Rime | UTF-8 | 200 | ✅ |
| E04 | 微软拼音 | UTF-8 | 200 | ✅ |
| E05 | 小狼毫 | UTF-8 | 200 | ⏳ 待实施 |

### 二进制格式导出(往返测试)

| 测试ID | 目标格式 | 往返测试 | 词条数 | 状态 |
|--------|---------|---------|--------|------|
| E10 | 搜狗.scel | 是 | 200 | ✅ |
| E11 | QQ拼音.qpyd | 是 | 200 | ✅ |
| E12 | QQ拼音.qcel | 是 | 200 | ✅ |
| E13 | 百度.bdict | 是 | 200 | ⏳ 待修复 |

**当前覆盖**:
- ✅ **已启用**: 7个测试用例
- ✅ **格式类型**: 4种文本格式 + 3种二进制格式
- ✅ **词条数**: 200个(来自唐诗300首)
- ⏳ **待完善**: 2个测试用例

## 运行测试

### 基本运行

```bash
# 进入测试目录
cd tests/integration

# 运行导出测试套件
./run-tests.sh -s 2-exports

# 带详细输出
./run-tests.sh -s 2-exports -v

# 保留测试输出文件用于调试
./run-tests.sh -s 2-exports --keep-output
```

### 按标签过滤

```bash
# 只运行文本格式导出测试
./run-tests.sh -s 2-exports -t text

# 只运行二进制格式导出测试
./run-tests.sh -s 2-exports -t binary

# 只运行往返测试
./run-tests.sh -s 2-exports -t roundtrip

# 运行QQ拼音相关导出测试
./run-tests.sh -s 2-exports -t qqpinyin
```

### 可用标签

- `text` - 文本格式导出测试
- `binary` - 二进制格式导出测试  
- `roundtrip` - 往返测试
- `baseline` - 基准测试(sgpy→sgpy)
- `sougou` - 搜狗拼音相关
- `qqpinyin` - QQ拼音相关
- `rime` - Rime输入法
- `microsoft` - 微软拼音
- `encoding` - 编码相关测试

## 测试数据

**源数据文件** (`source/source-data.txt`):
- 来源: 唐诗300首前200个词条
- 格式: 搜狗拼音文本格式
- 编码: UTF-8
- 内容: 包含拼音、词语、词频
- 特点: 覆盖常见汉字和拼音组合

示例数据:
```
'ai'jiang'tou	哀江头
'ai'nai'yi'sheng'shan'shui'lv	唉乃一声山水绿
'ai'ruo'yu'zhi'piao'ling	哀弱羽之飘零
```

## 文件结构

```
2-exports/
├── test-config.yaml           # 统一配置文件
├── README.md                  # 本文档
├── source/                    # 源数据
│   └── source-data.txt       # 统一的测试数据(200词条)
└── expected/                  # 预期输出文件
    ├── e01-sgpy-to-sgpy.expected
    ├── e02-sgpy-to-qqpy.expected
    ├── e03-sgpy-to-rime.expected
    ├── e04-sgpy-to-mspy.expected
    ├── e10-sgpy-to-scel-roundtrip.expected
    ├── e11-sgpy-to-qpyd-roundtrip.expected
    └── e12-sgpy-to-qcel-roundtrip.expected
```

## 往返测试执行流程

对于二进制格式的往返测试(E10-E13),测试运行器会:

1. **导出阶段**: 源数据(sgpy) → 导出为二进制格式
   ```bash
   dotnet run ... -i:sgpy source-data.txt -o:scel intermediate.scel
   ```

2. **导入阶段**: 二进制格式 → 导入回sgpy格式
   ```bash
   dotnet run ... -i:scel intermediate.scel -o:sgpy actual-output.txt
   ```

3. **验证阶段**: 比较实际输出与源数据是否一致
   ```bash
   diff expected.txt actual-output.txt
   ```

## 添加新测试

### 1. 文本格式导出测试

```bash
cd /Users/devinzeng/Code/studyzy/imewlconverter

# 生成预期输出
dotnet run --project src/ImeWlConverterCmd -- \
  -i:sgpy tests/integration/test-cases/2-exports/source/source-data.txt \
  -o:新格式代码 tests/integration/test-cases/2-exports/expected/eNN-sgpy-to-新格式.expected
```

### 2. 在test-config.yaml中添加配置

```yaml
- name: "ENN-搜狗文本到新格式"
  description: "测试导出为新格式"
  enabled: true
  timeout: 15
  expect_failure: false
  input:
    file: "source/source-data.txt"
    format: "sgpy"
    encoding: "UTF-8"
  output:
    format: "新格式代码"
    expected: "expected/eNN-sgpy-to-新格式.expected"
    encoding: "UTF-8"
  tags:
    - "text"
    - "新格式"
  stats:
    words: 200
    source: "唐诗300首前200词条"
```

### 3. 二进制格式往返测试

```yaml
- name: "ENN-搜狗文本到新二进制格式(往返)"
  description: "测试导出为新二进制格式,往返验证"
  enabled: true
  timeout: 20
  expect_failure: false
  roundtrip: true  # 重要:标记为往返测试
  input:
    file: "source/source-data.txt"
    format: "sgpy"
    encoding: "UTF-8"
  output:
    format: "新格式代码"
    intermediate: "test-output/2-exports/eNN-intermediate.新扩展名"
    expected: "expected/eNN-sgpy-to-新格式-roundtrip.expected"
    encoding: "UTF-8"
  tags:
    - "binary"
    - "roundtrip"
    - "新格式"
  stats:
    words: 200
    source: "唐诗300首前200词条"
  note: "往返测试:源数据→新格式→sgpy,验证最终输出与源数据一致"
```

预期输出文件应该是源数据的副本:
```bash
cp source/source-data.txt expected/eNN-sgpy-to-新格式-roundtrip.expected
```

### 4. 运行验证

```bash
cd tests/integration
./run-tests.sh -s 2-exports
```

## 常见格式代码

| 格式 | 代码 | 说明 |
|------|------|------|
| 搜狗拼音文本 | `sgpy` | 搜狗拼音文本格式 |
| QQ拼音文本 | `qqpy` | QQ拼音文本格式(UTF-16LE) |
| Rime | `rime` | Rime输入法格式 |
| 微软拼音 | `mspy` | 微软拼音格式 |
| 搜狗.scel | `scel` | 搜狗细胞词库 |
| QQ.qpyd | `qpyd` | QQ拼音分类词库 |
| QQ.qcel | `qcel` | QQ拼音分类词库(新) |
| 百度.bdict | `bdict` | 百度拼音词库 |

完整格式代码列表: `dotnet run --project src/ImeWlConverterCmd -- -h`

## 故障排查

### 文本格式测试失败

1. **检查编码**: 确认输出文件编码是否正确
   ```bash
   file expected/e02-sgpy-to-qqpy.expected
   ```

2. **查看详细差异**:
   ```bash
   ./run-tests.sh -s 2-exports -v
   ```

3. **手动对比**:
   ```bash
   diff -u expected/e02-sgpy-to-qqpy.expected test-output/2-exports/e02-actual.txt
   ```

### 往返测试失败

往返测试失败通常表明:
- 导出过程丢失了某些信息(拼音、词频等)
- 二进制格式编码/解码不对称
- 字符编码问题

调试步骤:
1. 保留测试输出: `./run-tests.sh -s 2-exports --keep-output`
2. 检查中间二进制文件: `test-output/2-exports/e10-intermediate.scel`
3. 手动执行往返转换,逐步验证

### 转换超时

某些大文件可能转换超时:
- 调整 `timeout` 配置(默认15-20秒)
- 临时禁用: 设置 `enabled: false`

## 与其他测试类别的关系

```
1-imports (导入测试) → 各种格式 → 统一格式(sgpy)
                                       ↓
                              2-exports (导出测试)
                                       ↓
                            统一格式 → 各种格式
                                       ↓
                           3-advanced (高级功能)
                                过滤、合并等
```

导出测试是整个测试体系的**中间环节**:
- ✅ 验证数据能正确输出为各种格式
- ✅ 确保导出的文件格式规范
- ✅ 通过往返测试验证导入导出的对称性

## 相关文档

- [集成测试总览](../../README.md)
- [测试矩阵设计](../../TEST-MATRIX.md)
- [第1类:导入测试](../1-imports/README.md)
- [第3类:高级功能测试](../3-advanced/README.md) (待实施)
- [任务规划](../../../specs/001-integration-tests/tasks.md)
