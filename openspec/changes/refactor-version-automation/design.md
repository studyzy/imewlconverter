# 设计文档：自动化版本号生成机制

## 上下文

### 当前状态
项目使用多处硬编码的版本号：
- `src/ImeWlConverterCore/ConstantString.cs:25` - `VERSION = "3.2.0.0"`
- 多个 `.csproj` 文件中的 `<Version>` 标签
- 版本号需要在每次发布时手动同步更新

### 问题
1. 最近的 v3.3.0 发布时，代码中的版本号未更新，导致二进制文件显示 v3.2.0
2. 版本号分散在多个文件中，容易遗漏或不一致
3. Git tag 与代码版本号脱节，增加维护负担

### 约束条件
- 必须支持所有现有平台：Windows (x86/x64)、Linux (x64/ARM64)、macOS (x64/ARM64)
- 不能破坏现有的构建流程和 CI/CD 管道
- 版本号格式必须符合 .NET 程序集版本规范（X.Y.Z.W）
- 需要支持语义化版本（Semantic Versioning）

## 目标 / 非目标

### 目标
1. **单一真相来源**：Git tag 作为版本号的唯一权威来源
2. **自动化注入**：构建时自动从 Git 提取版本号并注入到二进制文件
3. **多平台支持**：所有平台的构建都使用相同的版本号机制
4. **开发友好**：本地开发环境能够正常构建，即使没有 Git tag
5. **向后兼容**：不影响现有的版本号显示方式

### 非目标
1. 不改变版本号的语义（仍然使用 `vX.Y.Z` 格式）
2. 不引入复杂的版本号计算逻辑（如自动递增构建号）
3. 不修改程序的版本显示 UI（仅改变版本号来源）

## 决策

### 决策 1：技术方案选择

**选定方案：使用 MinVer NuGet 包**

MinVer 是一个轻量级的 NuGet 包，能够：
- 自动从 Git tag 读取版本号
- 遵循语义化版本规范
- 无需配置即可工作（约定优于配置）
- 支持多种版本号格式
- 在 CI/CD 环境和本地开发环境都能正常工作

**配置方式**：
```xml
<!-- 在 Directory.Build.props 中全局配置 -->
<ItemGroup>
  <PackageReference Include="MinVer" Version="5.0.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
</ItemGroup>

<PropertyGroup>
  <!-- MinVer 配置 -->
  <MinVerTagPrefix>v</MinVerTagPrefix>
  <MinVerDefaultPreReleaseIdentifiers>dev</MinVerDefaultPreReleaseIdentifiers>
  <MinVerVerbosity>minimal</MinVerVerbosity>
</PropertyGroup>
```

### 考虑的替代方案

#### 方案 A：使用 GitVersion
- **优点**：功能强大，支持复杂的分支策略和版本计算
- **缺点**：配置复杂，学习曲线陡峭，对本项目来说过于重量级
- **结论**：不采用，MinVer 已足够满足需求

#### 方案 B：自定义 MSBuild Target
- **优点**：完全控制，无外部依赖
- **缺点**：需要自己实现 Git tag 解析、跨平台兼容性、错误处理等逻辑
- **结论**：不采用，重复造轮子，维护成本高

#### 方案 C：在 CI/CD 脚本中手动注入
- **优点**：实现简单，逻辑集中在 workflow 文件中
- **缺点**：本地构建无法获取版本号，不同 workflow 需要重复逻辑
- **结论**：不采用，不符合"单一真相来源"目标

### 决策 2：版本号格式

**格式规范**：
- **Git tag 格式**：`vX.Y.Z`（如 `v3.4.0`）
- **程序集版本**：`X.Y.Z.0`（如 `3.4.0.0`）
- **开发构建版本**：`0.0.0-dev.{commits}.{commit-sha}`（如 `0.0.0-dev.42.abc1234`）
- **文件版本**：与程序集版本相同

**理由**：
- `v` 前缀是 Git tag 的常见约定
- `.0` 后缀保持与 .NET 版本号的兼容性
- 开发版本使用 `0.0.0-dev` 前缀，清晰标识非正式版本

### 决策 3：ConstantString.VERSION 的处理

**方案：从程序集属性读取**

修改 `ConstantString.cs`：
```csharp
public class ConstantString
{
    // 旧代码：public const string VERSION = "3.2.0.0";
    
    // 新代码：从程序集属性动态读取
    public static readonly string VERSION = 
        typeof(ConstantString).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion 
        ?? typeof(ConstantString).Assembly.GetName().Version?.ToString() 
        ?? "0.0.0.0";
    
    // 或者使用 AssemblyFileVersion
    public static readonly string VERSION = 
        typeof(ConstantString).Assembly
            .GetCustomAttribute<AssemblyFileVersionAttribute>()?
            .Version 
        ?? "0.0.0.0";
}
```

**理由**：
- 保持 `ConstantString.VERSION` 的使用方式不变，代码侵入性最小
- 运行时从程序集元数据读取版本号，与构建时注入的版本一致
- 提供后备机制（`?? "0.0.0.0"`），确保在异常情况下不会崩溃

### 决策 4：本地开发环境支持

