
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
