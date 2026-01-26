# 快速开始: 词库转换集成测试

**目标**: 5分钟内完成测试框架的设置并运行第一个测试用例

**前置条件**:
- ✅ 已安装 Bash 4.0+ 或 Zsh 5.0+（macOS/Linux自带，Windows使用Git Bash或WSL）
- ✅ 已安装 .NET 8.0 或 .NET Framework 4.6
- ✅ 已克隆项目仓库到本地

---

## 步骤1: 构建命令行工具 (1分钟)

```bash
# 进入项目根目录
cd /path/to/imewlconverter

# 构建命令行工具
dotnet build src/ImeWlConverterCmd/ImeWlConverterCmd.csproj --configuration Release

# 验证工具可用
dotnet run --project src/ImeWlConverterCmd/ImeWlConverterCmd.csproj -- --help
```

**预期输出**: 显示命令行工具的帮助信息

---

## 步骤2: 准备测试环境 (30秒)

```bash
# 进入测试目录
cd tests/integration

# 确认测试脚本可执行
chmod +x run-tests.sh

# 查看可用的测试选项
./run-tests.sh --help
```

**预期输出**:
```
词库转换集成测试运行器 v1.0
用法: ./run-tests.sh [选项]

选项:
  -h, --help          显示帮助信息
  -s, --suite <name>  运行指定测试套件
  -t, --tag <tag>     按标签过滤测试
  -v, --verbose       显示详细输出
  --keep-output       保留测试输出文件（用于调试）
  --xml               生成JUnit XML报告

示例:
  ./run-tests.sh                    # 运行所有测试
  ./run-tests.sh -s sougou-pinyin   # 只运行搜狗拼音测试
  ./run-tests.sh -t basic           # 只运行basic标签的测试
  ./run-tests.sh -v --xml           # 详细模式 + 生成XML报告
```

---

## 步骤3: 运行第一个测试 (2分钟)

```bash
# 运行基础测试用例
./run-tests.sh -s sougou-pinyin -t basic
```

**预期输出**:
```
==========================================
词库转换集成测试报告
==========================================
测试套件: 搜狗拼音格式测试
开始时间: 2026-01-25 14:30:00
==========================================

[1/1] ✓ 搜狗拼音到QQ拼音 - 基本词条
      耗时: 2.3秒
      状态: PASS

==========================================
测试总结
==========================================
总计: 1 个测试
通过: 1 (100%)
失败: 0 (0%)
跳过: 0
总耗时: 2.3秒
==========================================
```

**恭喜！** 您已经成功运行了第一个集成测试！ 🎉

---

## 步骤4: 查看测试数据 (1分钟)

```bash
# 查看测试用例配置
cat test-cases/sougou-pinyin/test-config.yaml

# 查看输入数据示例
head -n 5 test-cases/sougou-pinyin/basic.txt

# 查看预期输出示例
head -n 5 test-cases/sougou-pinyin/basic-to-qq.expected
```

**理解测试数据结构**:
```
test-cases/
└── sougou-pinyin/
    ├── test-config.yaml         # 测试配置
    ├── basic.txt                # 输入数据
    └── basic-to-qq.expected     # 预期输出
```

---

## 步骤5: 运行完整测试套件 (1分钟)

```bash
# 运行所有搜狗拼音测试
./run-tests.sh -s sougou-pinyin
```

**预期输出**: 显示所有测试用例的执行结果（4-5个测试）

---

## 常见操作

### 运行所有测试

```bash
./run-tests.sh
```

### 只运行特定标签的测试

```bash
# 只运行基本测试
./run-tests.sh -t basic

# 只运行性能测试
./run-tests.sh -t performance

# 运行多个标签（组合）
./run-tests.sh -t basic -t boundary
```

### 生成JUnit XML报告（用于CI）

```bash
./run-tests.sh --xml

# 查看生成的报告
cat reports/latest.xml
```

### 调试失败的测试

```bash
# 运行测试并保留输出文件
./run-tests.sh --keep-output --verbose

# 查看实际输出
ls test-output/

# 手动比较差异
diff test-cases/sougou-pinyin/basic-to-qq.expected \
     test-output/sougou-pinyin/basic.actual
```

---

## 添加新测试用例（10分钟）

### 方法1: 最简单的方式（复制现有测试）

```bash
cd test-cases/sougou-pinyin

# 复制现有测试数据
cp basic.txt my-new-test.txt
cp basic-to-qq.expected my-new-test-to-qq.expected

# 编辑新的测试数据
vim my-new-test.txt
vim my-new-test-to-qq.expected

# 在 test-config.yaml 中添加新测试用例
vim test-config.yaml
```

在 `test-config.yaml` 中添加：

```yaml
test_cases:
  # ...现有测试用例...
  
  - name: "搜狗拼音到QQ拼音 - 我的新测试"
    description: "测试我添加的特定场景"
    enabled: true
    timeout: 10
    input:
      file: "my-new-test.txt"
      format: "SougouPinyin"
    output:
      format: "QQPinyin"
      expected: "my-new-test-to-qq.expected"
    tags:
      - custom
      - pinyin
```

