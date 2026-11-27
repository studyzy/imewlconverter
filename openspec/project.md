# 项目上下文

## 目的

深蓝词库转换（IME WL Converter）是一款跨平台的输入法词库转换工具，旨在帮助用户在不同输入法之间轻松转换词库格式。该项目注册了 58 种词库格式代码（56 种可导入、49 种可导出），包括搜狗拼音、QQ拼音、Rime、微软拼音、谷歌拼音等，覆盖拼音、五笔、郑码、仓颉、注音等多种编码方式。

核心功能：
- 批量转换多个词库文件
- 命令行模式支持自动化处理
- 跨平台支持（Windows、Linux、macOS）
- 支持 PC 端和移动端输入法格式
- 提供 GUI 界面和命令行工具

## 技术栈

### 核心技术
- **.NET 10.0** - 主要开发框架
- **C#** - 主要编程语言

### 项目结构
- **ImeWlConverter.Abstractions** - 接口与 DTO 层（零依赖）
- **ImeWlConverter.CodeData** - 码表数据叶子项目（零依赖，嵌入式码表与只读表服务）
- **ImeWlConverter.Core** - 核心库，包含转换管道、编码生成、过滤、简繁转换
- **ImeWlConverter.Formats** - 格式实现层（105 个格式插件类，58 种格式代码）
- **ImeWlConverter.Application** - 三端共享应用层（参数映射、请求工厂、CLI 前端、格式识别、引导）
- **ImeWlConverter.SourceGenerators** - 编译时 Source Generator（格式注册与 Metadata 生成）
- **ImeWlConverterCmd** - 命令行薄入口（逻辑在 Application/Cli）
- **IME WL Converter Win** - Windows GUI 应用（WinForms）
- **ImeWlConverterMac** - macOS GUI 应用（Avalonia 11.2.3）
- **ImeWlConverterCoreTest** - xUnit 单元测试项目

### 主要依赖包
- **SharpZipLib (1.4.2)** - 压缩文件处理
- **UTF.Unknown (2.5.1)** - 字符编码自动检测
- **System.Text.Encoding.CodePages** - .NET 10 内置（非 NuGet 包），通过 `Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)` 启用扩展编码
- **xUnit (2.9.3)** - 单元测试框架
- **Avalonia (11.2.3)** - macOS GUI 框架
- **MinVer (6.0.0)** - 从 Git tag 生成版本号（仅构建时依赖）

### 构建和部署
- **GitHub Actions** - CI/CD 自动化
- **跨平台发布** - 支持 Windows (x86/x64)、Linux (x64/arm64)、macOS (x64/arm64)
- **单文件发布** - 使用 PublishSingleFile 简化分发

## 项目约定

### 代码风格
- 使用 `.editorconfig` 统一代码格式
- 禁用特定诊断规则：
  - CA1416：平台兼容性验证（已禁用）
  - CS0067：未使用事件警告（已禁用）
- 采用标准 C# 命名约定和代码组织方式

### 架构模式
- **分层与依赖方向** - Abstractions（接口/DTO）← CodeData（码表）← Core（业务服务）← Formats（格式实现）← Application（三端共享应用层）← 三端 UI（CLI / WinForms / macOS）
- **三端统一管道** - CLI、WinForms GUI、macOS GUI 共用 `ConversionPipeline`，仅在用户交互层不同
- **编译期格式注册** - 格式类用 `[FormatPlugin]` 标记，Source Generator 生成 DI 注册（`AddAllFormats()`）与 `Metadata` 属性；禁止运行时反射注册
- **模块化设计** - 主要目录：
  - `Core/Pipeline/` - 转换管道与词条阶段处理
  - `Core/Filters/` - 过滤器
  - `Core/CodeGeneration/Generators/` - 编码生成器
  - `Core/Language/` - 简繁转换
  - `Formats/{Format}/` - 各输入格式的 Importer / Exporter
  - `CodeData/Resources/` - 嵌入式码表与简繁映射文件
- **可扩展架构** - 添加格式只需新增带 `[FormatPlugin]` 的 Importer / Exporter

### 测试策略
- 使用 **xUnit 2.9.3** 作为单元测试框架
- 测试项目：`ImeWlConverterCoreTest`（581 个用例：578 通过、3 跳过）
- 集成测试：`tests/integration/`（4 个套件共 29 个用例，另有 Windows GUI 矩阵脚本 43 个用例）
- 测试并行执行（`xunit.runner.json` 中 `parallelizeTestCollections: true`）
- CI 流程中自动运行单元测试与集成测试（Linux x64 + macOS arm64）

