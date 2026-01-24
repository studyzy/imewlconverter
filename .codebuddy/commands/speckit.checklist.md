---
description: 基于用户需求为当前功能生成自定义检查清单.
---

## 清单目的: "需求编写的单元测试"

**核心概念**: 清单是**需求编写的单元测试** - 它们验证特定领域中需求的质量、清晰度和完整性.

**不用于验证/测试**: 
- ❌ 不是"验证按钮点击正确"
- ❌ 不是"测试错误处理有效"
- ❌ 不是"确认 API 返回 200"
- ❌ 不是检查代码/实现是否符合规范

**用于需求质量验证**: 
- ✅ "是否为所有卡片类型定义了视觉层次需求？"(完整性)
- ✅ "'突出显示'是否通过具体尺寸/位置进行了量化？"(清晰度)
- ✅ "所有交互元素的悬停状态需求是否一致？"(一致性)
- ✅ "是否为键盘导航定义了可访问性需求？"(覆盖度)
- ✅ "规范是否定义了 logo 图像加载失败时的处理？"(边缘情况)

**比喻**: 如果你的规范是用英文编写的代码, 那么清单就是它的单元测试套件. 你测试的是需求是否编写良好、完整、明确并准备好实施 - 而不是实现是否有效.

## 用户输入

```text
$ARGUMENTS
```

在继续之前, 你**必须**考虑用户输入(如果不为空).

## 执行步骤

1. **设置**: 从仓库根目录运行 `.specify/scripts/bash/check-prerequisites.sh --json` 并解析JSON以获取FEATURE_DIR和AVAILABLE_DOCS列表.
   - 所有文件路径必须是绝对路径.
   - 对于参数中的单引号如"I'm Groot", 使用转义语法: 例如 'I'\''m Groot'(或者尽可能使用双引号: "I'm Groot").

2. **澄清意图(动态)**: 推导最多三个初始上下文澄清问题(无预编目录). 它们必须: 
   - 从用户的表述 + 从规范/计划/任务中提取的信号生成
   - 只询问实质上改变清单内容的信息
   - 如果在`$ARGUMENTS`中已经明确, 则单独跳过
   - 优先考虑精确性而非广度

   Generation algorithm:
   1. Extract signals: feature domain keywords (e.g., auth, latency, UX, API), risk indicators ("critical", "must", "compliance"), stakeholder hints ("QA", "review", "security team"), and explicit deliverables ("a11y", "rollback", "contracts").
   2. Cluster signals into candidate focus areas (max 4) ranked by relevance.
   3. Identify probable audience & timing (author, reviewer, QA, release) if not explicit.
   4. Detect missing dimensions: scope breadth, depth/rigor, risk emphasis, exclusion boundaries, measurable acceptance criteria.
   5. Formulate questions chosen from these archetypes:
      - Scope refinement (e.g., "Should this include integration touchpoints with X and Y or stay limited to local module correctness?")
      - Risk prioritization (e.g., "Which of these potential risk areas should receive mandatory gating checks?")
      - Depth calibration (e.g., "Is this a lightweight pre-commit sanity list or a formal release gate?")
      - Audience framing (e.g., "Will this be used by the author only or peers during PR review?")
      - Boundary exclusion (e.g., "Should we explicitly exclude performance tuning items this round?")
      - Scenario class gap (e.g., "No recovery flows detected—are rollback / partial failure paths in scope?")

   Question formatting rules:
   - If presenting options, generate a compact table with columns: Option | Candidate | Why It Matters
   - Limit to A–E options maximum; omit table if a free-form answer is clearer
   - Never ask the user to restate what they already said
   - Avoid speculative categories (no hallucination). If uncertain, ask explicitly: "Confirm whether X belongs in scope."

   Defaults when interaction impossible:
   - Depth: Standard
   - Audience: Reviewer (PR) if code-related; Author otherwise
   - Focus: Top 2 relevance clusters

   Output the questions (label Q1/Q2/Q3). After answers: if ≥2 scenario classes (Alternate / Exception / Recovery / Non-Functional domain) remain unclear, you MAY ask up to TWO more targeted follow‑ups (Q4/Q5) with a one-line justification each (e.g., "Unresolved recovery path risk"). Do not exceed five total questions. Skip escalation if user explicitly declines more.

