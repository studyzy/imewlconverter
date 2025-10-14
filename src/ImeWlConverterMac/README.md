# IME WL Converter - macOS版本

这是深蓝词库转换工具的macOS版本，使用Avalonia UI框架构建，提供跨平台的现代化用户界面。

## 功能特性

- 🔄 支持多种输入法词库格式的相互转换
- 📁 支持拖拽文件操作
- 🎛️ 丰富的过滤和转换选项
- 📊 实时转换进度显示
- 🌐 简繁体转换支持
- 🔧 词频生成和词条过滤功能

## 系统要求

- macOS 10.15 (Catalina) 或更高版本
- .NET 6.0 或更高版本

## 安装和运行

### 方法1：直接运行
```bash
cd src/ImeWlConverterMac
dotnet restore
dotnet run
```

### 方法2：构建发布版本
```bash
cd src/ImeWlConverterMac
dotnet publish -c Release -r osx-x64 --self-contained
```

发布的应用程序将位于 `bin/Release/net6.0/osx-x64/publish/` 目录中。

### 方法3：创建macOS应用包
```bash
cd src/ImeWlConverterMac
dotnet publish -c Release -r osx-x64 --self-contained
# 然后可以创建.app包或使用打包工具
```

## 使用方法

1. **选择源词库文件**：点击"..."按钮选择文件，或直接拖拽文件到文本框
2. **选择转换格式**：从下拉菜单中选择源格式和目标格式
3. **配置选项**：通过菜单栏的"高级设置"配置过滤和转换选项
4. **开始转换**：点击"转换"按钮开始处理
5. **查看结果**：转换结果将显示在下方的文本区域

## 支持的词库格式

### 输入格式
- 搜狗细胞词库(.scel)
- QQ分类词库(.qpyd)
- 百度分类词库(.bdict, .bcd)
- 搜狗备份词库(.bin)
- 紫光分类词库(.uwl)
- 微软拼音词库(.dat)
- Gboard词库(.zip)
- 灵格斯词库(.ld2)
- 文本文件(.txt)
- 自定义格式

### 输出格式
- 搜狗细胞词库(.scel)
- 文本文件(.txt)
- 微软拼音(.dctx)
- 各种输入法专用格式
- 自定义格式

## 高级功能

### 词条过滤
- 按词长度过滤
- 按词频过滤
- 忽略英文、数字、标点符号
- 自定义过滤规则

### 简繁体转换
- 支持简体转繁体
- 支持繁体转简体
- 多种转换引擎可选

### 批处理
- 支持多文件批量转换
- 支持文件夹批量处理
- 可合并多个词库到单一文件

## 项目结构

```
src/ImeWlConverterMac/
├── App.axaml                 # 应用程序主配置
├── App.axaml.cs             # 应用程序启动逻辑
├── Program.cs               # 程序入口点
├── Views/
│   ├── MainWindow.axaml     # 主窗口界面定义
│   └── MainWindow.axaml.cs  # 主窗口代码逻辑
├── ViewModels/
│   ├── ViewModelBase.cs     # ViewModel基类
│   └── MainWindowViewModel.cs # 主窗口ViewModel
└── ImeWlConverterMac.csproj # 项目配置文件
```

## 开发说明

### 技术栈
- **UI框架**: Avalonia UI 11.0
- **架构模式**: MVVM (Model-View-ViewModel)
- **目标框架**: .NET 6.0
- **核心库**: ImeWlConverterCore

### 添加新功能
1. 在ViewModel中添加相应的属性和命令
2. 在AXAML文件中绑定UI元素
3. 实现业务逻辑，调用Core库的功能

### 调试
```bash
dotnet run --configuration Debug
```

## 故障排除

### 常见问题

**Q: 应用无法启动**
A: 确保已安装.NET 6.0运行时，并检查依赖项是否正确

**Q: 转换失败**
A: 检查源文件格式是否正确，查看错误日志获取详细信息

**Q: 界面显示异常**
A: 尝试更新到最新版本的Avalonia UI

### 日志和调试
应用程序会在控制台输出详细的转换过程信息，如遇问题请查看相关日志。

## 贡献

欢迎提交Issue和Pull Request来改进这个项目。

## 许可证

本项目基于GNU General Public License v3.0许可证开源。

## 相关链接

- [原项目GitHub](https://github.com/studyzy/imewlconverter)
- [Avalonia UI文档](https://docs.avaloniaui.net/)
- [.NET 6.0文档](https://docs.microsoft.com/en-us/dotnet/core/)