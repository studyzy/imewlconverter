# 任务: 词库转换集成测试框架

**输入**: 来自 `/specs/001-integration-tests/` 的设计文档
**前置条件**: plan.md、spec.md、research.md、data-model.md、contracts/test-case-schema.yaml

**技术栈**: Shell脚本 (Bash/Zsh)、YAML配置、diff工具、JUnit XML

**重要说明**: 
- 本项目使用Shell脚本而非C#，这是基于用户明确要求和黑盒测试特性的架构决策
- 测试框架是必需的（章程原则I），但测试框架本身的单元测试是可选的
- 所有文档和注释使用中文（章程原则II）
- **重大变更**: 基于 `src/ImeWlConverterCoreTest/Test/` 中现有的真实测试词库文件创建集成测试用例，大幅增加测试覆盖范围

## 格式: `[ID] [P?] [Story] 描述`
- **[P]**: 可以并行运行(不同文件, 无依赖关系)
- **[Story]**: 此任务属于哪个用户故事(US1、US2、US3)
- 在描述中包含确切的文件路径

---

## 阶段 1: 设置(共享基础设施)

**目的**: 项目初始化和基本目录结构创建

- [x] T001 根据 plan.md 创建完整的项目目录结构 `tests/integration/`
- [x] T002 [P] 在 `tests/integration/test-cases/` 下创建测试格式目录（基于现有测试文件的格式类型）
- [x] T003 [P] 创建 `.gitignore` 文件在 `tests/integration/` 目录，忽略 `test-output/` 临时文件
- [x] T004 [P] 创建 `tests/integration/reports/.gitkeep` 文件以保留空目录

---

## 阶段 2: 基础(阻塞前置条件)

**目的**: 在任何用户故事可以实施之前必须完成的核心脚本和辅助库

**⚠️ 关键**: 在此阶段完成之前, 无法开始任何用户故事工作

- [x] T005 在 `tests/integration/lib/test-helpers.sh` 中创建基础Shell函数库框架（包含中文注释说明用途）
- [x] T006 [P] 在 `tests/integration/lib/test-helpers.sh` 中实现 `run_converter()` 函数：调用 ImeWlConverterCmd 执行转换
- [x] T007 [P] 在 `tests/integration/lib/test-helpers.sh` 中实现 `compare_files()` 函数：使用 diff 比较预期和实际输出
- [x] T008 [P] 在 `tests/integration/lib/color-output.sh` 中实现彩色终端输出函数（支持 ✓/✗ 符号、绿色/红色输出）
- [x] T009 [P] 在 `tests/integration/lib/report-generator.sh` 中实现终端报告生成函数（测试摘要、通过率统计）
- [x] T010 在 `tests/integration/lib/yaml-parser.sh` 中实现简单的YAML解析函数（读取test-config.yaml配置）
- [x] T011 在 `tests/integration/run-tests.sh` 中创建主测试运行器脚本框架，实现命令行参数解析（-h、-s、-t、-v、--xml、--all）
- [x] T012 在 `tests/integration/run-tests.sh` 中实现测试执行流程：加载配置 → 执行测试 → 生成报告

**检查点**: 基础就绪 - 测试运行器框架和所有辅助函数已实现，现在可以开始实施用户故事

---

## 阶段 3: 用户故事 1 - 基础集成测试框架(优先级: P1)🎯 MVP

**目标**: 建立Shell脚本驱动的测试运行器，基于 `src/ImeWlConverterCoreTest/Test/` 中现有的真实测试文件创建集成测试用例，验证搜狗拼音(.scel)格式的转换功能

**独立测试**: 运行 `./run-tests.sh -s sougou-scel`，验证能成功转换搜狗拼音.scel文件并检查输出结果是否与预期一致

### 用户故事 1 的实施

**注意：实施时确保**：
- 遵循Shell脚本最佳实践（Google Shell Style Guide）
- 添加中文注释说明每个函数和关键逻辑（章程原则II）
- 考虑跨平台兼容性，使用POSIX标准命令（章程原则IV）
- 文件路径使用相对路径，避免硬编码
- **使用 `src/ImeWlConverterCoreTest/Test/` 中的真实测试文件作为测试数据源**