3. **理解用户请求**: 结合 `$ARGUMENTS` + 澄清答案: 
   - 推导清单主题(例如: security, review, deploy, ux)
   - 整合用户明确提到的必需项目
   - 将焦点选择映射到类别框架
   - 从规范/计划/任务中推断任何缺失的上下文(不要虚构)

4. **加载功能上下文**: 从 FEATURE_DIR 读取: 
   - spec.md: 功能需求和范围
   - plan.md(如果存在): 技术细节、依赖关系
   - tasks.md(如果存在): 实施任务

   **上下文加载策略**: 
   - 仅加载与活动焦点区域相关的必要部分(避免全文转储)
   - 优先将长部分总结为简洁的场景/需求要点
   - 使用渐进式披露: 仅在检测到差距时添加后续检索
   - 如果源文档很大, 生成临时摘要项目而不是嵌入原始文本

5. **生成清单** - 创建"需求的单元测试": 
   - Create `FEATURE_DIR/checklists/` directory if it doesn't exist
   - Generate unique checklist filename:
     - Use short, descriptive name based on domain (e.g., `ux.md`, `api.md`, `security.md`)
     - Format: `[domain].md` 
     - If file exists, append to existing file
   - Number items sequentially starting from CHK001
   - Each `/speckit.checklist` run creates a NEW file (never overwrites existing checklists)

   **核心原则 - 测试需求, 而非实现**: 
   每个清单项目必须评估需求本身, 检查: 
   - **完整性**: 所有必要的需求是否存在？
   - **清晰度**: 需求是否明确无歧义且具体？
   - **一致性**: 需求之间是否相互一致？
   - **可测量性**: 需求是否可以客观验证？
   - **覆盖度**: 是否涵盖了所有场景/边缘情况？

   **类别结构** - 按需求质量维度分组项目: 
   - **需求完整性**(所有必要的需求是否已记录？)
   - **需求清晰度**(需求是否具体且无歧义？)
   - **需求一致性**(需求是否一致且无冲突？)
   - **验收标准质量**(成功标准是否可测量？)
   - **场景覆盖度**(是否涵盖了所有流程/情况？)
   - **边缘情况覆盖度**(是否定义了边界条件？)
   - **非功能性需求**(性能、安全性、可访问性等 - 是否已指定？)
   - **依赖关系和假设**(是否已记录和验证？)
   - **歧义和冲突**(什么 NEEDS CLARIFICATION？)

   **如何编写清单项目 - "需求编写的单元测试"**: 
   
   ❌ **WRONG** (Testing implementation):
   - "Verify landing page displays 3 episode cards"
   - "Test hover states work on desktop"
   - "Confirm logo click navigates home"
   
   ✅ **CORRECT** (Testing requirements quality):
   - "Are the exact number and layout of featured episodes specified?" [Completeness]
   - "Is 'prominent display' quantified with specific sizing/positioning?" [Clarity]
   - "Are hover state requirements consistent across all interactive elements?" [Consistency]
   - "Are keyboard navigation requirements defined for all interactive UI?" [Coverage]
   - "Is the fallback behavior specified when logo image fails to load?" [Edge Cases]
   - "Are loading states defined for asynchronous episode data?" [Completeness]
   - "Does the spec define visual hierarchy for competing UI elements?" [Clarity]
   
   **项目结构**: 
   Each item should follow this pattern:
   - Question format asking about requirement quality
   - Focus on what's WRITTEN (or not written) in the spec/plan
   - Include quality dimension in brackets [Completeness/Clarity/Consistency/etc.]
   - Reference spec section `[Spec §X.Y]` when checking existing requirements
   - Use `[Gap]` marker when checking for missing requirements
   
   **按质量维度分类的示例**: 
   
   完整性: 
   - "Are error handling requirements defined for all API failure modes? [Gap]"
   - "Are accessibility requirements specified for all interactive elements? [Completeness]"
   - "Are mobile breakpoint requirements defined for responsive layouts? [Gap]"
   
   清晰度: 
   - "Is 'fast loading' quantified with specific timing thresholds? [Clarity, Spec §NFR-2]"
   - "Are 'related episodes' selection criteria explicitly defined? [Clarity, Spec §FR-5]"
   - "Is 'prominent' defined with measurable visual properties? [Ambiguity, Spec §FR-4]"
   
   一致性: 
   - "Do navigation requirements align across all pages? [Consistency, Spec §FR-10]"
   - "Are card component requirements consistent between landing and detail pages? [Consistency]"
   
   覆盖度: 
   - "Are requirements defined for zero-state scenarios (no episodes)? [Coverage, Edge Case]"
   - "Are concurrent user interaction scenarios addressed? [Coverage, Gap]"
   - "Are requirements specified for partial data loading failures? [Coverage, Exception Flow]"
   
   可测量性: 
   - "Are visual hierarchy requirements measurable/testable? [Acceptance Criteria, Spec §FR-1]"
   - "Can 'balanced visual weight' be objectively verified? [Measurability, Spec §FR-2]"

   **场景分类与覆盖度**(需求质量焦点): 
   - Check if requirements exist for: Primary, Alternate, Exception/Error, Recovery, Non-Functional scenarios
   - For each scenario class, ask: "Are [scenario type] requirements complete, clear, and consistent?"
   - If scenario class missing: "Are [scenario type] requirements intentionally excluded or missing? [Gap]"
   - Include resilience/rollback when state mutation occurs: "Are rollback requirements defined for migration failures? [Gap]"

   **可追溯性要求**: 
   - MINIMUM: ≥80% of items MUST include at least one traceability reference
   - Each item should reference: spec section `[Spec §X.Y]`, or use markers: `[Gap]`, `[Ambiguity]`, `[Conflict]`, `[Assumption]`
   - If no ID system exists: "Is a requirement & acceptance criteria ID scheme established? [Traceability]"

   **发现和解决问题**(需求质量问题): 
   Ask questions about the requirements themselves:
   - Ambiguities: "Is the term 'fast' quantified with specific metrics? [Ambiguity, Spec §NFR-1]"
   - Conflicts: "Do navigation requirements conflict between §FR-10 and §FR-10a? [Conflict]"
   - Assumptions: "Is the assumption of 'always available podcast API' validated? [Assumption]"
   - Dependencies: "Are external podcast API requirements documented? [Dependency, Gap]"
   - Missing definitions: "Is 'visual hierarchy' defined with measurable criteria? [Gap]"

   **内容整合**: 
   - Soft cap: If raw candidate items > 40, prioritize by risk/impact
   - Merge near-duplicates checking the same requirement aspect
   - If >5 low-impact edge cases, create one item: "Are edge cases X, Y, Z addressed in requirements? [Coverage]"

   **🚫 ABSOLUTELY PROHIBITED** - These make it an implementation test, not a requirements test:
   - ❌ Any item starting with "Verify", "Test", "Confirm", "Check" + implementation behavior
   - ❌ References to code execution, user actions, system behavior
   - ❌ "Displays correctly", "works properly", "functions as expected"
   - ❌ "Click", "navigate", "render", "load", "execute"
   - ❌ Test cases, test plans, QA procedures
   - ❌ Implementation details (frameworks, APIs, algorithms)
   
   **✅ REQUIRED PATTERNS** - These test requirements quality:
   - ✅ "Are [requirement type] defined/specified/documented for [scenario]?"
   - ✅ "Is [vague term] quantified/clarified with specific criteria?"
   - ✅ "Are requirements consistent between [section A] and [section B]?"
   - ✅ "Can [requirement] be objectively measured/verified?"
   - ✅ "Are [edge cases/scenarios] addressed in requirements?"
   - ✅ "Does the spec define [missing aspect]?"

