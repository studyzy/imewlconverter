# 第1类测试:导入测试

## 测试目标

验证各种输入法词库格式能正确转换为**统一的CSV格式**(词语,拼音,词频)。

## 设计理念

### 为什么需要导入测试?

导入测试是词库转换工具的**核心功能**验证:
- ✅ **解析能力**: 验证能正确读取和解析各种输入法格式
- ✅ **数据准确性**: 确保拼音、词语、词频信息完整准确
- ✅ **编码处理**: 验证各种字符编码(UTF-8、UTF-16LE、GBK)的正确处理
- ✅ **格式兼容**: 覆盖二进制格式(.scel、.qpyd、.qcel)和文本格式

### 统一CSV格式的优势

所有测试都输出为**CSV格式**(词语,拼音 空格分隔,词频):
- 📝 **易于验证**: 文本格式可直接使用diff工具比较
- 🔍 **便于调试**: 出错时可直接查看文本内容
- 📊 **标准化**: 统一的CSV格式便于后续处理和导入其他系统
- ⚡ **高效**: 避免二进制格式验证的复杂性

**CSV格式示例**:
```csv
哀江头,ai jiang tou,1
唉乃一声山水绿,ai nai yi sheng shan shui lv,1
深蓝词库,shen lan ci ku,2
```

格式说明:
- 第1列: 词语(汉字)
- 第2列: 拼音(空格分隔)
- 第3列: 词频

## 测试矩阵

| 测试ID | 输入格式 | 测试文件 | 词条数 | 测试重点 | 状态 |
|--------|---------|---------|--------|---------|------|
| T01 | 搜狗.scel | 唐诗300首.scel | 3,563 | 二进制格式解析 | ✅ |
| T02 | QQ拼音文本 | QQPinyin.txt | 4 | UTF-16LE编码 | ✅ |
| T03 | QQ拼音.qpyd | 成语.qpyd | 1,657 | 分类词库 | ✅ |
| T04 | QQ拼音.qcel | 星际战甲.qcel | 4,675 | 英文字母处理 | ✅ |
| T06 | Rime | luna_pinyin_export.txt | 17 | Rime格式 | ✅ |
| T09 | 纯汉字 | 纯汉字.txt | 59 | 自动拼音生成 | ✅ |
| T05 | 百度.bdict | travel.bdict | ~200 | 百度格式 | ⏳ 超时待修复 |
| T07 | 灵格斯.ld2 | i.ld2 | ~50K | 大文件 | ⏳ 待优化 |
| T08 | 自定义 | array30.txt | ~1K | 行列30码表 | ⏳ 待实施 |
| T12 | GBK编码 | gbzy.txt | ~100 | GBK编码 | ⏳ 待实施 |
| T13 | UTF-8无BOM | u8nobomzy.txt | ~100 | UTF-8 | ⏳ 待实施 |

**当前覆盖**:
- ✅ **已启用**: 6个测试用例
- ✅ **格式类型**: 4种(搜狗、QQ拼音、Rime、纯汉字)
- ✅ **总词条数**: 约10,000词条
- ⏳ **待完善**: 5个测试用例

## 运行测试

### 基本运行

```bash
# 进入测试目录
cd tests/integration

# 运行导入测试套件
./run-tests.sh -s 1-imports

# 带详细输出
./run-tests.sh -s 1-imports -v

# 保留测试输出文件用于调试
./run-tests.sh -s 1-imports --keep-output
```

### 按标签过滤

```bash
# 只运行二进制格式测试
./run-tests.sh -s 1-imports -t binary

# 只运行文本格式测试
./run-tests.sh -s 1-imports -t text

# 运行QQ拼音相关测试
./run-tests.sh -s 1-imports -t qqpinyin

# 运行基础测试
./run-tests.sh -s 1-imports -t basic
```

### 可用标签

- `binary` - 二进制格式测试(.scel、.qpyd、.qcel)
- `text` - 文本格式测试
- `sougou` - 搜狗拼音相关
- `qqpinyin` - QQ拼音相关
- `rime` - Rime输入法
- `encoding` - 编码相关测试
- `basic` - 基础测试
- `english` - 包含英文字母
- `no-pinyin` - 无拼音自动生成

## 测试数据

所有测试数据直接引用 `src/ImeWlConverterCoreTest/Test/` 目录中的真实测试文件:
- ✅ **真实可靠**: 这些文件已在项目中使用多年
- ✅ **无需维护**: 不需要在测试目录中重复存储
- ✅ **自动同步**: 单元测试更新时集成测试自动受益

