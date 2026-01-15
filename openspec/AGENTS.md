# OpenSpec 使用说明

AI编程助手使用OpenSpec进行规范驱动开发的说明文档。

## 快速入门清单

- 搜索现有工作：`openspec-cn spec list --long`，`openspec-cn list`（仅使用`rg`进行全文搜索）
- 确定范围：新功能 vs 修改现有功能
- 选择唯一`change-id`：短横线命名法，动词开头（`add-`，`update-`，`remove-`，`refactor-`）
- 创建骨架：`proposal.md`，`tasks.md`，`design.md`（仅当需要时），以及受影响功能的增量规范
- 编写增量：使用`## 新增|修改|移除|重命名需求`；每个需求至少包含一个`#### 场景：`
- 验证：`openspec-cn validate [change-id] --strict`并修复问题
- 请求批准：在提案获得批准前不要开始实施

## 三阶段工作流

### 阶段 1：创建变更
当您需要时创建提案：
- 添加功能或特性
- 进行重大变更（API，架构）
- 更改架构或模式
- 优化性能（改变行为）
- 更新安全模式

触发条件（示例）：
- "帮我创建一个变更提案"
- "帮我规划一个变更"
- "帮我创建一个提案"
- "我想创建一个规范提案"
- "我想创建一个规范"

宽松匹配指南：
- 包含以下之一：`proposal`，`change`，`spec`
- 加上以下之一：`create`，`plan`，`make`，`start`，`help`

跳过提案的情况：
- 错误修复（恢复预期行为）
- 拼写错误、格式化、注释
- 依赖项更新（非重大变更）
- 配置更改
- 现有行为的测试

**工作流**
1. 查看`openspec/project.md`，`openspec-cn list`和`openspec-cn list --specs`以了解当前上下文。
2. 选择唯一的动词开头`change-id`并在`openspec/changes/<id>/`下创建`proposal.md`、`tasks.md`、可选的`design.md`，以及在`openspec/changes/<id>/`下的规范增量。
3. 使用`## 新增|修改|移除需求`编写规范增量，每个需求至少包含一个`#### 场景：`。
4. 运行`openspec-cn validate <id> --strict`并在分享提案前解决任何问题。

### 阶段 2：实施变更
将这些步骤作为TODO跟踪并逐一完成。
1. **阅读proposal.md** - 了解正在构建的内容
2. **阅读design.md**（如果存在） - 查看技术决策
3. **阅读tasks.md** - 获取实施清单
4. **按顺序实施任务** - 按顺序完成
5. **确认完成** - 确保`tasks.md`中的每个项目在更新状态前都已完成
6. **更新清单** - 所有工作完成后，将每个任务设置为`- [x]`以便清单反映实际情况
7. **批准门控** - 在提案获得审核和批准前不要开始实施

### 阶段 3：归档变更
部署后，创建单独的PR：
- 移动`changes/[name]/` → `changes/archive/YYYY-MM-DD-[name]/`
- 如果功能发生更改，更新`specs/`
- 对于仅工具变更，使用`openspec-cn archive <change-id> --skip-specs --yes`（始终明确传递变更ID）
- 运行`openspec-cn validate --strict`以确认归档的变更通过检查

## 任何任务前

**上下文检查清单：**
- [ ] 阅读`specs/[capability]/spec.md`中的相关规范
- [ ] 检查`changes/`中的待处理变更是否存在冲突
- [ ] 阅读`openspec/project.md`了解约定
- [ ] 运行`openspec-cn list`查看活动变更
- [ ] 运行`openspec-cn list --specs`查看现有功能

**创建规范前：**
- 始终检查功能是否已存在
- 优先修改现有规范而不是创建重复项
- 使用`openspec-cn show [spec]`查看当前状态
- 如果请求不明确，在创建骨架前询问1-2个澄清问题

### 搜索指南
- 枚举规范：`openspec-cn spec list --long`（或`--json`用于脚本）
- 枚举变更：`openspec-cn list`（或`openspec-cn change list --json` - 已弃用但可用）
- 显示详情：
  - 规范：`openspec-cn show <spec-id> --type spec`（使用`--json`进行过滤）
  - 变更：`openspec-cn show <change-id> --json --deltas-only`
- 全文搜索（使用ripgrep）：`rg -n "Requirement:|Scenario:|需求：|场景：" openspec/specs`

## 快速开始

### CLI 命令

```bash
# 基本命令
openspec-cn list                  # 列出活动变更
openspec-cn list --specs          # 列出规范
openspec-cn show [item]           # 显示变更或规范
openspec-cn validate [item]       # 验证变更或规范
openspec-cn archive <change-id> [--yes|-y]   # 部署后归档（添加--yes用于非交互式运行）

# 项目管理
openspec-cn init [path]           # 初始化OpenSpec
openspec-cn update [path]         # 更新说明文件

# 交互模式
openspec-cn show                  # 提示选择
openspec-cn validate              # 批量验证模式

# 调试
openspec-cn show [change] --json --deltas-only
openspec-cn validate [change] --strict
```

