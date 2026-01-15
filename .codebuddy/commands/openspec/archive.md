---
name: OpenSpec: 归档
description: 归档已部署的OpenSpec变更并更新规范。
argument-hint: "[change-id]"
---
<!-- OPENSPEC:START -->
**护栏规则**
- 优先使用简单、最小的实现，仅在请求或明确需要时才添加复杂性。
- 将变更紧密限制在请求的结果范围内。
- 如果需要额外的OpenSpec约定或澄清，请参考`openspec/AGENTS.md`（位于`openspec/`目录中—如果看不到，请运行`ls openspec`或`openspec-cn update`）。

**步骤**
1. 确定要归档的变更ID：
   - 如果此提示已包含特定变更ID（例如在由斜杠命令参数填充的`<ChangeId>`块内），在修剪空格后使用该值。
   - 如果对话松散地引用变更（例如通过标题或摘要），运行`openspec-cn list`以显示可能的ID，分享相关候选者，并确认用户意图归档哪一个。
   - 否则，查看对话，运行`openspec-cn list`，并询问用户要归档哪个变更；在继续前等待确认的变更ID。
   - 如果仍然无法识别单个变更ID，停止并告诉用户您还无法归档任何内容。
2. 通过运行`openspec-cn list`（或`openspec-cn show <id>`）验证变更ID，如果变更缺失、已归档或尚未准备好归档，则停止。
3. 运行`openspec-cn archive <id> --yes`，以便CLI在没有提示的情况下移动变更并应用规范更新（仅对仅工具工作使用`--skip-specs`）。
4. 查看命令输出以确认目标规范已更新且变更已放入`openspec/changes/archive/`。
5. 使用`openspec-cn validate --strict`验证，如果出现异常，使用`openspec-cn show <id>`检查。

**参考**
- 在归档前使用`openspec-cn list`确认变更ID。
- 使用`openspec-cn list --specs`检查刷新的规范，并在交前解决任何验证问题。
<!-- OPENSPEC:END -->