## 文件结构

```
1-imports/
├── test-config.yaml           # 统一配置文件
├── README.md                  # 本文档
└── expected/                  # 预期输出文件
    ├── t01-scel-to-sgpy.expected
    ├── t02-qqtxt-to-sgpy.expected
    ├── t03-qpyd-to-sgpy.expected
    ├── t04-qcel-to-sgpy.expected
    ├── t06-rime-to-sgpy.expected
    └── t09-word-to-sgpy.expected
```

## 添加新测试

### 1. 生成预期输出

```bash
cd /Users/devinzeng/Code/studyzy/imewlconverter

# 格式: dotnet run --project src/ImeWlConverterCmd -- \
#   -i:输入格式代码 "输入文件路径" \
#   -o:sgpy "预期输出路径"

# 示例: 添加新搜狗词库测试
dotnet run --project src/ImeWlConverterCmd -- \
  -i:scel "src/ImeWlConverterCoreTest/Test/新词库.scel" \
  -o:sgpy tests/integration/test-cases/1-imports/expected/t99-new-to-sgpy.expected
```

### 2. 在test-config.yaml中添加配置

```yaml
- name: "T99-新格式到搜狗文本"
  description: "测试描述"
  enabled: true
  timeout: 15
  expect_failure: false
  input:
    file: "../../../../src/ImeWlConverterCoreTest/Test/新词库.scel"
    format: "scel"
    encoding: "UTF-8"
  output:
    format: "sgpy"
    expected: "expected/t99-new-to-sgpy.expected"
    encoding: "UTF-8"
  tags:
    - "binary"
    - "sougou"
  stats:
    words: 1000
    source: "数据来源说明"
```

### 3. 运行验证

```bash
cd tests/integration
./run-tests.sh -s 1-imports
```

## 常见格式代码

| 格式 | 代码 | 说明 |
|------|------|------|
| 搜狗拼音.scel | `scel` | 搜狗细胞词库 |
| QQ拼音文本 | `qqpy` | QQ拼音文本格式 |
| QQ分类词库.qpyd | `qpyd` | QQ拼音分类词库 |
| QQ分类词库.qcel | `qcel` | QQ拼音分类词库(新) |
| 百度.bdict | `bdict` | 百度拼音词库 |
| Rime | `rime` | Rime输入法 |
| 灵格斯.ld2 | `ld2` | 灵格斯词典 |
| 纯汉字 | `word` | 纯汉字格式 |

完整格式代码列表: `dotnet run --project src/ImeWlConverterCmd -- -h`

## 预期结果格式

预期输出文件格式示例(搜狗拼音文本):
```
拼音	词语	词频
chun'tian	春天	100
xia'tian	夏天	95
qiu'tian	秋天	90
dong'tian	冬天	85
```

- 每行一个词条
- 使用Tab分隔(不是空格)
- 包含拼音、词语、词频三列
- 拼音使用单引号分隔音节

## 故障排查

### 测试失败

1. **查看详细差异**:
   ```bash
   ./run-tests.sh -s 1-imports -v
   ```

2. **保留输出文件**:
   ```bash
   ./run-tests.sh -s 1-imports --keep-output
   # 实际输出在 test-output/1-imports/ 目录
   ```

3. **手动对比**:
   ```bash
   diff -u expected/t01-scel-to-sgpy.expected test-output/1-imports/t01-actual.txt
   ```

### 转换超时

某些大文件(如T05百度.bdict)可能转换超时:
- 调整 `timeout` 配置(默认15秒)
- 临时禁用: 设置 `enabled: false`

### 编码问题

如果输出乱码,检查:
1. 输入文件编码是否正确
2. test-config.yaml中的encoding配置
3. 系统locale设置

## 与其他测试类别的关系

```
1-imports (导入测试)
    ↓
  统一格式 (搜狗拼音文本)
    ↓
2-exports (导出测试) → 各种输出格式
    ↓
3-advanced (高级功能) → 过滤、合并等
```

导入测试是整个测试体系的**基础**:
- ✅ 保证数据能正确读入系统
- ✅ 为导出测试提供可靠的源数据
- ✅ 为高级功能测试提供输入数据

## 相关文档

- [集成测试总览](../../README.md)
- [测试矩阵设计](../../TEST-MATRIX.md)
- [第2类:导出测试](../2-exports/README.md) (待实施)
- [第3类:高级功能测试](../3-advanced/README.md) (待实施)
- [任务规划](../../../specs/001-integration-tests/tasks.md)
