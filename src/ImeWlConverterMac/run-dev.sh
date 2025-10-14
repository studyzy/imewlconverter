#!/bin/bash

# 开发环境快速启动脚本

echo "🚀 启动深蓝词库转换工具开发版本..."

# 检查.NET是否安装
if ! command -v dotnet &> /dev/null; then
    echo "❌ 错误: 未找到.NET SDK"
    exit 1
fi

# 恢复依赖项（如果需要）
if [ ! -d "bin" ] || [ ! -d "obj" ]; then
    echo "📦 首次运行，恢复依赖项..."
    dotnet restore
fi

# 运行应用程序
echo "▶️  启动应用程序..."
dotnet run --configuration Debug

echo "✅ 应用程序已退出"