#### 测试数据准备 - 搜狗拼音 .scel 格式

- [x] T013 [P] [US1] 复制 `src/ImeWlConverterCoreTest/Test/唐诗300首【官方推荐】.scel` 到 `tests/integration/test-cases/sougou-scel/tangshi300.scel`
- [x] T014 [P] [US1] 使用现有CLI工具转换 tangshi300.scel 为文本格式，生成预期输出文件 `tangshi300-to-text.expected`
- [x] T015 [P] [US1] 复制 `src/ImeWlConverterCoreTest/Test/诗词名句大全.scel` 到 `tests/integration/test-cases/sougou-scel/shici.scel`（大文件性能测试）
- [x] T016 [P] [US1] 在 `tests/integration/test-cases/sougou-scel/` 创建 `README.md`：中文说明测试数据来源、用途和维护方法

#### 测试配置

- [x] T017 [US1] 在 `tests/integration/test-cases/sougou-scel/test-config.yaml` 创建测试套件配置：包含2个测试用例（基本转换、大文件性能测试）
- [x] T018 [US1] 验证 test-config.yaml 格式符合 `contracts/test-case-schema.yaml` 规范

#### 核心功能实现

- [x] T019 [US1] 在 `run-tests.sh` 中实现单个测试用例执行逻辑：读取配置 → 调用转换工具 → 比较结果
- [x] T020 [US1] 在 `run-tests.sh` 中实现批量测试执行：遍历test_cases数组，逐个执行测试
- [x] T021 [US1] 在 `lib/test-helpers.sh` 中实现错误处理逻辑：捕获转换失败、超时、文件不存在等错误
- [x] T022 [US1] 在 `lib/report-generator.sh` 中实现测试失败时的详细差异报告（使用 diff -u 显示期望值 vs 实际值）

#### 测试运行器完善

- [x] T023 [US1] 实现 `-s/--suite` 参数：按套件名称过滤测试（如 -s sougou-scel）
- [x] T024 [US1] 实现 `-t/--tag` 参数：按标签过滤测试（如 -t basic）
- [x] T025 [US1] 实现 `-v/--verbose` 参数：显示详细的执行日志和转换命令
- [x] T026 [US1] 实现 `--keep-output` 参数：保留测试输出文件用于调试
- [x] T027 [US1] 添加使用帮助信息：在 `-h/--help` 参数中显示中文帮助文档

#### 文档与验证

- [x] T028 [P] [US1] 创建 `tests/integration/README.md`：中文集成测试使用指南，包含快速开始、运行方法、添加测试用例
- [x] T029 [P] [US1] 在项目根目录 `README.md` 中添加"集成测试"章节：说明如何运行测试和测试覆盖范围
- [x] T030 [US1] 运行完整的用户故事1测试场景：`./run-tests.sh -s sougou-scel`，验证测试用例全部通过
- [x] T031 [US1] 验证错误处理：测试文件不存在、格式错误等场景，确认能正确检测并报告错误

**检查点**: 此时, 用户故事 1 应该完全功能化且可独立测试 - 能够运行搜狗拼音.scel格式的转换测试并生成报告

---

## 阶段 4: 用户故事 2 - 多格式转换覆盖(优先级: P2)

**目标**: 基于 `src/ImeWlConverterCoreTest/Test/` 中的所有真实测试文件，创建三类测试覆盖：
1. **导入测试** - 各种输入法格式 → 统一的带拼音、词语、词频格式（搜狗文本）
2. **导出测试** - 统一格式（搜狗文本） → 各种输入法格式
3. **高级功能测试** - 过滤、合并、编码处理等高级功能

**独立测试**: 运行完整测试套件 `./run-tests.sh --all`，验证三类测试都能通过

### 用户故事 2 的实施 - 重新设计为三类测试

#### 第1类：导入测试（各种格式 → 统一格式）

**目标**: 测试各种输入法词库格式能正确转换为统一的带拼音、词语、词频格式（搜狗拼音文本）

