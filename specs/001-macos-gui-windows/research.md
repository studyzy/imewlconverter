# research.md

**Feature**: macOS GUI 复刻 Windows 版
**Created**: 2025-10-21

## NEEDS CLARIFICATION

1. Mac GUI 自动化测试方案（XCUITest/Win 对应方案？） - 决策影响: 测试覆盖与 CI 自动化
2. Mac GUI 框架选型细节（Avalonia vs MAUI vs 原生 AppKit） - 决策影响: 开发成本、界面一致性、维护成本
3. Windows 版某些平台专有功能兼容性（例如注册表/系统调用） - 决策影响: 必要的替代实现或降级策略

## 研究任务

- Research 1: 比较 Avalonia、MAUI、原生 AppKit 在 UI 复刻、主题/深色模式、快捷键和打包（mac-x64/mac-arm64）方面的优缺点，给出推荐。
- Research 2: 调研 Mac UI 自动化测试方案（XCUITest、Appium、Sikuli、Spectron/Avalonia UI 自动化）以及在 CI 中的可行性。
- Research 3: 列出 Windows 版中所有与 OS 深度集成的点（如文件关联、安装器、系统路径、注册表读写等），并为 macOS 提出替代实现或降级方案。
- Research 4: 验证现有词库解析/生成逻辑在 macOS 运行时（.NET 6/8）上无平台特有依赖，或列出需要修改的部分。

## 计划输出

每项研究输出都应包含：Decision、Rationale、Alternatives considered、Impact on schedule