6. **结构参考**: 按照 `.specify/templates/checklist-template.md` 中的规范模板生成清单, 包括标题、元部分、类别标题和 ID 格式. 如果模板不可用, 使用: H1 标题、purpose/created 元行、包含 `- [ ] CHK### <requirement item>` 行的 `##` 类别部分, ID 从 CHK001 开始全局递增.

7. **报告**: 输出创建清单的完整路径、项目数量, 并提醒用户每次运行都会创建新文件. 总结: 
   - 选择的焦点区域
   - 深度级别
   - 执行者/时间
   - 任何整合的用户明确指定的必需项目

**重要说明**: 每次 `/speckit.checklist` 命令调用都会创建一个使用简短描述性名称的清单文件, 除非文件已存在. 这允许: 

- 创建多种不同类型的清单(例如: `ux.md`, `test.md`, `security.md`)
- 使用简单、易记的文件名来表明清单用途
- 在 `checklists/` 文件夹中轻松识别和导航

为避免混乱, 请使用描述性类型, 并在完成后清理过时的清单.

## 示例清单类型和示例项目

**UX 需求质量**: `ux.md`

示例项目(测试需求, 而非实现): 
- "Are visual hierarchy requirements defined with measurable criteria? [Clarity, Spec §FR-1]"
- "Is the number and positioning of UI elements explicitly specified? [Completeness, Spec §FR-1]"
- "Are interaction state requirements (hover, focus, active) consistently defined? [Consistency]"
- "Are accessibility requirements specified for all interactive elements? [Coverage, Gap]"
- "Is fallback behavior defined when images fail to load? [Edge Case, Gap]"
- "Can 'prominent display' be objectively measured? [Measurability, Spec §FR-4]"