- [x] T032 [P] [US2] 创建 `tests/integration/test-cases/1-imports/` 目录结构
- [x] T033 [P] [US2] 创建 `tests/integration/test-cases/1-imports/test-config.yaml` 统一配置文件
- [x] T034 [P] [US2] 在配置中添加搜狗.scel格式导入测试（唐诗300首.scel → 搜狗文本）
- [x] T035 [P] [US2] 在配置中添加QQ拼音文本格式导入测试（QQPinyin.txt → 搜狗文本）
- [x] T036 [P] [US2] 在配置中添加QQ拼音.qpyd格式导入测试（成语.qpyd → 搜狗文本）
- [x] T037 [P] [US2] 在配置中添加QQ拼音.qcel格式导入测试（星际战甲.qcel → 搜狗文本）
- [x] T038 [P] [US2] 在配置中添加百度.bdict格式导入测试（travel.bdict → 搜狗文本）
- [x] T039 [P] [US2] 在配置中添加Rime格式导入测试（luna_pinyin_export.txt → 搜狗文本）
- [x] T042 [P] [US2] 在配置中添加纯汉字格式导入测试（纯汉字.txt → 搜狗文本）
- [x] T043 [P] [US2] 为所有导入测试生成预期输出文件到 `expected/` 目录
- [x] T044 [P] [US2] 创建 `tests/integration/test-cases/1-imports/README.md` 说明导入测试设计
- [x] T045 [US2] 运行导入测试套件：`./run-tests.sh -s 1-imports`，验证所有测试通过

#### 第2类：导出测试（统一格式 → 各种格式）

**目标**: 使用统一的源数据（带拼音、词语、词频），测试能正确导出为各种输入法格式

- [x] T046 [P] [US2] 创建 `tests/integration/test-cases/2-exports/` 目录结构
- [x] T047 [P] [US2] 准备统一的源数据文件 `source-data.txt`（搜狗拼音文本格式，约100词条）
- [x] T048 [P] [US2] 创建 `tests/integration/test-cases/2-exports/test-config.yaml` 统一配置文件
- [x] T049 [P] [US2] 在配置中添加导出为搜狗.scel格式测试（源数据 → 搜狗.scel）
- [x] T050 [P] [US2] 在配置中添加导出为QQ拼音.qpyd格式测试（源数据 → QQ.qpyd）
- [x] T051 [P] [US2] 在配置中添加导出为QQ拼音.qcel格式测试（源数据 → QQ.qcel）
- [x] T052 [P] [US2] 在配置中添加导出为百度.bdict格式测试（源数据 → 百度.bdict）
- [x] T053 [P] [US2] 在配置中添加导出为Rime格式测试（源数据 → Rime）
- [x] T054 [P] [US2] 在配置中添加导出为微软拼音格式测试（源数据 → 微软拼音）
- [x] T055 [P] [US2] 实现二进制格式验证逻辑：将导出的二进制文件再次导入，比较是否与源数据一致
- [x] T056 [P] [US2] 为所有导出测试生成预期输出文件到 `expected/` 目录
- [x] T057 [P] [US2] 创建 `tests/integration/test-cases/2-exports/README.md` 说明导出测试设计
- [x] T058 [US2] 运行导出测试套件：`./run-tests.sh -s 2-exports`，验证所有测试通过

#### 第3类：高级功能测试（过滤、合并等）

**目标**: 测试词库的高级处理功能，如过滤、合并、去重、编码转换等

- [x] T059 [P] [US2] 创建 `tests/integration/test-cases/3-advanced/` 目录结构
- [x] T060 [P] [US2] 创建 `tests/integration/test-cases/3-advanced/test-config.yaml` 统一配置文件
- [x] T061 [P] [US2] 添加词频过滤测试：过滤低频词（词频<10）
- [x] T062 [P] [US2] 添加词长过滤测试：过滤短词（长度<2）或长词（长度>10）
- [x] T063 [P] [US2] 添加多文件合并测试：合并3个词库文件，验证合并结果
- [x] T064 [P] [US2] 添加去重测试：合并后去除重复词条
- [x] T065 [P] [US2] 添加编码转换测试：GBK → UTF-8, UTF-16LE → UTF-8
- [x] T066 [P] [US2] 添加UTF-8 BOM处理测试：自动检测和移除BOM
- [x] T067 [P] [US2] 添加大文件性能测试：诗词名句大全.scel（13MB, 342K词条，timeout: 60s）
- [x] T068 [P] [US2] 为所有高级功能测试生成预期输出
- [x] T069 [P] [US2] 创建 `tests/integration/test-cases/3-advanced/README.md` 说明高级功能测试设计
- [x] T070 [US2] 运行高级功能测试套件：`./run-tests.sh -s 3-advanced`，验证所有测试通过

