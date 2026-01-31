---
name: "OPSX: New"
description: 使用实验性的产出物工作流 (OPSX) 启动新变更
category: Workflow
tags: [workflow, artifacts, experimental]
---

使用实验性的产出物驱动方法启动新变更。

**输入**：`/opsx:new` 之后的参数是变更名称（kebab-case），或用户想要构建内容的描述。

**步骤**

1. **如果没有提供输入，询问他们想要构建什么**

   使用 **AskUserQuestion tool**（开放式，无预设选项）询问：
   > "您想要处理什么变更？请描述您想要构建或修复的内容。"

   根据他们的描述，推导出一个 kebab-case 名称（例如："add user authentication" → `add-user-auth`）。

   **重要提示**：在不了解用户想要构建什么的情况下，请勿继续。

2. **确定工作流 Schema**

   除非用户明确要求不同的工作流，否则使用默认 Schema（省略 `--schema`）。

   **Use a different schema only if the user mentions:**
   - A specific schema name → use `--schema <name>`
   - "show workflows" or "what workflows" → run `openspec schemas --json` and let them choose

   **否则**：省略 `--schema` 以使用默认值。

3. **创建变更目录**
   ```bash
   openspec-cn new change "<name>"
   ```
   仅当用户请求特定工作流时才添加 `--schema <name>`。
   这将在 `openspec/changes/<name>/` 下使用所选 Schema 创建一个脚手架变更。

4. **显示产出物状态**
   ```bash
   openspec-cn status --change "<name>"
   ```
   这会显示哪些产出物需要创建，以及哪些已就绪（依赖项已满足）。

5. **获取第一个产出物的指令**
   第一个产出物取决于 Schema。检查状态输出，找到第一个状态为 "ready" 的产出物。
   ```bash
   openspec-cn instructions <first-artifact-id> --change "<name>"
   ```
   这会输出创建第一个产出物所需的模板和上下文。

6. **停止并等待用户指示**

**输出**

完成上述步骤后，进行总结：
- 变更名称和位置
- 正在使用的 Schema/工作流及其产出物顺序
- 当前状态（0/N 个产出物已完成）
- 第一个产出物的模板
- 提示："准备好创建第一个产出物了吗？运行 `/opsx:continue` 或描述此变更的内容，我将为您起草。"

**护栏**
- 不要立即创建任何产出物 —— 仅显示指令
- 不要跳过显示第一个产出物模板的步骤
- 如果名称无效（非 kebab-case），请求有效的名称
- 如果同名变更已存在，建议使用 `/opsx:continue` 代替
- 如果使用非默认工作流，请传递 --schema
