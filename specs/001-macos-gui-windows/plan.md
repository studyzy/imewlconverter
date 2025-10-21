# 实施计划: [FEATURE]

**分支**: `[###-feature-name]` | **日期**: [DATE] | **规范**: [link]
**输入**: 来自 `/specs/[###-feature-name]/spec.md` 的功能规范

**注意**: 此模板由 `/speckit.plan` 命令填充. 执行工作流程请参见 `.specify/templates/commands/plan.md`.

## 摘要

[从功能规范中提取: 主要需求 + 研究得出的技术方法]

## 技术背景

**语言/版本**: C# 8.0+/ .NET 6.0+（Win 版已用 .NET 6.0，Mac 版建议同）
**主要依赖**: Avalonia UI（Mac 版）、WinForms（Win 版）、.NET 标准库、第三方输入法词库解析库（项目自有/外部）
**存储**: 文件（本地词库文件读写，无数据库）
**测试**: xUnit/NUnit（单元测试），手动 GUI 对照测试，后续可补充 UI 自动化测试（NEEDS CLARIFICATION: Mac GUI 自动化测试方案）
**目标平台**: macOS（Intel/Apple Silicon），Windows 10+，Linux（命令行）
**项目类型**: 跨平台桌面应用（Win: WinForms，Mac: Avalonia，目录分别为 IME WL Converter Win/ ImeWlConverterMac）
**性能目标**: 单次批量转换 100 个文件时，平均每个文件处理时间 ≤ Windows 版 1.2 倍
**约束条件**: UI 响应流畅，支持拖拽/批量/多格式，兼容主流 macOS 版本，界面与 Win 版一致
**规模/范围**: 20+ 词库格式，典型界面 3-5 个主窗口，支持批量/拖拽/导入导出/设置等全部 Win 版功能
**规模/范围**: [领域特定, 例如: 10k 用户、1M 行代码、50 个屏幕 或 需要澄清]

## 章程检查

*门控: 必须在阶段 0 研究前通过. 阶段 1 设计后重新检查. *

[基于章程文件确定的门控条件]

- 所有新功能在合并前必须有对应的单元测试或契约测试（尤其是词库格式的解析与导出）。
- CI 必须在 PR 合并前通过（测试、静态检查、构建打包）。
- 任何破坏向后兼容的变更（例如：更改核心转换规则或移除格式映射）必须升级版本策略并提供迁移脚本。
- 日志与可观测性要求：转换任务需产生日志（JSON 可解析），错误应被记录且不会导致程序崩溃。

**门控检查流程**:
- 阶段 0 结束时：研究报告（research.md）必须清晰列出所有 NEEDS CLARIFICATION 项并给出决策或建议；否则中止阶段 1。
- 阶段 1 结束时：必须展示契约测试或示例转换用例，CI 配置（测试+构建）能在本地/CI 环境通过。


## 项目结构

### 文档(此功能)

```
specs/[###-feature]/
├── plan.md              # 此文件 (/speckit.plan 命令输出)
├── research.md          # 阶段 0 输出 (/speckit.plan 命令)
├── data-model.md        # 阶段 1 输出 (/speckit.plan 命令)
├── quickstart.md        # 阶段 1 输出 (/speckit.plan 命令)
├── contracts/           # 阶段 1 输出 (/speckit.plan 命令)
└── tasks.md             # 阶段 2 输出 (/speckit.tasks 命令 - 非 /speckit.plan 创建)
```

### 源代码(仓库根目录)
<!--
  需要操作: 将下面的占位符树结构替换为此功能的具体布局.
  删除未使用的选项, 并使用真实路径(例如: apps/admin、packages/something)扩展所选结构.
  交付的计划不得包含选项标签.
-->

```
# [如未使用请删除] 选项 1: 单一项目(默认)
src/
├── models/
├── services/
├── cli/
└── lib/

tests/
├── contract/
├── integration/
└── unit/

# [如未使用请删除] 选项 2: Web 应用程序(检测到"前端" + "后端"时)
backend/
├── src/
│   ├── models/
│   ├── services/
│   └── api/
└── tests/

frontend/
├── src/
│   ├── components/
│   ├── pages/
│   └── services/
└── tests/

# [如未使用请删除] 选项 3: 移动端 + API(检测到 "iOS/Android" 时)
api/
└── [同上后端结构]

ios/ 或 android/
└── [平台特定结构: 功能模块、UI 流程、平台测试]
```

**结构决策**: [记录所选结构并引用上面捕获的真实目录]

**结构决策说明**: 本项目保留现有仓库结构：Windows GUI 位于 `src/IME WL Converter Win/`，Mac GUI 位于 `src/ImeWlConverterMac/`。共享核心逻辑位于 `src/ImeWlConverterCore/`。因此采用“单一项目多目标输出”结构，分别为 Win/Mac/CLI 产物。

## 复杂度跟踪

*仅在章程检查有必须证明的违规时填写*

| 违规 | 为什么需要 | 拒绝更简单替代方案的原因 |
|-----------|------------|-------------------------------------|
| [例如: 第 4 个项目] | [当前需求] | [为什么 3 个项目不够] |
| [例如: 仓储模式] | [特定问题] | [为什么直接数据库访问不够] |

## 阶段任务与时间估计

预计总工期（粗略估计）: 3-6 周

- 阶段 0 (研究与决策): 3-5 天
  - 产出: `research.md`（框架选型、测试方案、兼容性替代）
  - 门控: 所有 NEEDS CLARIFICATION 要么解决要么有明确的备选方案

- 阶段 1 (设计与合同): 1-2 周
  - 产出: `data-model.md`、`contracts/`、`quickstart.md`、契约测试用例样例
  - 门控: CI 能在本地跑通核心测试，契约测试覆盖主要格式转换用例

- 阶段 2 (实现与任务拆分): 1-3 周
  - 产出: 任务清单 (`tasks.md`)、分配与时间表、初步实现分支 PR

## 阶段 0 输出检查点
- `research.md` 存在且列出所有 NEEDS CLARIFICATION
- 对 UI 框架的推荐与理由
- Mac GUI 自动化测试策略草案
- 列出需要在代码中修改的 Windows 特有实现点并提出替代方案

## 阶段 1 输出检查点
- `data-model.md` 完成并映射到代码中实体
- `contracts/` 中至少包含 CLI 转换命令的输入/输出约定
- `quickstart.md` 包括开发与用户运行步骤
- CI 配置草案（构建 Mac 目标）