#### 测试套件整合

- [x] T071 [US2] 在 `run-tests.sh` 中实现 `--all` 参数：自动运行三类测试套件（1-imports、2-exports、3-advanced）
- [x] T072 [US2] 实现测试套件汇总报告：按类别显示通过率（导入、导出、高级功能）
- [x] T073 [US2] 运行完整测试套件：`./run-tests.sh --all`，验证三类测试全部通过
- [x] T074 [US2] 优化测试执行顺序：导入测试 → 导出测试 → 高级功能测试
- [x] T075 [US2] 更新 `TEST-MATRIX.md` 文档，说明三类测试的设计理念和覆盖范围
- [x] T076 [US2] 清理旧的测试目录（sougou-scel、qq-pinyin-txt、qq-qpyd、qq-qcel等）

**检查点**: 此时, 用户故事 1 和 2 都应该独立运行 - 已覆盖10+种输入法格式，30+测试用例基于真实的测试文件

---

## 阶段 5: 用户故事 3 - 回归测试与持续集成(优先级: P3)

**目标**: 集成到GitHub Actions工作流，实现每次代码提交后自动运行测试，生成JUnit XML报告用于CI系统展示

**独立测试**: 提交一个测试性代码变更到GitHub，验证CI系统自动运行测试并在PR中显示结果，测试失败时能清楚显示差异

### 用户故事 3 的实施

#### JUnit XML 报告实现

- [x] T080 [P] [US3] 在 `lib/report-generator.sh` 中实现 `generate_junit_xml()` 函数：生成符合JUnit XML格式的测试报告
- [x] T081 [P] [US3] 在 JUnit XML 中包含每个测试的执行时间、状态（passed/failed）、错误信息
- [x] T082 [US3] 在 `run-tests.sh` 中实现 `--xml` 参数：生成JUnit XML报告到 `reports/test-results.xml`
- [x] T083 [US3] 验证生成的XML格式：使用 xmllint 工具验证格式正确性

#### GitHub Actions 工作流配置

- [x] T084 [US3] 创建 `.github/workflows/integration-tests.yml`：配置CI工作流
- [x] T085 [US3] 配置工作流触发条件：push 到 main 分支、pull_request 事件
- [x] T086 [US3] 配置工作流步骤：构建 ImeWlConverterCmd CLI 工具（dotnet build）
- [x] T087 [US3] 配置工作流步骤：运行集成测试（./run-tests.sh --all --xml）
- [x] T088 [US3] 配置工作流步骤：上传测试报告（使用 actions/upload-artifact 或 test-reporter）
- [x] T089 [US3] 配置测试失败时的行为：标记构建失败，显示错误摘要

#### CI 环境适配

- [x] T090 [US3] 在 `run-tests.sh` 中添加 CI 环境检测：识别 GitHub Actions 环境变量
- [x] T091 [US3] 在 CI 环境下自动启用 `--xml` 和 `-v` 参数
- [x] T092 [US3] 在 CI 环境下禁用彩色输出（避免日志中的控制字符）
- [x] T093 [US3] 优化 CI 日志输出：使用 GitHub Actions 的日志分组功能

#### 跨平台 CI 测试

- [x] T094 [P] [US3] 配置 GitHub Actions matrix：在 Ubuntu、macOS、Windows（通过 Git Bash）上运行测试
- [x] T095 [US3] 处理平台差异：确保路径分隔符、换行符、编码在所有平台上一致
- [ ] T096 [US3] 验证跨平台测试：在所有三个平台上运行测试套件，确保结果一致

