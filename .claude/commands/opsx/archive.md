---
name: "OPSX: Archive"
description: 归档实验性工作流中已完成的变更
category: Workflow
tags: [workflow, archive, experimental]
---

归档实验性工作流中已完成的变更。

**输入**：可选择在 `/opsx:archive` 后指定变更名称（例如，`/opsx:archive add-auth`）。如果省略，检查是否可以从对话上下文中推断出来。如果模糊或不明确，你必须提示可用的变更。

**步骤**

1. **如果没有提供变更名称，提示选择**

   运行 `openspec-cn list --json` 获取可用变更。使用 **AskUserQuestion tool** 让用户选择。

   仅显示活动变更（未归档的）。
   如果可用，包括每个变更使用的 Schema。

   **重要提示**：不要猜测或自动选择变更。始终让用户选择。

2. **检查产出物完成状态**

   运行 `openspec-cn status --change "<name>" --json` 检查产出物完成情况。

   解析 JSON 以了解：
   - `schemaName`: 正在使用的工作流
   - `artifacts`: 产出物列表及其状态（`done` 或其他）

   **如果有任何产出物未 `done`：**
   - 显示列出未完成产出物的警告
   - 提示用户确认是否继续
   - 如果用户确认，则继续

3. **检查任务完成状态**

   阅读任务文件（通常是 `tasks.md`）以检查未完成的任务。

   统计标记为 `- [ ]`（未完成）与 `- [x]`（已完成）的任务。

   **如果发现未完成的任务：**
   - 显示警告，显示未完成任务的数量
   - 提示用户确认是否继续
   - 如果用户确认，则继续

   **如果没有任务文件存在：** 继续，无需任务相关警告。

4. **评估增量规格说明同步状态**

   在 `openspec/changes/<name>/specs/` 检查增量规格说明。如果不存在，不提示同步直接继续。

   **如果存在增量规格说明：**
   - 将每个增量规格说明与其在 `openspec/specs/<capability>/spec.md` 的相应主规格说明进行比较
   - 确定将应用哪些更改（添加、修改、删除、重命名）
   - 在提示前显示合并摘要

   **提示选项：**
   - 如果需要更改："立即同步（推荐）"，"不同步直接归档"
   - 如果已同步："立即归档"，"仍要同步"，"取消"

   如果用户选择同步，执行 `/opsx:sync` 逻辑。无论选择如何都继续归档。

5. **执行归档**

   如果归档目录不存在，则创建它：
   ```bash
   mkdir -p openspec/changes/archive
   ```

   使用当前日期生成目标名称：`YYYY-MM-DD-<change-name>`

   **检查目标是否已存在：**
   - 如果是：失败并报错，建议重命名现有归档或使用不同日期
   - 如果否：将变更目录移动到归档

   ```bash
   mv openspec/changes/<name> openspec/changes/archive/YYYY-MM-DD-<name>
   ```

6. **显示摘要**

   显示归档完成摘要，包括：
   - 变更名称
   - 使用的 Schema
   - 归档位置
   - 规格说明同步状态（已同步 / 跳过同步 / 无增量规格说明）
   - 任何警告的注释（未完成的产出物/任务）

**成功时的输出**

```
## 归档完成

**变更：** <change-name>
**Schema：** <schema-name>
**归档至：** openspec/changes/archive/YYYY-MM-DD-<name>/
**规范：** ✓ 已同步到主规范

所有产出物已完成。所有任务已完成。
```

**成功时的输出（无增量规范）**

```
## 归档完成

**变更：** <change-name>
**Schema：** <schema-name>
**归档至：** openspec/changes/archive/YYYY-MM-DD-<name>/
**规范：** 无增量规范

所有产出物已完成。所有任务已完成。
```

**成功时的输出（带警告）**

```
## 归档完成（带警告）

**变更：** <change-name>
**Schema：** <schema-name>
**归档至：** openspec/changes/archive/YYYY-MM-DD-<name>/
**规格说明：** 跳过同步（用户选择跳过）

**警告：**
- 带有 2 个未完成产出物的归档
- 带有 3 个未完成任务的归档
- 增量规格说明同步已跳过（用户选择跳过）

如果这不是故意的，请检查归档。
```

**错误时的输出（归档已存在）**

```
## 归档失败

**变更：** <change-name>
**目标：** openspec/changes/archive/YYYY-MM-DD-<name>/

目标归档目录已存在。

**选项：**
1. 重命名现有归档
2. 如果是重复的，删除现有归档
3. 等待不同的日期再归档
```

**防护措施**
- 如果未提供变更，始终提示选择
- 使用产出物图（openspec status --json）进行完成度检查
- 不要在警告时阻止归档 - 只需告知并确认
- 移动到归档时保留 .openspec.yaml（它与目录一起移动）
- 显示清晰的操作摘要
- 如果请求同步，使用 /opsx:sync 方法（代理驱动）
- If delta specs exist, always run the sync assessment and show the combined summary before prompting
