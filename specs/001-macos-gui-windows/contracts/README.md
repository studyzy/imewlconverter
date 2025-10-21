# contracts/

此目录用于存放 API 合同或 CLI 交互约定。对于本地 GUI 应用，优先记录 CLI 子命令和转换任务的输入/输出约定（JSON 格式）。

示例 CLI contract:

```
Command: convert
Input: JSON { "files": ["/path/a.scel"], "target": "rime", "options": { "encoding": "utf-8", "output_dir": "/tmp" } }
Output: JSON { "task_id": "...", "results": [{ "file": "a.scel","status":"success","output":"/tmp/a.txt" }] }
```
