# 变更：自动化版本号生成机制

## 为什么

当前项目的版本号管理存在以下问题：
1. **手动维护易出错**：版本号分散在多个文件中（ConstantString.cs、多个 .csproj 文件），每次发布需要手动更新所有位置，容易遗漏或不一致
2. **Git tag 与二进制版本脱节**：虽然 Git 仓库中打了 v3.3.0 标签，但代码中版本号仍为 3.2.0，导致用户下载的二进制文件显示错误的版本号
3. **缺乏单一真相来源**：版本号缺乏统一的权威来源，维护成本高

目标是建立一个自动化机制，使得：
- Git tag（如 v3.4.0）成为版本号的唯一真相来源
- 构建时自动从 Git tag 提取版本号并注入到所有二进制文件中
- 开发者只需打 tag，无需手动修改代码中的版本号

## 变更内容

1. **移除硬编码版本号**
   - 从 `ConstantString.cs` 中移除 `VERSION` 常量的硬编码值
   - 从所有 `.csproj` 文件中移除硬编码的 `<Version>` 标签

2. **实现版本号自动注入机制**
   - 在构建时从 Git tag 读取版本号（格式：`v3.4.0` → `3.4.0.0`）
   - 通过 MSBuild 属性或编译时常量将版本号注入到程序集中
   - 确保所有平台（Windows、Linux、macOS）的二进制文件都使用相同的版本号

3. **更新 CI/CD 工作流**
   - 修改 `.github/workflows/release.yml`，在构建前提取 tag 版本号
   - 修改 `.github/workflows/commit.yml`，在非 tag 构建时使用开发版本号（如 `0.0.0-dev`）
   - 确保版本号正确传递到所有构建步骤

4. **提供本地构建支持**
   - 更新 `Makefile`，支持从 Git tag 自动获取版本号
   - 当没有 tag 时，使用合理的默认值（如 `0.0.0-local`）

5. **更新文档**
   - 在 `README.md` 或构建文档中说明新的版本管理机制
   - 提供发布新版本的操作指南（创建 tag 和推送）

## 影响

### 受影响规范
- **build-system**（新增）：构建系统和版本管理规范

### 受影响代码
- `src/ImeWlConverterCore/ConstantString.cs` - VERSION 常量改为从编译时注入
- `src/ImeWlConverterCore/ImeWlConverterCore.csproj` - 版本号配置
- `src/ImeWlConverterCore/ImeWlConverterCore-net46.csproj` - 版本号配置
- `src/ImeWlConverterCmd/ImeWlConverterCmd.csproj` - 版本号配置
- `src/ImeWlConverterMac/ImeWlConverterMac.csproj` - 版本号配置
- `src/IME WL Converter Win/Properties/AssemblyInfo.cs` - 版本号引用
- `.github/workflows/release.yml` - Release 构建流程
- `.github/workflows/commit.yml` - CI 构建流程
- `Makefile` - 本地构建配置

### 破坏性变更
- 无。从用户角度看，版本号的显示方式保持不变，只是管理机制改变

### 风险评估
- **中等风险**：涉及构建系统的重构，需要充分测试各平台的构建流程
- **缓解措施**：
  - 在测试分支上完整验证所有平台的构建
  - 保留旧版本号作为后备方案（通过注释）
  - 确保 CI/CD 管道在所有场景下都能正常工作

## 预期效果

实施后：
1. 开发者发布新版本只需执行：
   ```bash
   git tag v3.4.0
   git push origin v3.4.0
   ```
2. GitHub Actions 自动构建并生成所有平台的二进制文件，版本号统一为 3.4.0
3. 不再出现"tag 是 v3.3.0 但程序显示 v3.2.0"的问题
4. 版本号管理更加清晰、可靠、易维护
