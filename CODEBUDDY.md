<!-- OPENSPEC:START -->
# OpenSpec 使用说明

这些说明适用于在此项目中工作的AI助手。

## 语言偏好设置

**默认使用中文**：除非明确说明使用英文，否则所有输出都应使用中文，包括：
- 文档内容
- 代码注释
- 提交信息
- 规范说明

## 工作流程

当请求满足以下条件时，始终打开`@/openspec/AGENTS.md`：
- 提及规划或提案（如提案、规范、变更、计划等词语）
- 引入新功能、重大变更、架构变更或大型性能/安全工作时
- 听起来不明确，需要在编码前了解权威规范时

使用`@/openspec/AGENTS.md`了解：
- 如何创建和应用变更提案
- 规范格式和约定
- 项目结构和指南

保持此托管块，以便'openspec-cn update'可以刷新说明。

<!-- OPENSPEC:END -->

## 项目约定

### 版本号管理

**重要**：项目使用自动化版本号生成机制，版本号从 Git tag 自动生成。

- **禁止手动修改**：不要手动修改以下位置的版本号：
  - `src/ImeWlConverterCore/ConstantString.cs` 中的 `VERSION` 字段
  - 任何 `.csproj` 文件中的 `<Version>` 标签
  
- **版本号来源**：版本号由 MinVer 从 Git tag 自动生成
  - 格式：`vX.Y.Z` → `X.Y.Z.0`
  - 配置文件：`src/Directory.Build.props`
  
- **发布新版本**：创建并推送 Git tag
  ```bash
  git tag v3.4.0
  git push origin v3.4.0
  ```

- **非 Git 环境构建**：在发行版打包系统等非 Git 环境中构建时
  ```bash
  export PACKAGE_VERSION=3.3.1
  dotnet build
  ```

详见 [RELEASING.md](RELEASING.md) 了解完整的发布流程。
