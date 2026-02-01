## 上下文

本项目目前提供基于百度和谷歌搜索结果数的词频估算功能。由于这些搜索引擎的页面结构多变且容易触发反爬虫机制，导致该功能经常失效。为了提升词频生成的可靠性和智能化程度，决定引入大语言模型（LLM）API。

## 目标 / 非目标

**目标：**
- 移除 `BaiduWordRankGenerater` 和 `GoogleWordRankGenerater`。
- 实现 `LlmWordRankGenerater`，支持 OpenAI 兼容的 API 协议。
- 在 WinForm、Avalonia 界面以及命令行工具（CLI）中提供必要的 LLM 配置项（Endpoint, API Key, Model）。
- 实现批量处理机制，将多个词语组合在一个请求中发送给 LLM。
- 内置系统级提示词，优化词频生成的准确性和返回格式。

**非目标：**
- 不会为特定的非 OpenAI 兼容模型（如某些私有协议模型）编写专门的适配器。
- 不会改变词频生成器的接口定义 `IWordRankGenerater`。

## 决策

1. **采用 OpenAI 兼容接口**：这是目前 LLM 行业的事实标准。通过支持该协议，用户可以轻松接入 OpenAI、DeepSeek、以及本地运行的 Ollama 或 LM Studio。
2. **基于 HttpClient 的实现**：使用标准的 `System.Net.Http.HttpClient` 发送 JSON 请求，不引入笨重的第三方 SDK。
3. **接口重构与批量处理**：
   - 更新 `IWordRankGenerater` 接口，增加支持批量处理词库列表的方法。
   - `LlmWordRankGenerater` 将词库列表按固定步长（如 50 个词）切分为多个 Batch。
   - 每个 Batch 构造一个包含所有词语的 Prompt，要求 LLM 返回特定格式（如 JSON）的结果。
4. **内置 Prompt 策略**：
   - 提示词应包含角色定义、任务目标、格式要求和示例。
   - 要求 LLM 仅返回结果数据，以便于系统解析。
5. **配置持久化与传递**：
   - WinForm、跨平台及命令行端同步更新配置模型，移除 Prompt 相关字段。

## 风险 / 权衡

- **网络延迟与并发控制**：LLM API 的响应速度通常慢。
  - *缓解措施*：批量处理显著降低请求次数。采用串行 Batch 处理以控制并发和上下文窗口。
- **响应解析**：批量返回结果的解析比单个数字复杂。
  - *缓解措施*：在 Prompt 中明确要求返回 JSON 数组或 Key-Value 对，并使用 `System.Text.Json` 解析。增加异常处理逻辑，若 Batch 解析失败则回退到备选方案或报错。
