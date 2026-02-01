## 为什么

目前的词频生成功能依赖于百度和谷歌搜索结果的数量估算，这种方式存在多个问题：
1. **批量处理**：不再逐个单词调用 API，而是将一批词语合并发送给 LLM，大幅提升效率并降低网络开销。
2. **内嵌提示词**：移除用户配置 Prompt 的需求，改为系统内置经过优化的提示词，减少用户的操作复杂度。
3. **准确性提升**：大语言模型（LLM）API 可以提供更稳定、智能且可定制的词频推断能力。

引入大语言模型（LLM）API 生成词频可以提供更稳定、智能且可定制的词频推断能力。

## 变更内容

1. **移除旧功能**：从核心库中移除 `BaiduWordRankGenerater` 和 `GoogleWordRankGenerater`。
2. **新增 LLM 生成器**：在 `ImeWlConverterCore` 中实现支持 OpenAI 兼容接口的 `LlmWordRankGenerater`，支持批量词频生成。
3. **更新 WinForm 界面**：修改 `WordRankGenerateForm`，移除百度/谷歌选项，增加 LLM 配置项（API 地址、Key、模型名称等），移除 Prompt 模板配置。
4. **更新 Avalonia 界面**：同步更新 `ImeWlConverterMac` 中的词频设置窗口，移除 Prompt 配置。
5. **支持命令行模式**：在 `IME WL Converter` 命令行工具中增加 LLM 相关参数，支持通过命令行配置 API 详情并生成词频。

## 功能 (Capabilities)

### 新增功能
- `llm-word-rank-generation`: 实现基于 LLM API 的词频生成逻辑，兼容 OpenAI 格式。
- `llm-configuration-ui`: 在 WinForm 和 Avalonia UI 中提供 LLM 参数配置界面。
- `llm-configuration-cli`: 在命令行工具中提供 LLM 参数配置选项。

### 修改功能
- `word-rank-management`: 调整现有的词频管理逻辑以接入新的生成器并移除旧的搜索生成器。

## 影响

- **代码库**：`ImeWlConverterCore`（新增类，移除旧类）、`IME WL Converter Win`（UI 变更）、`ImeWlConverterMac`（UI 变更）、`IME WL Converter`（CLI 参数变更）。
- **依赖**：可能需要引入简单的 HTTP 客户端处理逻辑（如果现有逻辑不够用）。
- **用户体验**：用户需要提供自己的 LLM API Key 才能使用增强的词频生成功能。
