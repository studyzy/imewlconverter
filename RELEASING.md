# 发布指南

本文档说明如何发布新版本的深蓝词库转换工具。

## 版本号管理

本项目使用**自动化版本号生成机制**。版本号由 [MinVer](https://github.com/adamralph/minver) 从 Git tag 自动生成，无需手动修改代码中的版本号。

### 版本号格式

- **Git tag 格式**：`vX.Y.Z`（如 `v3.4.0`）
- **程序版本号**：`X.Y.Z.0`（如 `3.4.0.0`）
- **开发版本**：`X.Y.Z-dev.N`（如 `3.3.1-dev.1`）

版本号遵循[语义化版本规范 (Semantic Versioning)](https://semver.org/lang/zh-CN/)：
- **主版本号(X)**：不兼容的 API 变更
- **次版本号(Y)**：向后兼容的功能性新增
- **修订号(Z)**：向后兼容的问题修正

## 发布新版本

### 准备工作

1. **确保所有变更已合并到主分支**
   ```bash
   git checkout main
   git pull origin main
   ```

2. **确认构建和测试通过**
   ```bash
   # 本地构建测试
   dotnet build src/ImeWlConverterCmd/ImeWlConverterCmd.csproj --configuration Release
   
   # 如果有测试，运行测试
   dotnet test
   ```

3. **更新 CHANGELOG 或发布说明**（可选）
   - 记录本次发布的主要变更
   - 列出修复的 Bug
   - 列出新增功能

### 创建 Release

1. **创建版本 tag**
   ```bash
   # 格式：vX.Y.Z
   git tag v3.4.0
   
   # 或者添加注释（推荐）
   git tag -a v3.4.0 -m "Release version 3.4.0"
   ```

2. **推送 tag 到远程仓库**
   ```bash
   git push origin v3.4.0
   ```

3. **自动构建和发布**
   - GitHub Actions 会自动检测到新 tag
   - 触发 `.github/workflows/release.yml` 工作流
   - 自动构建所有平台的二进制文件
   - 自动创建 GitHub Release 并上传构建产物

4. **验证发布**
   - 访问 [GitHub Releases 页面](https://github.com/studyzy/imewlconverter/releases)
   - 检查最新 Release 是否已创建
   - 下载并测试各平台的二进制文件
   - 确认版本号显示正确

### 版本号验证

发布后，验证各平台二进制文件的版本号：

- **命令行工具**：
  ```bash
  ./ImeWlConverterCmd --version
  # 应显示：3.4.0.0
  ```

- **Windows GUI**：查看"关于"对话框
- **macOS GUI**：查看"关于"对话框或 Finder 中的应用信息

## 预发布版本（可选）

如果需要创建 beta、rc 等预发布版本：

```bash
# Beta 版本
git tag v3.4.0-beta.1
git push origin v3.4.0-beta.1

# Release Candidate 版本
git tag v3.4.0-rc.1
git push origin v3.4.0-rc.1
```

预发布版本的程序版本号格式：`3.4.0-beta.1`

## 热修复发布

如果需要在已发布的版本上进行热修复：

1. **基于 tag 创建修复分支**
   ```bash
   git checkout v3.4.0
   git checkout -b hotfix/3.4.1
   ```

2. **进行修复并提交**
   ```bash
   git add .
   git commit -m "Fix: 修复XXX问题"
   ```

3. **创建新 tag**
   ```bash
   git tag v3.4.1
   git push origin hotfix/3.4.1
   git push origin v3.4.1
   ```

4. **合并回主分支**
   ```bash
   git checkout main
   git merge hotfix/3.4.1
   git push origin main
   ```

## 本地开发构建

在本地开发时，无需创建 tag。MinVer 会自动生成开发版本号：

```bash
# 在有 tag 的 commit 上构建
dotnet build  # 版本号：3.4.0.0

# 在 tag 后的 commit 上构建
dotnet build  # 版本号：3.4.1-dev.1（自动递增）
```

### 查看当前版本号

```bash
# 方法 1：使用 Makefile
make version

# 方法 2：直接使用 MSBuild
cd src/ImeWlConverterCore
dotnet msbuild -getProperty:Version -nologo

# 方法 3：查看 Git 状态
git describe --tags --long
```

## 故障排除

### 问题：版本号不正确

**症状**：构建的程序版本号不是预期值

**解决方法**：
1. 确认 Git history 完整：
   ```bash
   git fetch --tags --force
   git describe --tags
   ```

2. 清理并重新构建：
   ```bash
   dotnet clean
   dotnet build
   ```

3. 检查 MinVer 输出：
   ```bash
   dotnet build -p:MinVerVerbosity=detailed | grep MinVer
   ```

### 问题：非 Git 环境构建版本号为 0.0.0

**症状**：从源码 tarball（不含 .git 目录）构建时，版本号显示为 `0.0.0.0`

**原因**：MinVer 需要 Git 仓库才能从 tag 生成版本号。在发行版打包系统（如 Gentoo Portage）中，源码通常从 tarball 解压，不包含 `.git` 目录。

**解决方法**：
1. **推荐方式**：使用环境变量 `PACKAGE_VERSION` 指定版本号
   ```bash
   export PACKAGE_VERSION=3.3.1
   dotnet build
   ```
   
   构建脚本示例（适用于打包系统）：
   ```bash
   # 从文件名或其他来源获取版本号
   VERSION=3.3.1
   
   # 设置环境变量
   export PACKAGE_VERSION=${VERSION}
   
   # 构建项目
   dotnet build --configuration Release
   ```

2. **验证版本号**：
   ```bash
   ./ImeWlConverterCmd --version
   # 应显示：3.3.1.0
   ```

3. **Gentoo ebuild 示例**：
   ```bash
   src_compile() {
       export PACKAGE_VERSION=${PV}  # PV 是 Gentoo 的包版本变量
       dotnet build --configuration Release || die
   }
   ```

### 问题：CI 构建失败

**症状**：GitHub Actions 构建失败，提示找不到版本号

**解决方法**：
1. 检查 `.github/workflows/release.yml` 和 `commit.yml`
2. 确认 `actions/checkout@v4` 使用了 `fetch-depth: 0`
3. 查看 CI 日志中的 MinVer 输出

### 问题：tag 创建错误

**症状**：误创建了错误的 tag

**解决方法**：
```bash
# 删除本地 tag
git tag -d v3.4.0

# 删除远程 tag
git push origin :refs/tags/v3.4.0

# 重新创建正确的 tag
git tag v3.4.1
git push origin v3.4.1
```

## 参考资料

- [语义化版本规范](https://semver.org/lang/zh-CN/)
- [MinVer 文档](https://github.com/adamralph/minver)
- [GitHub Actions 文档](https://docs.github.com/en/actions)
- [Git 标签管理](https://git-scm.com/book/zh/v2/Git-%E5%9F%BA%E7%A1%80-%E6%89%93%E6%A0%87%E7%AD%BE)
