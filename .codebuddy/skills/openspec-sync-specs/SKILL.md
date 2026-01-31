---
name: openspec-sync-specs
description: 将变更中的增量规范同步到主规范。当用户想要使用增量规范中的更改更新主规范，而不归档该变更时使用。
license: MIT
compatibility: Requires openspec CLI.
metadata:
  author: openspec
  version: "1.0"
  generatedBy: "1.0.2"
---

将变更中的增量规范同步到主规范。

这是一个 **Agent 驱动** 的操作 - 你将读取增量规范并直接编辑主规范以应用更改。这允许智能合并（例如，添加场景而不复制整个需求）。

**输入**：可选指定变更名称。如果省略，检查是否可以从对话上下文中推断。如果模糊或不明确，你**必须**提示获取可用变更。

**步骤**

1. **如果没有提供变更名称，提示选择**

   运行 `openspec-cn list --json` 获取可用变更。使用 **AskUserQuestion tool** 让用户选择。

   显示具有增量规范（在 `specs/` 目录下）的变更。

   **重要提示**：不要猜测或自动选择变更。始终让用户选择。

2. **查找增量规范**

   在 `openspec/changes/<name>/specs/*/spec.md` 中查找增量规范文件。

   每个增量规范文件包含如下部分：
   - `## ADDED Requirements` - 要添加的新需求
   - `## MODIFIED Requirements` - 对现有需求的更改
   - `## REMOVED Requirements` - 要移除的需求
   - `## RENAMED Requirements` - 要重命名的需求（FROM:/TO: 格式）

   如果没有找到增量规范，通知用户并停止。

3. **对于每个增量规范，将更改应用到主规范**

   对于在 `openspec/changes/<name>/specs/<capability>/spec.md` 处具有增量规范的每个 capability：

   a. **阅读增量规范** 以了解预期的更改

   b. **阅读主规范** 于 `openspec/specs/<capability>/spec.md`（可能尚不存在）

   c. **智能地应用更改**：

      **ADDED Requirements:**
      - 如果需求在主规范中不存在 → 添加它
      - 如果需求已存在 → 更新它以匹配（视为隐式 MODIFIED）

      **MODIFIED Requirements:**
      - 在主规范中找到该需求
      - 应用更改 - 这可能是：
        - 添加新场景（不需要复制现有场景）
        - 修改现有场景
        - 更改需求描述
      - 保留增量中未提及的场景/内容

      **REMOVED Requirements:**
      - 从主规范中移除整个需求块

      **RENAMED Requirements:**
      - 找到 FROM 需求，重命名为 TO

   d. **创建新主规范** 如果 capability 尚不存在：
      - 创建 `openspec/specs/<capability>/spec.md`
      - 添加 Purpose 部分（可以简短，标记为 TBD）
      - 添加 Requirements 部分以及 ADDED 需求

4. **显示摘要**

   应用所有更改后，总结：
   - 哪些 capability 已更新
   - 做了什么更改（需求添加/修改/移除/重命名）

**增量规范格式参考**

```markdown
## ADDED Requirements

### Requirement: New Feature
The system SHALL do something new.

#### Scenario: Basic case
- **WHEN** user does X
- **THEN** system does Y

## MODIFIED Requirements

### Requirement: Existing Feature
#### Scenario: New scenario to add
- **WHEN** user does A
- **THEN** system does B

## REMOVED Requirements

### Requirement: Deprecated Feature

## RENAMED Requirements

- FROM: `### Requirement: Old Name`
- TO: `### Requirement: New Name`
```

**关键原则：智能合并**

与程序化合并不同，你可以应用 **部分更新**：
- 要添加场景，只需将该场景包含在 MODIFIED 下 - 不要复制现有场景
- 增量代表 *意图*，而不是整体替换
- 使用你的判断力合理地合并更改

**成功时的输出**

```
## 规范已同步：<change-name>

已更新主规范：

**<capability-1>**：
- 添加需求："New Feature"
- 修改需求："Existing Feature"（添加了 1 个场景）

**<capability-2>**：
- 创建了新规范文件
- 添加需求："Another Feature"

主规范现已更新。变更保持活动状态 - 在实现完成后归档。
```

**护栏**
- 在进行更改之前阅读增量规范和主规范
- 保留增量中未提及的现有内容
- 如果不清楚，询问澄清
- 在进行时显示你正在更改的内容
- 操作应该是幂等的 - 运行两次应给出相同的结果
