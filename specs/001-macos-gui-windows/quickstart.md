# quickstart.md

**Feature**: macOS GUI 复刻 Windows 版

## 快速开始（开发者）

1. 切换到功能分支: `git checkout 001-macos-gui-windows`
2. 在 macOS 上安装 .NET 6/8 SDK。
3. 进入 Mac 项目目录: `cd src/ImeWlConverterMac`
4. 运行: `dotnet build` 然后 `dotnet run`（或使用 Idea/VS for Mac 打开解决方案并运行）。
5. 运行手动测试：启动应用，尝试拖拽词库文件、选择目标格式并转换。

## 快速开始（用户）

1. 下载 mac-x64 或 mac-arm64 发布包（`publish/` 目录）。
2. 解压并打开应用，首次运行可能需要 macOS 签名授权。
3. 使用界面拖拽词库文件或通过“导入”菜单选择文件，选择目标格式并点击“转换”。
