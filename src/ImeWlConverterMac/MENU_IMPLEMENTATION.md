# macOS 菜单功能实现说明

## 已实现的菜单功能

本次更新已经将 Windows 版本的所有菜单功能复刻到 macOS 版本中。

### 1. 高级设置菜单

#### 1.1 词条过滤设置 ✅
- **文件**: `Views/FilterConfigWindow.axaml` 和 `Views/FilterConfigWindow.axaml.cs`
- **功能**: 
  - 词长过滤（从-到）
  - 词频过滤（从-到）
  - 词频百分比过滤
  - 忽略选项（英文、空格、标点符号、数字等）
  - 替换选项
  - 保留选项
  - 其他选项（全角、中文数字、英文前缀）
- **调用**: 点击菜单 "高级设置" -> "词条过滤设置"

#### 1.2 词频生成设置 ✅
- **文件**: `Views/WordRankGenerateWindow.axaml` 和 `Views/WordRankGenerateWindow.axaml.cs`
- **功能**:
  - 默认词频（可设置默认值）
  - Google词频
  - 百度词频
  - 计算词频
  - 强制使用新词频选项
- **调用**: 点击菜单 "高级设置" -> "词频生成设置"

#### 1.3 简繁体转换设置 ✅
- **文件**: `Views/ChineseConverterSelectWindow.axaml` 和 `Views/ChineseConverterSelectWindow.axaml.cs`
- **功能**:
  - 不转换
  - 转换为简体中文
  - 转换为繁体中文
  - 转换引擎：系统内核（macOS 仅支持系统内核）
- **调用**: 点击菜单 "高级设置" -> "简繁体转换设置"

#### 1.4 显示选项切换 ✅
- **结果只显示首、末10万字符**: 避免大文件导致界面卡顿
- **不显示结果，直接导出**: 提高处理速度
- **一边读取，一边导出**: 流式处理，适用于文本格式词库
- **合并多词库到一个文件**: 控制是否合并多个词库文件

### 2. 帮助菜单

#### 2.1 捐赠 ✅
- **文件**: `Views/DonateWindow.axaml` 和 `Views/DonateWindow.axaml.cs`
- **功能**: 显示捐赠信息和支持方式
- **调用**: 点击菜单 "帮助" -> "捐赠"

#### 2.2 帮助 ✅
- **文件**: `Views/HelpWindow.axaml` 和 `Views/HelpWindow.axaml.cs`
- **功能**: 显示使用帮助文档
- **调用**: 点击菜单 "帮助" -> "帮助"

#### 2.3 关于 ✅
- **文件**: `Views/AboutWindow.axaml` 和 `Views/AboutWindow.axaml.cs`
- **功能**: 显示软件版本、版权信息和描述
- **调用**: 点击菜单 "帮助" -> "关于"

#### 2.4 查看最新版本 ✅
- **功能**: 在浏览器中打开 GitHub Releases 页面
- **调用**: 点击菜单 "帮助" -> "查看最新版本"

#### 2.5 文件分割 ✅
- **文件**: `Views/SplitFileWindow.axaml` 和 `Views/SplitFileWindow.axaml.cs`
- **功能**: 
  - 按行数分割文件
  - 按文件大小分割（KB）
  - 按字符数分割
  - 实时显示分割日志
  - 自动检测文件编码
- **调用**: 点击菜单 "帮助" -> "文件分割"

#### 2.6 词库合并 ✅
- **文件**: `Views/MergeWLWindow.axaml` 和 `Views/MergeWLWindow.axaml.cs`
- **功能**:
  - 选择主词库文件
  - 选择多个附加词库文件
  - 按编码排序选项
  - 实时显示合并结果
  - 保存合并后的词库
- **调用**: 点击菜单 "帮助" -> "词库合并"

## 技术实现细节

### 对话框模式
所有对话框都使用 Avalonia 的 `ShowDialog` 方法，确保模态显示：
```csharp
var window = new FilterConfigWindow(_filterConfig);
var result = await window.ShowDialog<bool?>(mainWindow);
if (result == true)
{
    // 用户点击了确定
}
```

### 配置持久化
- `FilterConfig`: 过滤配置在 ViewModel 中保存
- `WordRankGenerater`: 词频生成器在 ViewModel 中保存
- `ChineseTranslate` 和 `IChineseConverter`: 简繁转换配置在 ViewModel 中保存

### 菜单项绑定
菜单项通过 Command 绑定到 ViewModel 中的命令：
```xml
<MenuItem Header="词条过滤设置" Command="{Binding FilterConfigCommand}"/>
```

### 切换菜单项
使用 IsVisible 属性实现复选框效果：
```xml
<MenuItem Header="✓ 结果只显示首、末10万字符" 
          Command="{Binding ToggleShowLessCommand}" 
          IsVisible="{Binding ShowLess}"/>
<MenuItem Header="  结果只显示首、末10万字符" 
          Command="{Binding ToggleShowLessCommand}" 
          IsVisible="{Binding !ShowLess}"/>
```

## 与 Windows 版本的差异

1. **Office 组件**: macOS 版本不支持 Office 组件，仅使用系统内核进行简繁转换
2. **文件对话框**: 使用 Avalonia 的跨平台文件对话框 API
3. **UI 框架**: Windows 版本使用 WinForms，macOS 版本使用 Avalonia UI

## 测试建议

1. 测试所有菜单项是否能正常打开对应的对话框
2. 测试配置修改后是否能正确应用到转换过程中
3. 测试切换菜单项是否能正确切换状态
4. 测试对话框的取消操作是否不会修改配置

## 文件分割功能详解

### 支持的分割方式

1. **按行数分割**
   - 设置每个文件包含的最大行数
   - 自动检测行分隔符（\r\n, \r, \n）
   - 保持原文件编码

2. **按文件大小分割**
   - 设置每个文件的最大大小（KB）
   - 智能在行尾分割，避免截断词条
   - 自动添加文件头（BOM）

3. **按字符数分割**
   - 设置每个文件的最大字符数
   - 智能在行尾分割
   - 保持词条完整性

### 分割文件命名规则
分割后的文件会自动命名为：`原文件名01.扩展名`、`原文件名02.扩展名` 等

## 词库合并功能详解

### 合并规则

1. **主词库**: 作为合并的基础词库
2. **附加词库**: 可以选择多个，会依次合并到主词库中
3. **去重**: 自动去除重复的词条
4. **排序**: 可选择按编码排序

### 词库格式要求
每一行的格式必须为：`编码 词1 词2 词3`

例如：
```
a 啊 阿 呵
ai 爱 哀 唉
```

## 后续优化建议

1. ✅ 所有菜单功能已完整实现
2. 添加更多的错误处理和用户提示
3. 优化对话框的 UI 布局和用户体验
4. 添加进度条显示（用于大文件处理）
5. 支持拖拽文件到对话框