**API 需求质量**: `api.md`

示例项目: 
- "Are error response formats specified for all failure scenarios? [Completeness]"
- "Are rate limiting requirements quantified with specific thresholds? [Clarity]"
- "Are authentication requirements consistent across all endpoints? [Consistency]"
- "Are retry/timeout requirements defined for external dependencies? [Coverage, Gap]"
- "Is versioning strategy documented in requirements? [Gap]"

**性能需求质量**: `performance.md`

示例项目: 
- "Are performance requirements quantified with specific metrics? [Clarity]"
- "Are performance targets defined for all critical user journeys? [Coverage]"
- "Are performance requirements under different load conditions specified? [Completeness]"
- "Can performance requirements be objectively measured? [Measurability]"
- "Are degradation requirements defined for high-load scenarios? [Edge Case, Gap]"

**安全需求质量**: `security.md`

示例项目: 
- "Are authentication requirements specified for all protected resources? [Coverage]"
- "Are data protection requirements defined for sensitive information? [Completeness]"
- "Is the threat model documented and requirements aligned to it? [Traceability]"
- "Are security requirements consistent with compliance obligations? [Consistency]"
- "Are security failure/breach response requirements defined? [Gap, Exception Flow]"

## 反例: 什么不要做

**❌ 错误 - 这些测试实现, 而非需求: **

```markdown
- [ ] CHK001 - Verify landing page displays 3 episode cards [Spec §FR-001]
- [ ] CHK002 - Test hover states work correctly on desktop [Spec §FR-003]
- [ ] CHK003 - Confirm logo click navigates to home page [Spec §FR-010]
- [ ] CHK004 - Check that related episodes section shows 3-5 items [Spec §FR-005]
```

**✅ 正确 - 这些测试需求质量: **

```markdown
- [ ] CHK001 - Are the number and layout of featured episodes explicitly specified? [Completeness, Spec §FR-001]
- [ ] CHK002 - Are hover state requirements consistently defined for all interactive elements? [Consistency, Spec §FR-003]
- [ ] CHK003 - Are navigation requirements clear for all clickable brand elements? [Clarity, Spec §FR-010]
- [ ] CHK004 - Is the selection criteria for related episodes documented? [Gap, Spec §FR-005]
- [ ] CHK005 - Are loading state requirements defined for asynchronous episode data? [Gap]
- [ ] CHK006 - Can "visual hierarchy" requirements be objectively measured? [Measurability, Spec §FR-001]
```

**关键区别: **
- 错误: 测试系统是否正常工作
- 正确: 测试需求是否编写正确
- 错误: 验证行为
- 正确: 验证需求质量
- 错误: "它是否做 X？"
- 正确: "X 是否明确指定？"
