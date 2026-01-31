---
name: "OPSX: Fast Forward"
description: 一键创建变更并生成实现所需的所有产出物
category: Workflow
tags: [workflow, artifacts, experimental]
---

快速完成产出物创建 - 生成开始实现所需的一切。

**输入**：`/opsx:ff` 之后的参数是变更名称（kebab-case），或用户想要构建内容的描述。

**步骤**

1. **如果没有提供输入，询问他们想要构建什么**

   使用 **AskUserQuestion tool**（开放式，无预设选项）询问：
   > "您想要处理什么变更？请描述您想要构建或修复的内容。"

   根据他们的描述，推导出一个 kebab-case 名称（例如："add user authentication" → `add-user-auth`）。

   **重要提示**：在不了解用户想要构建什么的情况下，请勿继续。

2. **创建变更目录**
   ```bash
   openspec-cn new change "<name>"
   ```
   这将在 `openspec/changes/<name>/` 创建一个脚手架变更。

3. **获取产出物构建顺序**
   ```bash
   openspec-cn status --change "<name>" --json
   ```
   解析 JSON 以获取：
   - `applyRequires`: 实现前所需的产出物 ID 数组（例如：`["tasks"]`）
   - `artifacts`: 所有产出物及其状态和依赖项的列表

4. **按顺序创建产出物直到准备好应用**

   使用 **TodoWrite tool** 跟踪产出物的进度。

   按依赖顺序循环遍历产出物（没有待处理依赖项的产出物优先）：

   a. **对于每个 `ready`（依赖项已满足）的产出物**：
      - 获取指令：
        ```bash
        openspec-cn instructions <artifact-id> --change "<name>" --json
        ```
      - The instructions JSON includes:
        - `context`: Project background (constraints for you - do NOT include in output)
        - `rules`: Artifact-specific rules (constraints for you - do NOT include in output)
        - `template`: The structure to use for your output file
        - `instruction`: Schema-specific guidance for this artifact type
        - `outputPath`: Where to write the artifact
        - `dependencies`: Completed artifacts to read for context
      - Read any completed dependency files for context
      - Create the artifact file using `template` as the structure
      - Apply `context` and `rules` as constraints - but do NOT copy them into the file
      - Show brief progress: "✓ Created <artifact-id>"

   b. **继续直到所有 `applyRequires` 产出物完成**
      - 创建每个产出物后，重新运行 `openspec-cn status --change "<name>" --json`
      - 检查 `applyRequires` 中的每个产出物 ID 在 artifacts 数组中是否具有 `status: "done"`
      - 当所有 `applyRequires` 产出物完成时停止

   c. **如果产出物需要用户输入**（上下文不清楚）：
      - 使用 **AskUserQuestion tool** 进行澄清
      - 然后继续创建

5. **显示最终状态**
   ```bash
   openspec-cn status --change "<name>"
   ```

**输出**

完成所有产出物后，总结：
- 变更名称和位置
- 已创建产出物的列表及简要描述
- 准备就绪："所有产出物已创建！准备好实现。"
- 提示："运行 `/opsx:apply` 以开始实现。"

**产出物创建指南**

- 遵循每个产出物类型的 `openspec-cn instructions` 中的 `instruction` 字段
- Schema 定义了每个产出物应包含的内容 - 遵循它
- 在创建新产出物之前阅读依赖产出物以获取上下文
- 使用 `template` 作为起点，根据上下文填写

**护栏**
- 创建实现所需的所有产出物（由 Schema 的 `apply.requires` 定义）
- 在创建新产出物之前始终阅读依赖产出物
- 如果上下文极其不清楚，询问用户 - 但倾向于做出合理的决定以保持势头
- 如果同名变更已存在，询问用户是否要继续它或创建一个新的
- 在继续下一个之前，验证写入后每个产出物文件是否存在
