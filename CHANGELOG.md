# 更新日志 (Changelog)

本项目遵循 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/) 格式，
版本号遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

1.1 ~ 3.2 版的历史更新记录可在程序"帮助"窗口中查看（见
`src/IME WL Converter Win/Forms/HelpForm.cs`）。

## [未发布]

### 新增

- 新增 Fcitx5 / libime 二进制拼音词库格式 `libimebin`（导入/导出），
  生成的文件可被 Fcitx5 及 `libime_pinyindict` 直接加载

### 修复

- 修复 `libimetxt` 导入器把「汉字 拼音」两列读反、导致词面与拼音错位的问题，
  并支持词频可省略（两列）的 libime 文本行
- `libimetxt` 导出改为不带 BOM 的 UTF-8，与 libime `saveText` 输出一致
  （此前 BOM 会混入首条词条的词面）
- `libimetxt` 按 libime 的 fcitx 转义规则读写词面：词面含空白、引号或反斜杠时
  用引号包裹并转义（此前这类词面会被拆成多列或丢失字符）

## [3.5.0]

### 修复

- 修复百度 bdict 词库旧版头部解析导入 0 条的问题（官网下载的词库及部分
  旧版词库此前全部导入为空）

### 新增

- 新增 Windows GUI 自动化转换矩阵测试（`windows-gui-matrix.ps1`），
  驱动图形界面执行 35 条"导入 → 错位导出"转换路径并校验产物
- 新增 Win10 微软拼音实机端到端测试（替换系统 UDP 词库后断言输入法
  候选窗真实出词）
- 新增真实词库导入单元测试（`RealDictionaryImportTest`），覆盖搜狗备份、
  百度 bcd/bdict、百度备份、紫光 uwl、Gboard 文本词典、Rime UserDb、
  Win10 自学习/五笔 UDP、极点用户词等 9 种真实样本
- 新增截屏辅助调试工具（`lib/take-screenshot.ps1`）

### 变更

- `windows-ime-e2e.ps1` 重构为复用共享 UI 自动化库 `lib/gui-automation.ps1`
- 集成测试文档（TEST-MATRIX.md、README.md）更新至最新测试矩阵

## [3.4.3] - 2026-06-29

### 修复

- 修复〇 (U+3007) 误判为标点导致词条丢失
- 接入 CodeGenerationOptions 修复拼音编码生成选项失效 (#406)
- 修复英文标点过滤器正则
- 修复纯汉字词库导出为空的问题 (#400)
- 补充缺失的 Exporter 并修复 SinaPinyin 分隔符 bug
- 新增 Issue #403、#406、#408 回归测试

## [3.4.2] - 2026-05-28

### 修复

- 修复保存文件时扩展名始终为 .txt 的问题 (#397)
- 修复导出预览编码乱码问题 (#399)
- WinForms 导出 Rime 等格式时传递用户选择的编码类型到转换管道
- 修复 CI 构建错误（NETSDK1102/NETSDK1151）

### 新增

- 恢复选择文件后自动识别源词库格式的功能

## [3.4.1] - 2026-05-12

### 新增

- CLI 自定义编码映射功能（`-c`/`-m` 参数）
- WinForm exe 带参数时作为 CLI 运行

### 变更

- 三端（CLI/WinForms/macOS GUI）统一底层转换引擎 ConversionPipeline
- WinForms 全面现代化重构（async/await + DI + 进度报告 + 取消支持）
- 全部 50+ 词库格式迁移到新架构（ImeWlConverter.Abstractions/Core/Formats
  + Source Generator 自动注册）
- 测试框架从 NUnit 迁移到 xUnit
- 消除运行时反射，CLI 支持裁剪发布
- `--list-formats` 显示中文格式名称而非类名 (#395)

## [3.4.0] - 2026-05-07

### 新增

- 支持导出搜狗细胞词库 scel 格式
- LLM 词频生成功能
- macOS GUI 应用（基于 Avalonia UI）
- 集成测试框架

### 修复

- 修复自定义编码、Rime 拼音码表、新世纪五笔生成器等问题
- 修复汉字〇 (U+3007) 被误判为标点导致拼音丢失
- 修复 scel 格式导出，生成的文件可被搜狗输入法正确导入

### 变更

- 升级 .NET 到 10.0
- 重构命令行参数为 GNU 风格

## [3.3.1] - 2026-01-17

### 修复

- 修复转换拼音出错 (#363)
- 修复 Rime 相关问题与构建错误

### 变更

- 版本号自动化生成机制（MinVer）

## [3.3.0] - 2025-11-13

### 新增

- macOS GUI 应用基础能力

[未发布]: https://github.com/studyzy/imewlconverter/compare/v3.4.3...HEAD
[3.5.0]: https://github.com/studyzy/imewlconverter/compare/v3.4.3...v3.5.0
[3.4.3]: https://github.com/studyzy/imewlconverter/compare/v3.4.2...v3.4.3
[3.4.2]: https://github.com/studyzy/imewlconverter/compare/v3.4.1...v3.4.2
[3.4.1]: https://github.com/studyzy/imewlconverter/compare/v3.4.0...v3.4.1
[3.4.0]: https://github.com/studyzy/imewlconverter/compare/v3.3.1...v3.4.0
[3.3.1]: https://github.com/studyzy/imewlconverter/compare/v3.3.0...v3.3.1
[3.3.0]: https://github.com/studyzy/imewlconverter/releases/tag/v3.3.0