### Git 工作流
- 主分支保护，所有更改通过 Pull Request
- **提交约定**：使用中文提交信息，清晰描述变更内容
- **CI 触发**：任意分支 push 触发构建与测试（纯文档变更 `**.md`、`docs/**`、`.gitignore` 通过 paths-ignore 跳过），另支持 PR 与 workflow_dispatch
- **发布流程**：通过 GitHub Actions 自动构建多平台发布包
- **工件管理**：CLI 工具和 GUI 应用分别上传为构建工件

### 版本号管理
- **自动化生成**：版本号从 Git tag 自动生成，使用 MinVer 工具
- **版本号格式**：
  - Git tag: `vX.Y.Z` (如 `v3.4.0`)
  - 程序版本: `X.Y.Z.0` (如 `3.4.0.0`)
  - 开发版本: `X.Y.Z-dev.N` (如 `3.3.1-dev.1`)
- **发布新版本**：创建并推送 Git tag 即可触发自动发布
- **配置文件**：`src/Directory.Build.props` 包含 MinVer 全局配置
- **禁止手动修改**：不要在代码中硬编码版本号
- 详见 `docs/RELEASING.md` 了解完整发布流程

## 领域上下文

### 输入法词库知识
- **词库格式多样性** - 不同输入法使用专有的二进制或文本格式存储词库
- **编码方案** - 项目需要理解多种中文输入编码：
  - 拼音（全拼、双拼）
  - 五笔（86、98、新世纪）
  - 郑码
  - 仓颉
  - 二笔
  - 注音
- **词频处理** - 词库通常包含词频信息，需要在转换时保留或转换
- **字符编码** - 处理 GBK、GB2312、UTF-8 等多种字符编码

### 常见词库格式
- **文本格式** - 如搜狗文本词库、Rime YAML 格式
- **二进制格式** - 如搜狗 scel、百度 bdict、QQ qpyd 等
- **系统格式** - 如 Windows 注册表格式、macOS plist 格式

## 重要约束

### 技术约束
- 必须保持跨平台兼容性（Windows、Linux、macOS）
- 核心库不应依赖特定平台的 UI 框架
- 需要处理大文件（词库可能包含数十万条目）
- 内存效率：批量处理时避免内存溢出

### 兼容性约束
- 仅支持 .NET 10.0（`net10.0`；Windows GUI 为 `net10.0-windows`，SourceGenerators 为 `netstandard2.0`），不再兼容 .NET Framework 4.6
- 支持旧版输入法格式的读取
- 自 v3.4.0 起 CLI 参数改为 GNU 风格（BREAKING CHANGE，旧 `-i:` 冒号格式已移除）；迁移见 `docs/MIGRATION.md`

### 业务约束
- 必须准确转换词条，不能丢失或错误转换数据
- 保持词频信息的完整性
- 尊重各输入法的版权和使用条款

## 外部依赖

### 第三方库
- **SharpZipLib** - 处理压缩的词库文件（如 .zip、.rar）
- **UTF.Unknown** - 自动检测文件编码，处理各种字符集
- **MinVer** - 从 Git tag 自动生成版本号（仅构建时依赖）

### 资源文件
- 内置编码映射表（作为嵌入式资源，位于 `src/ImeWlConverter.CodeData/Resources/`）：
  - `Cangjie5.txt` - 仓颉五代编码表
  - `Zhengma.txt` - 郑码编码表
  - `Erbi.txt` - 二笔编码表
  - `Shuangpin.txt` - 双拼方案
  - `Zhuyin.txt` - 注音符号映射
  - `WordPinyin.txt` - 汉字拼音对照
  - `ChineseCode.txt` - 汉字编码表
  - `ChaoyinCodeMapping.txt` - 超音码映射

### 外部系统
- **GitHub** - 代码托管和协作
- **GitHub Actions** - CI/CD 自动化构建
- **GitHub Releases** - 分发发布版本
- 默认完全离线运行；可选 `-r llm` 词频生成会调用外部 LLM API（默认 `https://api.openai.com/v1/chat/completions`，需 `--llm-key` 或环境变量 `IMEWL_LLM_KEY`）

### 平台特定依赖
- **Windows** - WinForms UI 框架
- **macOS** - Avalonia 11.2.3（跨平台 UI 框架）
- **Linux** - 仅命令行，无 GUI 依赖
