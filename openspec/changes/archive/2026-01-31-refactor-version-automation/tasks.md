# 实施任务清单

## 1. 准备和调研
- [x] 1.1 研究 .NET MSBuild 版本号注入最佳实践（MinVer、GitVersion 等工具）
- [x] 1.2 确定技术方案：使用 MinVer NuGet 包
- [x] 1.3 设计版本号格式规范（tag 格式、语义化版本、构建元数据）

## 2. 核心机制实现
- [x] 2.1 在 `Directory.Build.props` 中配置全局版本号生成逻辑
- [x] 2.2 修改 `ConstantString.cs`，将 VERSION 改为从程序集版本属性读取
- [x] 2.3 从所有 `.csproj` 文件中移除硬编码的 `<Version>` 标签
- [x] 2.4 确保 Windows GUI 的 `AssemblyInfo.cs` 正确使用自动生成的版本号

## 3. 本地构建支持
- [x] 3.1 更新 `Makefile`，添加 `version` 目标显示当前版本号
- [x] 3.2 为本地开发环境提供版本号后备机制（MinVer 自动处理）
- [x] 3.3 测试本地构建：验证在有 tag 和无 tag 两种情况下的行为

## 4. CI/CD 集成
- [x] 4.1 修改 `.github/workflows/release.yml`
  - [x] 4.1.1 添加 `fetch-depth: 0` 获取完整 Git history（MinVer 自动提取版本号）
  - [x] 4.1.2 验证 tag 格式（MinVer 自动验证）
  - [x] 4.1.3 将版本号传递给构建命令（MinVer 自动注入）
- [x] 4.2 修改 `.github/workflows/commit.yml`
  - [x] 4.2.1 添加 `fetch-depth: 0` 确保开发版本号正确生成
  - [x] 4.2.2 确保 CI 构建不因缺少 tag 而失败（MinVer 会生成 dev 版本号）
- [ ] 4.3 测试 GitHub Actions 工作流（需要推送到远程仓库后验证）
  - [ ] 4.3.1 测试 release.yml：推送 tag 触发构建
  - [ ] 4.3.2 测试 commit.yml：普通提交触发构建

## 5. 多平台验证（需要 CI/CD 构建后验证）
- [ ] 5.1 验证 Windows x86 构建的版本号
- [ ] 5.2 验证 Windows x64 构建的版本号
- [ ] 5.3 验证 Linux x64 构建的版本号
- [ ] 5.4 验证 Linux ARM64 构建的版本号
- [ ] 5.5 验证 macOS x64 构建的版本号
- [ ] 5.6 验证 macOS ARM64 构建的版本号
- [ ] 5.7 验证命令行工具 `--version` 参数输出
- [ ] 5.8 验证 GUI 程序"关于"对话框显示的版本号

## 6. 文档更新
- [x] 6.1 在 `CODEBUDDY.md` 和 `openspec/project.md` 中记录版本管理约定
- [x] 6.2 创建 `RELEASING.md`，说明发布新版本的流程
- [x] 6.3 项目无 `CONTRIBUTING.md` 文件，已在 CODEBUDDY.md 中添加说明

## 7. 测试和验证
- [x] 7.1 本地测试构建，验证 MinVer 工作正常（生成 3.3.1-dev.1）
- [ ] 7.2 创建测试 tag，触发完整 CI 构建（待推送后验证）
- [ ] 7.3 下载构建产物，验证所有二进制文件的版本号（待 CI 构建后验证）
- [x] 7.4 验证回退机制：MinVer 在无 tag 时使用默认版本（已验证）

## 8. 清理和完善
- [x] 8.1 已从 .csproj 文件移除硬编码版本号，添加注释说明
- [x] 8.2 确保所有项目文件格式一致
- [x] 8.3 核心库构建成功，版本号机制正常工作
- [x] 8.4 审查代码变更，确保没有遗漏的版本号引用

## 9. 发布和监控（待用户执行）
- [ ] 9.1 合并变更到主分支
- [ ] 9.2 创建正式 tag（如 `v3.4.0`），触发 Release 构建
- [ ] 9.3 验证 GitHub Release 页面的产物和版本号
- [ ] 9.4 在多个平台上测试下载的二进制文件
- [ ] 9.5 监控用户反馈，确认版本号显示正确

## 实施总结

### 已完成
✅ **核心实施已完成**，包括：
1. 创建 `src/Directory.Build.props` 配置 MinVer
2. 修改 `ConstantString.cs` 从程序集属性动态读取版本号
3. 从所有 `.csproj` 文件移除硬编码版本号
4. 更新 GitHub Actions workflows 确保获取完整 Git history
5. 更新 Makefile 添加 `version` 目标
6. 创建完整的发布流程文档 `RELEASING.md`
7. 更新项目约定文档
8. 本地构建测试成功，MinVer 正常工作（生成版本 3.3.1-dev.1）

### 待验证
⏳ **以下任务需要推送到远程仓库后验证**：
- CI/CD workflows 的实际运行
- 所有平台二进制文件的版本号
- 创建 tag 触发的 Release 构建

### 下一步
1. 提交所有变更到 Git
2. 推送到远程仓库，观察 CI 构建
3. 创建测试 tag 验证 Release 流程
4. 确认无误后创建正式 tag 发布新版本

## 依赖关系说明
- 任务 2（核心机制）必须在任务 4（CI/CD）之前完成 ✅
- 任务 3（本地构建）可以与任务 4 并行进行 ✅
- 任务 5（多平台验证）依赖任务 2 和任务 4 ✅
- 任务 7（测试）必须在所有实施任务完成后进行 ⏳
- 任务 9（发布）必须在任务 7 验证通过后进行 ⏳

## 回滚计划
如果自动化版本号机制出现问题：
1. 恢复 `ConstantString.cs` 中的硬编码版本号
2. 恢复所有 `.csproj` 文件中的 `<Version>` 标签
3. 回滚 CI/CD 工作流文件到之前的版本
4. 移除 `src/Directory.Build.props` 中的 MinVer 配置
5. 重新构建和发布
