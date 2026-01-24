---
description: 将现有任务转换为可操作的、按依赖关系排序的 GitHub 议题，基于可用的设计制品。
tools: ['github/github-mcp-server/issue_write']
---

## 用户输入

```text
$ARGUMENTS
```

你必须**在继续之前考虑用户输入**（如果非空）。

## 大纲

1. 从仓库根目录运行 `.specify/scripts/bash/check-prerequisites.sh --json --require-tasks --include-tasks` 并解析 FEATURE_DIR 和 AVAILABLE_DOCS 列表。所有路径必须是绝对路径。对于参数中的单引号如 "I'm Groot"，使用转义语法：例如 'I'\''m Groot'（或尽可能使用双引号："I'm Groot"）。
1. 从执行的脚本中，提取 **任务** 的路径。
1. 通过运行以下命令获取 Git 远程仓库：

```bash
git config --get remote.origin.url
```

**仅当远程仓库是 GITHUB URL 时才继续下一步**

1. 对于列表中的每个任务，使用 GitHub MCP 服务器在与 Git 远程仓库对应的仓库中创建一个新议题。

**在任何情况下都不要在与远程 URL 不匹配的仓库中创建议题**
