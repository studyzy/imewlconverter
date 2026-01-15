---
name: OpenSpec: 实施
description: 实施已批准的OpenSpec变更并保持任务同步。
argument-hint: "[change-id]"
---
<!-- OPENSPEC:START -->
**护栏规则**
- 优先使用简单、最小的实现，仅在请求或明确需要时才添加复杂性。
- 将变更紧密限制在请求的结果范围内。
- 如果需要额外的OpenSpec约定或澄清，请参考`openspec/AGENTS.md`（位于`openspec/`目录中—如果看不到，请运行`ls openspec`或`openspec-cn update`）。

**步骤**
将这些步骤作为TODO跟踪并逐一完成。
1. 阅读`openspec/changes/<id>/proposal.md`、`design.md`（如果存在）和`tasks.md`以确认范围和验收标准。
2. 按顺序完成任务，保持编辑最小化并专注于请求的变更。
3. 在更新状态前确认完成—确保`tasks.md`中的每个项目都已完成。
4. 所有工作完成后更新清单，使每个任务标记为`- [x]`并反映实际情况。
5. 需要额外上下文时参考`openspec-cn list`或`openspec-cn show <item>`。

**参考**
- 如果在实施过程中需要提案的额外上下文，请使用`openspec-cn show <id> --json --deltas-only`。
<!-- OPENSPEC:END -->