**策略**：
1. MinVer 会自动检测 Git 仓库状态
2. 如果没有 tag，使用 `0.0.0-dev.{commits}.{sha}` 格式
3. 如果不在 Git 仓库中，使用 `0.0.0` 作为默认值

**Makefile 更新**：
```makefile
# 无需修改，MinVer 自动处理
# 可选：添加 version 目标用于显示当前版本号
version:
	@echo "Current version: $$(dotnet minver)"
```

## 实施计划

### 阶段 1：基础设施（1 天）
1. 创建 `Directory.Build.props` 文件（如果不存在）
2. 添加 MinVer 包引用
3. 配置 MinVer 参数

### 阶段 2：代码修改（1 天）
1. 修改 `ConstantString.cs`
2. 从所有 `.csproj` 移除 `<Version>` 标签
3. 验证本地构建

### 阶段 3：CI/CD 集成（1 天）
1. 更新 `release.yml`：
   - 确保 Git history 完整（fetch-depth: 0）
   - 验证 tag 格式
   - 构建和测试
2. 更新 `commit.yml`：
   - 确保 Git history 完整
   - 验证开发版本号格式

### 阶段 4：测试和验证（1 天）
1. 创建测试 tag（如 `v3.4.0-beta.1`）
2. 触发 Release 构建
3. 下载并验证所有平台的二进制文件
4. 测试本地构建（有 tag 和无 tag）

### 阶段 5：文档和发布（0.5 天）
1. 更新发布流程文档
2. 创建正式 tag（如 `v3.4.0`）
3. 监控构建和发布

**总计：约 4.5 天**

## 风险与缓解

### 风险 1：MinVer 在某些环境中无法获取 Git 信息
**影响**：中等 - 构建失败或版本号不正确  
**概率**：低  
**缓解措施**：
- 在 CI/CD 中使用 `actions/checkout@v4` 时设置 `fetch-depth: 0`，确保获取完整的 Git history 和 tags
- 设置后备版本号：在 `.csproj` 中添加 `<Version>0.0.0</Version>` 作为最后的后备

### 风险 2：现有代码中有硬编码的版本号字符串匹配逻辑
**影响**：中等 - 某些功能可能依赖特定的版本号格式  
**概率**：低  
**缓解措施**：
- 使用 `rg -n "3\.\d+\.\d+" src/` 搜索所有版本号相关的硬编码
- 确保版本号格式保持一致（`X.Y.Z.W`）

### 风险 3：macOS 应用包的 CFBundleVersion 更新
**影响**：低 - macOS 应用显示错误的版本号  
**概率**：中等  
**缓解措施**：
- 确认 MinVer 生成的版本号也会更新 `CFBundleVersion`
- 如果不自动更新，在 `.csproj` 中显式绑定：
  ```xml
  <CFBundleVersion>$(Version)</CFBundleVersion>
  ```

### 风险 4：版本号格式变化导致用户混淆
**影响**：低 - 用户可能不理解 `0.0.0-dev` 版本号  
**概率**：中等  
**缓解措施**：
- 在 README 或文档中说明版本号含义
- 确保正式 Release 的版本号格式不变（`X.Y.Z.0`）

## 迁移计划

### 向前迁移
1. **准备阶段**（可选）：在测试分支上验证
   - 创建 `feature/auto-version` 分支
   - 实施所有变更
   - 创建测试 tag（如 `v3.3.1-test`）
   - 验证构建和版本号

2. **正式迁移**：
   - 合并到主分支
   - 不立即创建新 tag，先观察 CI 构建
   - 验证开发版本号（`0.0.0-dev.X.Y`）正常工作

3. **首次正式发布**：
   - 创建 tag（如 `v3.4.0`）
   - 触发 Release 构建
   - 验证所有产物的版本号
   - 发布到 GitHub Releases

### 回滚计划
如果出现严重问题：
1. **立即回滚 CI/CD**：
   - 恢复 `.github/workflows/*.yml` 到之前的版本
   - 使用之前的构建流程创建紧急 Release

2. **后续修复**：
   - 恢复 `ConstantString.cs` 的硬编码版本号
   - 恢复 `.csproj` 文件的 `<Version>` 标签
   - 移除 `Directory.Build.props` 中的 MinVer 配置
   - 创建修复版本 tag（如 `v3.4.1`）

## 待决问题

1. **问题**：是否需要在版本号中包含构建元数据（如构建日期、commit SHA）？
   - **倾向**：对于正式 Release，不需要；对于开发构建，MinVer 自动包含
   - **需要确认**：产品负责人的偏好

2. **问题**：是否需要支持预发布版本（如 `v3.4.0-beta.1`）？
   - **倾向**：需要，MinVer 原生支持
   - **需要确认**：发布流程中是否有 beta/RC 阶段

3. **问题**：`ConstantString.VERSION` 改为 `static readonly` 后，是否有性能影响？
   - **评估**：影响极小，版本号读取不在热路径上
   - **需要确认**：是否有代码依赖 `const` 的编译时常量特性

## 参考资料

- [MinVer GitHub](https://github.com/adamralph/minver)
- [Semantic Versioning 2.0.0](https://semver.org/)
- [.NET Assembly Versioning](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning)
- [GitHub Actions: Checkout action](https://github.com/actions/checkout)