### 命令标志

- `--json` - 机器可读输出
- `--type change|spec` - 消除项目歧义
- `--strict` - 全面验证
- `--no-interactive` - 禁用提示
- `--skip-specs` - 归档时不更新规范
- `--yes`/`-y` - 跳过确认提示（非交互式归档）

## 目录结构

```
openspec/
├── project.md              # 项目约定
├── specs/                  # 当前真相 - 已构建的内容
│   └── [capability]/       # 单一专注功能
│       ├── spec.md         # 需求和场景
│       └── design.md       # 技术模式
├── changes/                # 提案 - 应该更改的内容
│   ├── [change-name]/
│   │   ├── proposal.md     # 为什么，什么，影响
│   │   ├── tasks.md        # 实施清单
│   │   ├── design.md       # 技术决策（可选；见标准）
│   │   └── specs/          # 增量变更
│   │       └── [capability]/
│   │           └── spec.md # ADDED/MODIFIED/REMOVED
│   └── archive/            # 已完成的变更
```

## 创建变更提案

### 决策树

```
新请求？
├─ 修复恢复规范行为的错误？ → 直接修复
├─ 拼写/格式/注释？ → 直接修复
├─ 新功能/能力？ → 创建提案
├─ 重大变更？ → 创建提案
├─ 架构变更？ → 创建提案
└─ 不明确？ → 创建提案（更安全）
```

### 提案结构

1. **创建目录：** `changes/[change-id]/`（短横线命名法，动词开头，唯一）

2. **编写proposal.md：**
```markdown
# 变更：[变更的简要描述]

## 为什么
[1-2句说明问题/机会]

## 变更内容
- [变更项目列表]
- [用**重大变更**标记破坏性变更]

## 影响
- 受影响规范：[列出功能]
- 受影响代码：[关键文件/系统]
```

3. **创建规范增量：** `specs/[capability]/spec.md`
```markdown
## 新增需求
### 需求：新功能
系统应提供...

#### 场景：成功案例
- **当** 用户执行操作
- **那么** 预期结果

## 修改需求
### 需求：现有功能
[完整的修改后需求]

## 移除需求
### 需求：旧功能
**原因**：[为什么移除]
**迁移**：[如何处理]
```
如果多个功能受到影响，在`changes/[change-id]/specs/<capability>/spec.md`下创建多个增量文件——每个功能一个。

4. **创建tasks.md：**
```markdown
## 1. 实施
- [ ] 1.1 创建数据库架构
- [ ] 1.2 实现API端点
- [ ] 1.3 添加前端组件
- [ ] 1.4 编写测试
```

5. **需要时创建design.md：**
如果以下任何情况适用，则创建`design.md`；否则省略：
- 跨领域变更（多个服务/模块）或新的架构模式
- 新的外部依赖或重要的数据模型变更
- 安全性、性能或迁移复杂性
- 在编码前从技术决策中受益的模糊性

最小`design.md`骨架：
```markdown
## 上下文
[背景、约束、利益相关者]

## 目标 / 非目标
- 目标：[...]
- 非目标：[...]

## 决策
- 决策：[什么和为什么]
- 考虑的替代方案：[选项 + 理由]

## 风险 / 权衡
- [风险] → 缓解措施

## 迁移计划
[步骤、回滚]

## 待决问题
- [...]
```

## 规范文件格式

### 关键：场景格式化

**正确**（使用####标题）：
```markdown
#### 场景：用户登录成功
- **当** 提供有效凭据
- **那么** 返回JWT令牌
```

**错误**（不要使用项目符号或粗体）：
```markdown
- **场景：用户登录**  ❌
**场景**：用户登录     ❌
### 场景：用户登录      ❌
```

每个需求必须至少有一个场景。

### 需求措辞
- 使用必须/禁止（或SHALL/MUST）表示规范要求（除非有意为非规范，避免使用should/may）

### 增量操作

- `## 新增需求` - 新功能
- `## 修改需求` - 变更行为
- `## 移除需求` - 弃用功能
- `## 重命名需求` - 名称变更

使用`trim(header)`匹配标题 - 忽略空格。

#### 何时使用新增 vs 修改
- 新增：引入可以独立作为需求的新功能或子功能。当变更正交时（例如添加"斜杠命令配置"）而不是改变现有需求的语义时，优先使用新增。
- 修改：更改现有需求的行为、范围或接受标准。始终粘贴完整、更新的需求内容（标题 + 所有场景）。归档器将用您在此提供的内容替换整个需求；部分增量将丢失以前的详细信息。
- 重命名：仅当名称更改时使用。如果同时更改行为，请使用重命名（名称）加上修改（内容）引用新名称。

常见陷阱：使用修改添加新关注点而不包含先前文本。这会导致归档时丢失详细信息。如果您没有明确更改现有需求，请在新增下添加新需求。

正确编写修改需求：
1) 在`openspec/specs/<capability>/spec.md`中找到现有需求。
2) 复制整个需求块（从`### 需求：...`到其场景）。
3) 粘贴到`## 修改需求`下并编辑以反映新行为。
4) 确保标题文本完全匹配（空格不敏感）并保持至少一个`#### 场景：`。