### 方法2: 生成预期输出（推荐）

```bash
# 准备输入文件
echo "你好 ni hao 100" > test-cases/sougou-pinyin/my-test.txt
echo "世界 shi jie 50" >> test-cases/sougou-pinyin/my-test.txt

# 使用真实的转换工具生成预期输出
dotnet run --project ../../src/ImeWlConverterCmd/ImeWlConverterCmd.csproj -- \
  -i test-cases/sougou-pinyin/my-test.txt \
  --from SougouPinyin \
  --to QQPinyin \
  -o test-cases/sougou-pinyin/my-test-to-qq.expected

# 检查生成的文件
cat test-cases/sougou-pinyin/my-test-to-qq.expected

# 添加到配置文件（同方法1）
```

### 运行新测试

```bash
./run-tests.sh -s sougou-pinyin -t custom
```

---

## 添加新的输入法格式测试（15分钟）

### 第1步: 创建新测试套件目录

```bash
cd test-cases

# 创建新格式目录（以QQ拼音为例）
mkdir -p qq-pinyin

cd qq-pinyin
```

### 第2步: 创建测试配置

```bash
# 创建配置文件
cat > test-config.yaml << 'EOF'
suite_name: "QQ拼音格式测试"
description: "验证QQ拼音格式到其他格式的转换"
maintainer: "your-name"
created_at: "2026-01-25"

test_cases:
  - name: "QQ拼音到搜狗拼音 - 基本词条"
    description: "测试基本词条的转换"
    enabled: true
    timeout: 10
    input:
      file: "basic.txt"
      format: "QQPinyin"
    output:
      format: "SougouPinyin"
      expected: "basic-to-sougou.expected"
    tags:
      - basic
      - pinyin
EOF
```

### 第3步: 准备测试数据

```bash
# 创建输入文件（QQ拼音格式）
cat > basic.txt << 'EOF'
你好 ni'hao 100
世界 shi'jie 80
中国 zhong'guo 120
输入法 shu'ru'fa 90
EOF

# 生成预期输出
dotnet run --project ../../../src/ImeWlConverterCmd/ImeWlConverterCmd.csproj -- \
  -i basic.txt \
  --from QQPinyin \
  --to SougouPinyin \
  -o basic-to-sougou.expected
```

### 第4步: 创建README

```bash
cat > README.md << 'EOF'
# QQ拼音格式测试

## 测试覆盖

- ✅ 基本词条转换
- ⏳ 边界情况测试（待添加）
- ⏳ 编码测试（待添加）
- ⏳ 性能测试（待添加）

## 测试数据说明

### basic.txt
包含4个常用词条，测试基本转换功能

## 如何运行

```bash
cd ../../
./run-tests.sh -s qq-pinyin
```
EOF
```

### 第5步: 运行新测试套件

```bash
cd ../..
./run-tests.sh -s qq-pinyin
```

---

## 在CI中运行测试

### GitHub Actions 配置示例

在项目根目录创建 `.github/workflows/integration-tests.yml`:

```yaml
name: 集成测试

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, macos-latest, windows-latest]
    
    steps:
      - uses: actions/checkout@v4
      
      - name: 设置 .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: 构建CLI工具
        run: dotnet build src/ImeWlConverterCmd/ImeWlConverterCmd.csproj --configuration Release
      
      - name: 运行集成测试
        run: |
          cd tests/integration
          bash run-tests.sh --xml
      
      - name: 上传测试报告
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-reports-${{ matrix.os }}
          path: tests/integration/reports/
      
      - name: 发布测试结果
        if: always()
        uses: EnricoMi/publish-unit-test-result-action@v2
        with:
          files: tests/integration/reports/latest.xml
```

---

## 故障排查

### 问题: 测试脚本无法执行

```bash
# 解决方法: 添加执行权限
chmod +x run-tests.sh
```

### 问题: Windows下找不到bash

```bash
# 解决方法1: 使用Git Bash
"C:\Program Files\Git\bin\bash.exe" run-tests.sh

# 解决方法2: 使用WSL
wsl bash run-tests.sh
```

### 问题: 转换工具找不到

```bash
# 检查工具是否构建成功
ls src/ImeWlConverterCmd/bin/Release/

# 重新构建
dotnet build src/ImeWlConverterCmd/ImeWlConverterCmd.csproj --configuration Release
```

### 问题: 测试失败但无详细错误

```bash
# 使用详细模式运行
./run-tests.sh -v --keep-output

# 查看实际输出
cat test-output/{test-name}.actual

# 手动比较差异
diff -u {expected-file} test-output/{test-name}.actual
```

---

## 下一步

- 📖 阅读 [data-model.md](./data-model.md) 了解数据结构
- 📄 查看 [test-case-schema.yaml](./contracts/test-case-schema.yaml) 了解配置格式
- 🔬 阅读 [research.md](./research.md) 了解技术选型
- 📋 查看现有测试用例获取更多示例

---

## 获取帮助

- 💬 在项目仓库提交Issue
- 📚 查看项目Wiki文档
- 🔗 参考 [tests/integration/README.md](../../tests/integration/README.md)

---

**祝测试愉快！** 🚀
