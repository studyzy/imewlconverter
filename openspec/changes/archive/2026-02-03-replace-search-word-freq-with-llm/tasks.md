## 1. 接口与核心逻辑重构 (ImeWlConverterCore)

- [x] 1.1 更新 `IWordRankGenerater` 接口，增加 `void GenerateRank(WordLibraryList wordLibraryList)` 方法
- [x] 1.2 在 `DefaultWordRankGenerater` 和 `CalcWordRankGenerater` 中实现该方法（保持原有逻辑）
- [x] 1.3 修改 `MainBody.GenerateWordRank`，优先调用生成器的 `GenerateRank` 方法，若不支持则回退到循环调用 `GetRank`
- [x] 1.4 在 `LlmWordRankGenerater` 中实现批量处理逻辑：
    - [x] 1.4.1 实现分批（Batch）切分逻辑（每批约 50 个词）
    - [x] 1.4.2 定义内置的优化提示词（Prompt）模板
    - [x] 1.4.3 实现批量请求与结果解析逻辑（支持解析 JSON 或结构化文本）

## 2. 配置清理

- [x] 2.1 修改 `LlmConfig.cs`，移除 `Prompt` 属性
- [x] 2.2 修改 `CommandLineOptions.cs`，移除 `LlmPrompt` 参数
- [x] 2.3 清理 WinForm 和 Avalonia (Mac) 界面中的 Prompt 配置控件与相关逻辑

## 4. 增强功能与兼容性

- [x] 4.1 自动适配 LLM Endpoint 后缀：若未以 `/v1/chat/completions` 结尾则自动补全