#### 测试历史和趋势

- [x] T097 [US3] 配置测试报告保留策略：保存最近30天的测试报告作为artifacts
- [x] T098 [US3] 在 README.md 中添加测试状态徽章：显示最新的测试通过状态
- [x] T099 [US3] 在 `tests/integration/README.md` 中添加CI集成说明：如何在本地模拟CI环境运行测试

#### 最终验证

- [ ] T100 [US3] 提交完整的集成测试代码到功能分支 `001-integration-tests`
- [ ] T101 [US3] 创建 Pull Request 到 main 分支，验证 CI 自动触发
- [ ] T102 [US3] 验证 PR 中显示测试结果：查看通过率、失败详情、执行时间
- [ ] T103 [US3] 故意引入一个转换错误，验证 CI 能检测并报告失败

**检查点**: 所有用户故事现在应该独立功能化 - CI自动化已完成，每次代码提交自动运行测试

---

## 阶段 6: 完善与横切关注点

**目的**: 影响多个用户故事的改进，确保符合所有章程要求

### 文档完善（章程原则II）

- [x] T104 [P] 完善 `tests/integration/README.md`：添加故障排查指南、常见问题解答
- [x] T105 [P] 在项目根目录 README.md 中完善集成测试章节：添加徽章、快速开始链接、覆盖范围说明
- [x] T106 [P] 为每个格式的测试数据目录添加或完善 README.md：说明测试场景、维护方法、数据来源
- [ ] T107 [P] 创建 `tests/integration/docs/adding-tests.md`：详细的中文指南，说明如何添加新测试用例（<10分钟）
- [ ] T108 [P] 创建 `tests/integration/docs/troubleshooting.md`：常见问题和解决方案

### 质量保证（章程原则I）

- [ ] T109 运行完整测试套件验证：在本地环境运行 `./run-tests.sh --all`，确认所有测试通过
- [ ] T110 验证测试覆盖范围：确认覆盖10+种输入法格式，30+测试用例（基于真实的单元测试文件）
- [ ] T111 验证性能目标：确认完整测试套件在5分钟内完成
- [ ] T112 验证错误检测能力：确认能检测并报告至少95%的已知转换Bug

### 代码质量（Shell脚本最佳实践）

- [ ] T113 [P] 代码审查和清理：确保所有Shell脚本符合Google Shell Style Guide
- [ ] T114 [P] 添加或完善中文注释：确保所有函数、关键逻辑都有清晰的中文注释
- [ ] T115 [P] 使用 shellcheck 进行静态分析：修复所有警告和错误
- [ ] T116 [P] 优化脚本性能：减少不必要的子进程调用、优化循环逻辑

### 跨平台验证（章程原则IV）

- [ ] T117 在 Windows 上测试：使用 Git Bash 运行完整测试套件，验证所有功能正常
- [ ] T118 在 Linux 上测试：在 Ubuntu 20.04+ 上运行完整测试套件
- [ ] T119 在 macOS 上测试：在 macOS 10.15+ 上运行完整测试套件
- [ ] T120 验证路径处理：确保相对路径在所有平台上正确工作
- [ ] T121 验证编码处理：确保 UTF-8、GBK、UTF-16 编码在所有平台上正确转换

### 用户体验优化

- [ ] T122 [P] 优化终端输出：使用表格格式展示测试摘要，提高可读性
- [ ] T123 [P] 添加进度指示：在执行长时间测试时显示进度（如 [3/30] 正在测试...）
- [ ] T124 [P] 优化错误消息：确保所有错误消息清晰、可操作，包含解决建议
- [ ] T125 验证 quickstart.md 场景：按照 quickstart.md 的步骤操作，确认新用户能在5分钟内运行第一个测试

### 最终验证和发布准备

- [ ] T126 运行完整的验收测试：验证所有用户故事的验收场景都能通过
- [ ] T127 生成最终测试报告：展示测试覆盖范围、通过率、性能指标
- [ ] T128 更新项目文档：确保所有文档与实际实现一致
- [ ] T129 准备 Pull Request：整理提交历史，编写详细的 PR 描述

---

## 依赖关系与执行顺序

