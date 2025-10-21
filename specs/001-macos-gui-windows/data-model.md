# data-model.md

**Feature**: macOS GUI 复刻 Windows 版

## 关键实体与字段

- 词库文件
  - file_path: string
  - file_name: string
  - format: string
  - encoding: string
  - size_bytes: integer
  - status: enum (pending, processing, success, error)
  - error_message: string (nullable)

- 转换任务
  - task_id: string
  - source_files: list[词库文件]
  - target_format: string
  - options: dict (export_path, encoding, overwrite)
  - created_at: datetime
  - completed_at: datetime (nullable)
  - results: list[转换结果]

- 转换结果
  - file: 词库文件
  - output_path: string
  - status: enum (success, error)
  - message: string (nullable)

- 用户设置
  - language: string
  - theme: string (light/dark/system)
  - default_output_dir: string
  - recent_files: list[string]
