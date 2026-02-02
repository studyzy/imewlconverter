---
name: openspec-continue-change
description: 通过创建下一个产出物继续处理 OpenSpec 变更。当用户想要推进其变更、创建下一个产出物或继续其工作流程时使用。
license: MIT
compatibility: 需要 openspec CLI。
metadata:
  author: openspec
  version: "1.0"
  generatedBy: "1.1.1-1"
---

通过创建下一个产出物继续处理变更。

**输入**：可选指定变更名称。如果省略，检查是否可以从对话上下文中推断。如果模糊或不明确，你**必须**提示获取可用变更。

**步骤**

1. **如果没有提供变更名称，提示选择**

   运行 `openspec-cn list --json` 获取按最近修改排序的可用变更。然后使用 **AskUserQuestion tool** 让用户选择要处理哪个变更。

   展示前 3-4 个最近修改的变更作为选项，显示：
   - 变更名称
   - Schema（如果存在 `schema` 字段，否则为 "spec-driven"）
   - 状态（例如："0/5 tasks", "complete", "no tasks"）
   - 最近修改时间（来自 `lastModified` 字段）

   将最近修改的变更标记为 "(推荐)"，因为它很可能是用户想要继续的。

   **重要提示**：不要猜测或自动选择变更。始终让用户选择。

2. **检查当前状态**
   ```bash
   openspec-cn status --change "<name>" --json
   ```
   解析 JSON 以了解当前状态。响应包括：
   - `schemaName`：正在使用的工作流 schema（例如："spec-driven"）
   - `artifacts`：产出物数组及其状态（"done"、"ready"、"blocked"）
   - `isComplete`：布尔值，表示是否所有产出物都已完成

3. **根据状态行动**：

   ---

   **如果所有产出物已完成 (`isComplete: true`)**：
   - 祝贺用户
   - 显示最终状态，包括使用的 Schema
   - 建议："所有产出物已创建！您现在可以实现此变更或将其归档。"
   - 停止

   ---

   **如果产出物准备好创建**（状态显示有 `status: "ready"` 的产出物）：
   - 从状态输出中选择第一个 `status: "ready"` 的产出物
   - 获取其指令：
     ```bash
     openspec-cn instructions <artifact-id> --change "<name>" --json
     ```
   - 解析 JSON。关键字段包括：
     - `context`：项目背景（对你的约束 - 不要包含在输出中）
     - `rules`：产出物特定规则（对你的约束 - 不要包含在输出中）
     - `template`：输出文件使用的结构
     - `instruction`：schema 特定指导
     - `outputPath`：产出物写入路径
     - `dependencies`：已完成的依赖产出物（用于读取上下文）
   - **创建产出物文件**：
     - 阅读任何已完成的依赖文件以获取上下文
     - 使用 `template` 作为结构 - 填充各个章节
     - 在编写时将 `context` 和 `rules` 作为约束 - 但不要把它们复制进文件
     - 写入指令中指定的 outputPath
   - 展示创建了什么，以及现在解锁了什么
   - 创建一个产出物后停止

   ---

   **如果没有产出物准备好（全部受阻）**：
   - 在有效的 Schema 下不应发生这种情况
   - 显示状态并建议检查问题

4. **创建产出物后，显示进度**
   ```bash
   openspec-cn status --change "<name>"
   ```

**输出**

每次调用后，显示：
- 创建了哪个产出物
- 正在使用的 Schema 工作流
- 当前进度（N/M 完成）
- 现在解锁了哪些产出物
- 提示："想要继续吗？只需让我继续或告诉我下一步做什么。"

**产出物创建指南**

产出物类型及其用途取决于 Schema。使用指令输出中的 `instruction` 字段来了解要创建什么。

常见的产出物模式：

**spec-driven schema**（proposal → specs → design → tasks）：
- **proposal.md**：如果变更不清楚，先向用户确认。填写“为什么”“什么变化”“能力”“影响”。
  - “能力”部分很关键——列出的每个能力都需要一个 spec 文件。
- **specs/<capability>/spec.md**：为提案“能力”部分列出的每个能力创建一个 spec（使用 capability 名称，而不是 change 名称）。
- **design.md**：记录技术决策、架构和实现方法。
- **tasks.md**：把实现拆分为带复选框的任务。

对于其他 schema，遵循 CLI 输出中的 `instruction` 字段。

**护栏**
- 每次调用只创建一个产出物
- 创建新产出物前，总是先阅读依赖产出物
- 不要跳过产出物，也不要乱序创建
- 如果上下文不清楚，创建前先询问用户
- 写入后先确认产出物文件存在，再标记进度
- 使用 schema 的产出物顺序，不要假设固定的产出物名称
- **重要**：`context` 和 `rules` 是对你的约束，不是文件内容
  - 不要把 `<context>`、`<rules>`、`<project_context>` 块复制进产出物
  - 它们用于指导你写作，但绝不能出现在输出中