### 阶段依赖关系

```
阶段1: 设置
  ↓
阶段2: 基础（阻塞所有用户故事）
  ↓
  ├─→ 阶段3: 用户故事1 (P1) 🎯 MVP ← 可以在这里停止并交付
  ├─→ 阶段4: 用户故事2 (P2)
  └─→ 阶段5: 用户故事3 (P3)
       ↓
阶段6: 完善
```

**关键路径**: 设置 → 基础 → 用户故事1（MVP）

### 用户故事依赖关系

- **用户故事 1 (P1)**: 可在基础(阶段 2)后开始 - 无其他故事依赖 - 🎯 **推荐作为MVP**
- **用户故事 2 (P2)**: 可在基础(阶段 2)后开始 - 独立于US1，但会复用US1的脚本框架
- **用户故事 3 (P3)**: 可在基础(阶段 2)后开始 - 独立于US1/US2，但需要US1/US2的测试用例才有意义

### 每个用户故事内部

- **用户故事 1**: 测试数据准备 → 测试配置 → 核心功能 → 运行器完善 → 文档验证
- **用户故事 2**: 各格式测试数据可并行创建 → 编码测试 → 性能测试 → 整合验证
- **用户故事 3**: JUnit XML实现 → GitHub Actions配置 → CI环境适配 → 跨平台测试 → 最终验证

### 并行机会

**阶段1 设置**: T002、T003、T004 可并行（不同目录）

**阶段2 基础**: T006、T007、T008、T009 可并行（不同文件）

**阶段3 用户故事1**:
- T013、T014、T015、T016 可并行（准备不同的测试数据文件）
- T028、T029 可并行（不同的文档文件）

**阶段4 用户故事2**:
- T032-T036 (QQ拼音txt)、T037-T040 (QQ qpyd)、T041-T044 (QQ qcel)、T045-T049 (百度bdict)、T050-T053 (Rime)、T054-T058 (灵格斯ld2)、T059-T062 (自定义)、T063-T066 (无拼音) 可并行（不同格式目录）
- 每个格式内部的文件复制任务可并行

**阶段5 用户故事3**:
- T080、T081 可并行（JUnit XML函数实现）
- T094 使用 GitHub Actions matrix 自动并行跨平台测试

**阶段6 完善**:
- T104-T108 可并行（不同的文档文件）
- T113-T116 可并行（不同方面的代码质量检查）

---

## 并行示例

### 用户故事 1 - 测试数据准备

```bash
# 可以同时启动这些任务（不同文件）：
任务: "复制 src/ImeWlConverterCoreTest/Test/唐诗300首【官方推荐】.scel 到 tests/integration/test-cases/sougou-scel/tangshi300.scel"
任务: "使用CLI工具转换 tangshi300.scel 为文本格式，生成预期输出"
任务: "复制 src/ImeWlConverterCoreTest/Test/诗词名句大全.scel"
任务: "创建 README.md"
```

### 用户故事 2 - 多格式测试数据

```bash
# 可以同时为不同格式创建测试数据：
任务: "在 tests/integration/test-cases/qq-pinyin-txt/ 创建完整测试数据集"
任务: "在 tests/integration/test-cases/qq-qpyd/ 创建完整测试数据集"
任务: "在 tests/integration/test-cases/qq-qcel/ 创建完整测试数据集"
任务: "在 tests/integration/test-cases/baidu-bdict/ 创建完整测试数据集"
任务: "在 tests/integration/test-cases/rime/ 创建完整测试数据集"
任务: "在 tests/integration/test-cases/lingoes-ld2/ 创建完整测试数据集"
任务: "在 tests/integration/test-cases/self-defining/ 创建完整测试数据集"
任务: "在 tests/integration/test-cases/no-pinyin/ 创建完整测试数据集"
```

### 用户故事 3 - 跨平台测试

```bash
# GitHub Actions 自动并行运行：
矩阵: [ubuntu-latest, macos-latest, windows-latest]
# 三个平台同时运行完整测试套件
```

---

## 实施策略

### 🎯 仅 MVP（推荐新用户快速开始）