重命名示例：
```markdown
## 重命名需求
- FROM: `### 需求：登录`
- TO: `### 需求：用户认证`
```

## 故障排除

### 常见错误

**"变更必须至少有一个增量"**
- 检查`changes/[name]/specs/`是否存在.md文件
- 验证文件具有操作前缀（## 新增需求）

**"需求必须至少有一个场景"**
- 检查场景使用`#### 场景：`格式（4个井号）
- 不要使用项目符号或粗体作为场景标题

**静默场景解析失败**
- 需要精确格式：`#### 场景：名称`
- 调试：`openspec-cn show [change] --json --deltas-only`

### 验证提示

```bash
# 始终使用严格模式进行全面检查
openspec-cn validate [change] --strict

# 调试增量解析
openspec-cn show [change] --json | jq '.deltas'

# 检查特定需求
openspec-cn show [spec] --json -r 1
```

## 快乐路径脚本

```bash
# 1) 探索当前状态
openspec-cn spec list --long
openspec-cn list
# 可选全文搜索：
# rg -n "Requirement:|Scenario:|需求：|场景：" openspec/specs
# rg -n "^#|Requirement:|需求：" openspec/changes

# 2) 选择变更id并创建骨架
CHANGE=add-two-factor-auth
mkdir -p openspec/changes/$CHANGE/{specs/auth}
printf "## 为什么\n...\n\n## 变更内容\n- ...\n\n## 影响\n- ...\n" > openspec/changes/$CHANGE/proposal.md
printf "## 1. 实施\n- [ ] 1.1 ...\n" > openspec/changes/$CHANGE/tasks.md

# 3) 添加增量（示例）
cat > openspec/changes/$CHANGE/specs/auth/spec.md << 'EOF'
## 新增需求
### 需求：双因素认证
用户必须在登录时提供第二个因素。

#### 场景：需要OTP
- **当** 提供有效凭据
- **那么** 需要OTP挑战
EOF

# 4) 验证
openspec-cn validate $CHANGE --strict
```

## 多功能示例

```
openspec/changes/add-2fa-notify/
├── proposal.md
├── tasks.md
└── specs/
    ├── auth/
    │   └── spec.md   # 新增：双因素认证
    └── notifications/
        └── spec.md   # 新增：OTP邮件通知
```

auth/spec.md
```markdown
## 新增需求
### 需求：双因素认证
...
```

notifications/spec.md
```markdown
## 新增需求
### 需求：OTP邮件通知
...
```

## 最佳实践

### 简单优先
- 默认新代码<100行
- 单文件实现直到证明不足
- 没有明确理由避免框架
- 选择成熟、经过验证的模式

### 复杂性触发条件
仅在以下情况下添加复杂性：
- 性能数据显示当前解决方案太慢
- 具体规模要求（>1000用户，>100MB数据）
- 多个经过验证的用例需要抽象

### 明确引用
- 使用`file.ts:42`格式表示代码位置
- 将规范引用为`specs/auth/spec.md`
- 链接相关变更和PR

### 功能命名
- 使用动词-名词：`user-auth`，`payment-capture`
- 每个功能单一目的
- 10分钟可理解性规则
- 如果描述需要"AND"则拆分

### 变更ID命名
- 使用短横线命名法，简短且描述性：`add-two-factor-auth`
- 优先动词开头前缀：`add-`，`update-`，`remove-`，`refactor-`
- 确保唯一性；如果已存在，追加`-2`，`-3`等

## 工具选择指南

| 任务 | 工具 | 原因 |
|------|------|-----|
| 按模式查找文件 | Glob | 快速模式匹配 |
| 搜索代码内容 | Grep | 优化的正则搜索 |
| 读取特定文件 | Read | 直接文件访问 |
| 探索未知范围 | Task | 多步骤调查 |

## 错误恢复

### 变更冲突
1. 运行`openspec-cn list`查看活动变更
2. 检查重叠规范
3. 与变更所有者协调
4. 考虑合并提案

### 验证失败
1. 使用`--strict`标志运行
2. 检查JSON输出获取详情
3. 验证规范文件格式
4. 确保场景格式正确

### 缺少上下文
1. 首先阅读project.md
2. 检查相关规范
3. 查看最近归档
4. 请求澄清

## 快速参考

### 阶段指示器
- `changes/` - 提案，尚未构建
- `specs/` - 已构建并部署
- `archive/` - 已完成的变更

### 文件用途
- `proposal.md` - 为什么和什么
- `tasks.md` - 实施步骤
- `design.md` - 技术决策
- `spec.md` - 需求和行为

### CLI 要点
```bash
openspec-cn list              # 正在进行什么？
openspec-cn show [item]       # 查看详情
openspec-cn validate --strict # 正确吗？
openspec-cn archive <change-id> [--yes|-y]  # 标记完成（添加--yes用于自动化）
```

记住：规范是真相。变更是提案。保持它们同步。
