---
name: OpenSpec: 提案
description: 搭建新的OpenSpec变更提案并进行严格验证。
argument-hint: "[feature description or request]"
---
<!-- OPENSPEC:START -->
**护栏规则**
- 优先使用简单、最小的实现，仅在请求或明确需要时才添加复杂性。
- 将变更紧密限制在请求的结果范围内。
- 如果需要额外的OpenSpec约定或澄清，请参考`openspec/AGENTS.md`（位于`openspec/`目录中—如果看不到，请运行`ls openspec`或`openspec-cn update`）。
- 识别任何模糊或不明确的细节，并在编辑文件前询问必要的后续问题。
- 在提案阶段不要编写任何代码。仅创建设计文档（proposal.md、tasks.md、design.md和规范增量）。实施在批准后的apply阶段进行。

**步骤**
1. 查看`openspec/project.md`，运行`openspec-cn list`和`openspec-cn list --specs`，并检查相关代码或文档（例如通过`rg`/`ls`）以使提案基于当前行为；注意任何需要澄清的空白。
2. 选择唯一的动词开头`change-id`并在`openspec/changes/<id>/`下创建`proposal.md`、`tasks.md`和`design.md`（需要时）。
3. 将变更映射为具体功能或需求，将多范围工作分解为具有明确关系和顺序的独立规范增量。
4. 当解决方案跨越多个系统、引入新模式或在提交规范前需要权衡讨论时，在`design.md`中记录架构推理。
5. 在`openspec/changes/<id>/specs/<capability>/spec.md`中起草规范增量（每个功能一个文件夹），使用`## 新增需求|修改需求|移除需求|重命名需求`，每个需求至少有一个`#### 场景：`，并在相关时交叉引用相关功能。
6. 将`tasks.md`起草为有序的小型可验证工作项列表，这些项提供用户可见的进展，包括验证（测试、工具），并突出显示依赖关系或可并行工作。
7. 使用`openspec-cn validate <id> --strict`验证并在分享提案前解决每个问题。

**参考**
- 当验证失败时，使用`openspec-cn show <id> --json --deltas-only`或`openspec-cn show <spec> --type spec`检查详情。
- 在编写新需求前，使用`rg -n "需求：|场景：" openspec/specs`搜索现有需求。
- 使用`rg <keyword>`、`ls`或直接文件读取探索代码库，以便提案与当前实现现实保持一致。
<!-- OPENSPEC:END -->