**目标**: 最快交付可用的集成测试框架

1. ✅ 完成阶段 1: 设置（4个任务，约30分钟）
2. ✅ 完成阶段 2: 基础（8个任务，约2-3小时）
3. 完成阶段 3: 用户故事 1（19个任务，约4-6小时）
4. **🛑 停止并验证**: 运行 `./run-tests.sh -s sougou-scel`，确认能够：
   - 执行搜狗拼音.scel格式的转换测试
   - 检测转换结果差异
   - 生成易读的终端报告
5. 投入使用：开始用于日常开发回归测试

**MVP 交付物**:
- 可运行的测试框架
- 1种输入法格式（搜狗拼音.scel）的测试覆盖
- 基于真实的单元测试文件创建的测试用例
- 完整的使用文档

**MVP 用时估算**: 1-2个工作日

---

### 📈 增量交付（推荐生产环境）

**目标**: 逐步增加测试覆盖范围，每个里程碑都可独立交付价值

**里程碑 1: MVP（用户故事1）**
1. 完成设置 + 基础 + 用户故事1
2. 验证：能够测试搜狗拼音.scel格式转换
3. 交付：开始用于开发环境测试
4. **价值**: 立即可以发现搜狗拼音相关的Bug

**里程碑 2: 多格式覆盖（用户故事2）**
1. 完成阶段4: 用户故事2（48个任务，约8-12小时）
2. 验证：`./run-tests.sh --all` 覆盖10+种格式、30+测试用例
3. 交付：全面的格式转换测试
4. **价值**: 覆盖项目中所有真实的测试文件，显著提高Bug检测能力

**里程碑 3: CI集成（用户故事3）**
1. 完成阶段5: 用户故事3（24个任务，约4-6小时）
2. 验证：在GitHub PR中自动显示测试结果
3. 交付：自动化回归测试
4. **价值**: 每次代码提交自动验证，防止回归问题

**里程碑 4: 生产就绪（完善）**
1. 完成阶段6: 完善（26个任务，约4-6小时）
2. 验证：跨平台测试、文档完整、代码质量
3. 交付：生产级测试框架
4. **价值**: 可靠、易维护、文档完善的测试系统

**总用时估算**: 4-6个工作日

---

### 👥 并行团队策略

**适用场景**: 有2-3个开发人员同时工作

**阶段1-2: 一起完成基础设施**（必须串行，约半天）
- 所有人一起完成设置和基础阶段
- 确保测试框架骨架搭建完成

**阶段3+: 并行开发用户故事**
```
开发人员 A: 负责用户故事1（基础测试框架）
  ↓
  完成后协助用户故事3（CI集成）

开发人员 B: 负责用户故事2前半部分（QQ拼音、百度拼音、Rime）
  ↓
  完成后处理性能测试和编码测试

开发人员 C: 负责用户故事2后半部分（灵格斯、自定义格式、无拼音）
  ↓
  完成后协助文档完善
```

**并行策略**:
1. 基础完成后，US1、US2、US3 可以同时开工
2. US2 内部可以按格式分工（每人负责2-3种格式）
3. 阶段6的文档任务可以分配给不同人员

**总用时估算**: 2-3个工作日（假设3人团队）

---

## 成功标准验证清单

完成所有任务后，使用此清单验证是否满足 spec.md 中定义的成功标准：

- [ ] **SC-001**: 新开发者能在5分钟内完成设置并运行第一个测试（按照 quickstart.md 操作）
- [ ] **SC-002**: 单个测试（文件<1MB）执行时间 <10秒
- [ ] **SC-003**: 完整测试套件（10+种格式、30+用例）能在5分钟内完成
- [ ] **SC-004**: 能检测并报告至少95%的已知转换Bug（通过手动引入错误验证）
- [ ] **SC-005**: 测试失败时，错误报告清楚指出具体差异，无需额外调试工具
- [ ] **SC-006**: 新测试用例添加过程 <10分钟（准备数据 + 配置）
- [ ] **SC-007**: 测试报告以易读格式展示（彩色输出、表格、通过率统计）
- [ ] **SC-008**: CI环境中的失败率 <1%（排除真实Bug）

