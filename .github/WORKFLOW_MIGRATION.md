# GitHub Actions Workflow 重构说明

## 概述

本次重构将 GitHub Actions 工作流程升级到符合现代最佳实践的标准,提高了构建效率、可维护性和安全性。

## 主要变更

### 1. 文件变更
- ❌ 删除: `.github/workflows/commit.yml`
- ✅ 新增: `.github/workflows/ci.yml`
- ✅ 重构: `.github/workflows/release.yml`
- ✅ 新增: `.github/dependabot.yml`

### 2. CI Workflow (ci.yml)

#### 改进点:

**触发条件优化**
- 🔧 从"所有 push"改为"主分支 push + PR"
- 🔧 添加路径忽略规则 (markdown、文档等)
- ✅ 支持手动触发 (`workflow_dispatch`)

**并发控制**
- ✅ 同一分支/PR 只保留最新运行
- ✅ 自动取消过时的构建

**权限最小化**
- ✅ 显式声明 `contents: read` 权限

**构建流程优化**
- ✅ 代码检查前置 (快速失败原则)
- ✅ 在 Ubuntu 上统一执行测试 (最快)
- ✅ 使用 matrix 策略管理多平台构建
- ✅ 减少重复代码

**依赖缓存**
- ✅ 启用 .NET 依赖缓存
- ✅ 基于 `packages.lock.json` 的智能缓存

**超时控制**
- ✅ 每个 job 都设置合理的超时时间
- ✅ 防止意外的长时间运行

**构建产物管理**
- ✅ Artifact 名称包含架构信息,避免冲突
- ✅ 保留期限设为 7 天 (节省存储)

### 3. Release Workflow (release.yml)

#### 改进点:

**权限优化**
- ✅ 显式声明 `contents: write` 权限

**构建流程**
- ✅ 使用 matrix 策略统一管理多平台打包
- ✅ 分离 changelog 生成为独立 job
- ✅ 并行执行所有平台打包

**文件组织**
- ✅ 统一的 artifact 命名规范
- ✅ 自动识别所有平台的构建产物

**发布策略**
- ✅ 自动识别预发布版本 (alpha/beta/rc)
- ✅ 保留 NuGet 发布的扩展点 (已注释)

### 4. Dependabot 配置

#### 新增功能:

**GitHub Actions 依赖更新**
- ✅ 每周一自动检查 actions 版本更新
- ✅ 自动创建更新 PR

**NuGet 包更新**
- ✅ 每周一自动检查包更新
- ✅ 忽略主版本更新 (避免破坏性变更)
- ✅ 自动标记和分配审核人

## 性能对比

### 构建时间预估

| 场景 | 旧配置 | 新配置 | 改进 |
|------|-------|--------|------|
| CI (代码检查) | N/A | ~5 分钟 | 新增快速失败 |
| CI (完整构建) | ~45 分钟 | ~30 分钟 | -33% |
| Release (发布) | ~35 分钟 | ~25 分钟 | -29% |

### 改进原因:

1. **并行构建**: matrix 策略并行执行所有平台
2. **依赖缓存**: 避免重复下载 NuGet 包
3. **快速失败**: 代码检查前置,提前发现问题
4. **减少重复**: 测试只在 Ubuntu 上执行一次

## Matrix 策略说明

### Windows 构建
```yaml
matrix:
  arch: [x64, x86]
```
- 并行构建两个架构
- 输出统一命名的 artifacts

### Linux 构建
```yaml
matrix:
  arch: [x64, arm64]
```
- 并行构建两个架构
- 仅构建 CLI 版本

### macOS 构建
```yaml
matrix:
  arch: [x64, arm64]
```
- 并行构建两个架构
- 包含 CLI 和 GUI (.app bundle)

## 环境变量优化

新增全局环境变量:
```yaml
env:
  DOTNET_VERSION: '10.0.x'
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: 1
  DOTNET_NOLOGO: true
  DOTNET_CLI_TELEMETRY_OPTOUT: 1
```

作用:
- 统一 .NET 版本管理
- 禁用遥测,加快构建速度
- 减少日志噪音

## 安全性改进

### 权限最小化原则
- CI workflow: `contents: read` (只读)
- Release workflow: `contents: write` (发布需要)

### Secret 保护
- 预留 NuGet API Key 扩展点
- 使用 GitHub 内置的 `GITHUB_TOKEN`

## 迁移检查清单

在合并这次重构前,请确认:

- [ ] 验证 CI workflow 在主分支正常运行
- [ ] 验证 PR 触发 CI workflow
- [ ] 创建测试 tag 验证 release workflow
- [ ] 确认 dependabot 配置被识别
- [ ] 检查 artifact 下载和使用是否正常
- [ ] 更新项目文档引用旧 workflow 名称的地方

## 兼容性说明

### 不兼容变更

1. **Artifact 名称变更**
   - 旧: `imewlconverter-cli`, `imewlconverter-macos`
   - 新: `windows-x64`, `macos-arm64` 等

2. **触发条件变更**
   - CI 不再对所有 push 触发
   - 需要推送到主分支或创建 PR

### 向后兼容

- Makefile 命令保持不变
- 发布产物命名保持不变
- Release 产物结构保持不变

## 后续优化建议

1. **添加代码覆盖率报告**
   - 集成 Codecov 或 Coveralls
   - 在 PR 中显示覆盖率变化

2. **性能基准测试**
   - 添加性能测试 job
   - 对比不同版本的性能

3. **多版本 .NET 测试**
   - 支持在多个 .NET 版本上测试
   - 确保向后兼容性

4. **发布到更多平台**
   - NuGet 包发布
   - Homebrew formula 更新
   - Chocolatey 包发布

## 故障排查

### 如果 CI 失败

1. 检查 workflow 文件语法:
   ```bash
   python3 -c "import yaml; yaml.safe_load(open('.github/workflows/ci.yml'))"
   ```

2. 检查 Makefile 命令:
   ```bash
   make restore
   make build-all DOTNET_CONFIG=Release
   make test DOTNET_CONFIG=Release
   ```

3. 检查 .NET 版本:
   ```bash
   dotnet --version
   ```

### 如果 Release 失败

1. 验证 tag 格式是否为 `v*.*.*`
2. 检查 Makefile 中的 app bundle 创建脚本
3. 验证所有平台的打包命令

## 参考资料

- [GitHub Actions 最佳实践](https://docs.github.com/en/actions/learn-github-actions/best-practices-for-using-github-actions)
- [GitHub Actions Matrix 策略](https://docs.github.com/en/actions/using-jobs/using-a-matrix-for-your-jobs)
- [Dependabot 配置](https://docs.github.com/en/code-security/dependabot/dependabot-version-updates/configuration-options-for-the-dependabot.yml-file)
- [.NET GitHub Actions](https://docs.microsoft.com/en-us/dotnet/devops/github-actions-overview)

## 联系方式

如有问题或建议,请创建 Issue 或 PR。
