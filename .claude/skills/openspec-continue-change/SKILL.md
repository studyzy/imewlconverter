---
name: openspec-continue-change
description: 通过创建下一个产出物继续处理 OpenSpec 变更。当用户想要推进其变更、创建下一个产出物或继续其工作流程时使用。
license: MIT
compatibility: Requires openspec CLI.
metadata:
  author: openspec
  version: "1.0"
  generatedBy: "1.0.2"
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
   Parse the JSON to understand current state. The response includes:
   - `schemaName`: The workflow schema being used (e.g., "spec-driven")
   - `artifacts`: Array of artifacts with their status ("done", "ready", "blocked")
   - `isComplete`: Boolean indicating if all artifacts are complete

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
   - Parse the JSON. The key fields are:
     - `context`: Project background (constraints for you - do NOT include in output)
     - `rules`: Artifact-specific rules (constraints for you - do NOT include in output)
     - `template`: The structure to use for your output file
     - `instruction`: Schema-specific guidance
     - `outputPath`: Where to write the artifact
     - `dependencies`: Completed artifacts to read for context
   - **Create the artifact file**:
     - Read any completed dependency files for context
     - Use `template` as the structure - fill in its sections
     - Apply `context` and `rules` as constraints when writing - but do NOT copy them into the file
     - Write to the output path specified in instructions
   - Show what was created and what's now unlocked
   - STOP after creating ONE artifact

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

**spec-driven schema** (proposal → specs → design → tasks):
- **proposal.md**: Ask user about the change if not clear. Fill in Why, What Changes, Capabilities, Impact.
  - The Capabilities section is critical - each capability listed will need a spec file.
- **specs/<capability>/spec.md**: Create one spec per capability listed in the proposal's Capabilities section (use the capability name, not the change name).
- **design.md**: Document technical decisions, architecture, and implementation approach.
- **tasks.md**: Break down implementation into checkboxed tasks.

For other schemas, follow the `instruction` field from the CLI output.

**Guardrails**
- Create ONE artifact per invocation
- Always read dependency artifacts before creating a new one
- Never skip artifacts or create out of order
- If context is unclear, ask the user before creating
- Verify the artifact file exists after writing before marking progress
- Use the schema's artifact sequence, don't assume specific artifact names
- **IMPORTANT**: `context` and `rules` are constraints for YOU, not content for the file
  - Do NOT copy `<context>`, `<rules>`, `<project_context>` blocks into the artifact
  - These guide what you write, but should never appear in the output
