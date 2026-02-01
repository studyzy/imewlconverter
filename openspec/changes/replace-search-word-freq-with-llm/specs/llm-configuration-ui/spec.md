## 新增需求

### 需求：LLM 参数配置界面 (GUI)
系统必须在 WinForm 和 Avalonia (Mac) 界面中提供配置 LLM 的 API 地址（Endpoint）、API Key 和模型名称（Model Name）的控件。

#### 场景：在 WinForm 中保存 LLM 配置
- **当** 用户在 WinForm 窗口中输入了有效的 Endpoint、Key 和 Model 并点击保存
- **那么** 系统必须持久化这些设置，以便下次使用

#### 场景：在 Avalonia (Mac) 中保存 LLM 配置
- **当** 用户在 Mac 版词频生成窗口中输入了有效的 Endpoint、Key 和 Model 并点击确定
- **那么** 系统必须应用这些设置并生成词频

### 需求：API Key 屏蔽显示
系统在显示 API Key 时必须使用掩码（如星号）隐藏真实内容，防止泄露。

#### 场景：查看配置
- **当** 用户打开配置窗口查看已保存的设置
- **那么** API Key 文本框必须以密码字符显示内容
