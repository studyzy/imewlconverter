---
description: 执行实施规划工作流，使用计划模板生成设计工件。
agent_scripts:
  sh: .specify/scripts/bash/update-agent-context.sh copilot
  ps: .specify/scripts/powershell/update-agent-context.ps1 -AgentType copilot
---

## 用户输入

```text
$ARGUMENTS
```

在继续之前，您**必须**考虑用户输入（如果不为空）。

## 大纲

1. **设置**：从仓库根目录运行 `.specify/scripts/bash/setup-plan.sh --json` 并解析 JSON 获取 FEATURE_SPEC、IMPL_PLAN、SPECS_DIR、BRANCH。对于参数中的单引号如 "I'm Groot"，使用转义语法：例如 'I'\''m Groot'（或尽可能使用双引号："I'm Groot"）。

2. **加载上下文**：读取 FEATURE_SPEC 和 `.specify/memory/constitution.md`。加载 IMPL_PLAN 模板（已复制）。

3. **执行计划工作流**：按照 IMPL_PLAN 模板中的结构：
   - 填充技术上下文（将未知项标记为"NEEDS CLARIFICATION"）
   - 从章程文档填充章程检查部分
   - 评估关卡（如果违规无正当理由则报错）
   - 阶段 0：生成 research.md（解决所有 NEEDS CLARIFICATION）
   - 阶段 1：生成 data-model.md、contracts/、quickstart.md
   - 阶段 1：通过运行代理脚本更新代理上下文
   - 设计后重新评估章程检查

4. **停止并报告**：命令在阶段 2 规划后结束。报告分支、IMPL_PLAN 路径和生成的工件。

## 阶段

### 阶段 0：大纲与研究

1. **从上述技术上下文中提取未知项**：
   - 每个 NEEDS CLARIFICATION → 研究任务
   - 每个依赖项 → 最佳实践任务
   - 每个集成 → 模式任务

2. **生成和分发研究代理**：
   ```
   For each unknown in Technical Context:
     Task: "Research {unknown} for {feature context}"
   For each technology choice:
     Task: "Find best practices for {tech} in {domain}"
   ```

3. **在 `research.md` 中整合发现**，使用格式：
   - Decision: [选择了什么]
   - Rationale: [为什么选择]
   - Alternatives considered: [还评估了什么]

**输出**：research.md，所有 NEEDS CLARIFICATION 已解决

### 阶段 1：设计与合同

**前提条件**：`research.md` 完成

1. **从功能规范中提取实体** → `data-model.md`：
   - 实体名称、字段、关系
   - 来自需求的验证规则
   - 状态转换（如适用）

2. **从功能需求生成 API 合同**：
   - 每个用户操作 → 端点
   - 使用标准 REST/GraphQL 模式
   - 将 OpenAPI/GraphQL 模式输出到 `/contracts/`

3. **代理上下文更新**：
   - 运行 `{AGENT_SCRIPT}`
   - 这些脚本检测正在使用哪个 AI 代理
   - 更新相应的代理特定上下文文件
   - 仅添加当前计划中的新技术
   - 保留标记之间的手动添加内容

**输出**：data-model.md、/contracts/*、quickstart.md、代理特定文件

## 关键规则

- 使用绝对路径
- 关卡失败或未解决的澄清事项时报错