---

## 注意事项

### 格式约定
- ✅ [P] 任务 = 不同文件，无依赖关系，可并行执行
- ✅ [Story] 标签 = 将任务映射到特定用户故事（US1、US2、US3）
- ✅ 每个任务包含确切的文件路径
- ✅ 所有任务按执行顺序编号（T001-T129）

### 章程遵循
- ✅ **测试优先**（原则I）：本功能本身就是测试框架，为其他代码提供测试能力
- ✅ **中文注释**（原则II）：所有Shell脚本函数、文档、注释使用中文
- ✅ **编码规范**（原则III）：遵循Google Shell Style Guide，使用shellcheck验证
- ✅ **跨平台**（原则IV）：使用POSIX命令，支持Windows/Linux/macOS
- ✅ **模块化**（原则VI）：测试框架独立于主代码库，易于扩展

### 开发最佳实践
- ✅ 在每个任务或逻辑组后提交代码
- ✅ 在每个检查点停止，独立验证用户故事
- ✅ 使用描述性的提交消息（中文）
- ✅ 避免：模糊任务、文件冲突、破坏独立性的跨故事依赖
- ✅ **重要**: 直接使用 `src/ImeWlConverterCoreTest/Test/` 中的真实测试文件，确保测试数据的真实性和有效性

### Shell脚本最佳实践
- ✅ 使用 `set -euo pipefail` 启用严格错误处理
- ✅ 所有变量使用引号：`"${VARIABLE}"`
- ✅ 使用 `[[ ]]` 而非 `[ ]` 进行条件判断
- ✅ 避免使用 `eval`，优先使用数组
- ✅ 使用 `local` 声明函数内变量
- ✅ 提供清晰的错误消息和退出码

---

## 总结

**总任务数**: 129个任务

**任务分布**:
- 阶段1（设置）: 4个任务
- 阶段2（基础）: 8个任务
- 阶段3（用户故事1 - MVP）: 19个任务 🎯
- 阶段4（用户故事2）: 48个任务（大幅增加，覆盖10+种格式）
- 阶段5（用户故事3）: 24个任务
- 阶段6（完善）: 26个任务

**并行机会**: 
- 设置阶段：3个并行任务
- 基础阶段：4个并行任务
- 用户故事1：4个并行任务
- 用户故事2：35个并行任务（按格式分组，大幅增加）
- 用户故事3：2个并行任务
- 完善阶段：9个并行任务
- **总计约57个任务可并行执行**

**独立测试标准**:
- ✅ **US1**: 运行 `./run-tests.sh -s sougou-scel`，验证搜狗拼音.scel格式转换测试通过
- ✅ **US2**: 运行 `./run-tests.sh --all`，验证10+种格式、30+测试用例全部通过
- ✅ **US3**: 提交PR，验证CI自动运行测试并显示结果

**建议的MVP范围**: 仅用户故事1（阶段1-3，共31个任务）
- 最快交付时间：1-2个工作日
- 立即可用：能够测试搜狗拼音.scel格式转换
- 后续可增量添加：更多格式（US2）和CI集成（US3）

**重大改进**:
- 🎯 **基于真实测试文件**: 所有测试用例都基于 `src/ImeWlConverterCoreTest/Test/` 中现有的真实测试文件
- 📈 **大幅增加覆盖范围**: 从5种格式增加到10+种格式，从20个测试用例增加到30+个测试用例
- 🔍 **覆盖更多场景**: 包含编码测试（GBK、UTF-8无BOM）、大文件性能测试、特殊格式（包含英文字母）等
- ✅ **真实可靠**: 测试数据来自项目多年积累的真实测试文件，确保测试的有效性

**技术亮点**:
- 🚀 零依赖：仅使用Shell脚本和系统自带工具
- 🎯 黑盒测试：不依赖被测代码的内部实现
- 🔄 易于维护：Shell脚本简单直观，学习曲线低
- 🌐 跨平台：支持Windows、Linux、macOS
- 📊 CI友好：生成JUnit XML，无缝集成GitHub Actions
- 📁 真实数据：直接使用项目中已验证的真实测试文件
