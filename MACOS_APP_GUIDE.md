# macOS 应用包创建指南

## 概述

现在您可以将 IME WL Converter 打包成标准的 macOS .app 格式，就像其他 Mac 应用一样！

## 🚀 快速开始

### 创建 .app 包

```bash
# 创建 ARM64 版本的 .app 包（推荐，适用于 M1/M2/M3 Mac）
make app-mac

# 或者创建 x64 版本的 .app 包（适用于 Intel Mac）
make app-mac-x64

# 或者创建 ARM64 版本（与 app-mac 相同）
make app-mac-arm64
```

### 运行应用

```bash
# 方法 1: 双击运行
# 在 Finder 中双击 "IME WL Converter.app"

# 方法 2: 命令行运行
open "IME WL Converter.app"

# 方法 3: 安装到应用程序文件夹
cp -r "IME WL Converter.app" /Applications/
```

## 📁 文件结构

创建的 .app 包具有标准的 macOS 应用结构：

```
IME WL Converter.app/
├── Contents/
│   ├── Info.plist          # 应用信息配置
│   ├── PkgInfo            # 包类型信息
│   ├── MacOS/             # 可执行文件和依赖
│   │   ├── ImeWlConverterMac  # 主执行文件
│   │   ├── *.dll          # .NET 运行时和依赖
│   │   └── ...
│   └── Resources/         # 资源文件
│       └── AppIcon.icns   # 应用图标
```

## 🛠️ 技术实现

### 项目配置更新

- **ImeWlConverterMac.csproj**: 添加了 macOS 应用包配置
- **Info.plist**: 标准的 macOS 应用信息文件
- **AppIcon.icns**: 自动生成的应用图标

### 构建脚本

- **scripts/create-app-bundle.sh**: 创建 .app 包的核心脚本
- **scripts/create-icon.sh**: 自动生成应用图标
- **Makefile**: 新增的 make 目标

### 新增的 Make 目标

| 命令 | 描述 |
|------|------|
| `make app-mac` | 创建 ARM64 版本的 .app 包（推荐） |
| `make app-mac-arm64` | 创建 ARM64 版本的 .app 包 |
| `make app-mac-x64` | 创建 x64 版本的 .app 包 |
| `make publish-mac` | 仅发布，不创建 .app 包 |

## 🎯 优势

### 之前的问题
- 执行 `make publish-mac` 会生成很多散乱的 dll 文件
- 用户需要找到正确的可执行文件来运行
- 不符合 macOS 应用的标准格式

### 现在的解决方案
- ✅ 生成标准的 .app 包格式
- ✅ 双击即可运行，就像其他 Mac 应用
- ✅ 可以拖拽到应用程序文件夹安装
- ✅ 包含应用图标和元数据
- ✅ 所有依赖都打包在内，无需额外安装

## 📦 分发

### 本地使用
```bash
# 创建应用包
make app-mac

# 运行应用
open "IME WL Converter.app"
```

### 分发给其他用户
```bash
# 创建压缩包
zip -r "IME-WL-Converter-macOS.zip" "IME WL Converter.app"

# 或者创建 DMG 文件（需要额外工具）
# hdiutil create -volname "IME WL Converter" -srcfolder "IME WL Converter.app" -ov -format UDZO "IME-WL-Converter.dmg"
```

## 🔧 自定义

### 修改应用图标
1. 替换 `src/ImeWlConverterMac/AppIcon.icns` 文件
2. 或者修改 `scripts/create-icon.sh` 脚本来生成自定义图标

### 修改应用信息
编辑 `src/ImeWlConverterMac/Info.plist` 文件：
- `CFBundleName`: 应用名称
- `CFBundleIdentifier`: 应用标识符
- `CFBundleVersion`: 版本号
- 等等...

### 修改应用包名称
修改 `Makefile` 中的应用名称：
```makefile
./scripts/create-app-bundle.sh ./publish/mac-arm64 "您的应用名称"
```

## 🐛 故障排除

### 应用无法启动
1. 检查可执行文件权限：
   ```bash
   chmod +x "IME WL Converter.app/Contents/MacOS/ImeWlConverterMac"
   ```

2. 查看系统日志：
   ```bash
   log show --predicate 'process == "ImeWlConverterMac"' --last 1m
   ```

### 重新构建
```bash
# 清理并重新构建
make clean-mac
make app-mac
```

## 📝 注意事项

1. **架构兼容性**: ARM64 版本适用于 M1/M2/M3 Mac，x64 版本适用于 Intel Mac
2. **代码签名**: 当前版本未包含代码签名，分发时可能需要用户允许运行
3. **依赖**: 应用包含所有必要的 .NET 运行时，无需用户单独安装

## 🎉 总结

现在您可以轻松地将 IME WL Converter 打包成专业的 macOS 应用！用户可以像使用其他 Mac 应用一样双击运行，或者将其安装到应用程序文件夹中。

使用 `make app-mac` 命令，一键生成标准的 .app 包格式！