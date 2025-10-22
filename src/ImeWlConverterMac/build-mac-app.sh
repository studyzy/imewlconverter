#!/bin/bash

# 深蓝词库转换 macOS App 构建脚本

echo "开始构建 macOS 应用程序..."

# 清理之前的构建
dotnet clean

# 发布应用程序
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=false

# 创建 App Bundle 结构
APP_NAME="深蓝词库转换.app"
BUILD_DIR="bin/Release/net8.0/osx-arm64/publish"
APP_DIR="$BUILD_DIR/$APP_NAME"

echo "创建 App Bundle 结构..."
mkdir -p "$APP_DIR/Contents/MacOS"
mkdir -p "$APP_DIR/Contents/Resources"

# 复制可执行文件和依赖
cp -r $BUILD_DIR/* "$APP_DIR/Contents/MacOS/" 2>/dev/null || true
rm -rf "$APP_DIR/Contents/MacOS/$APP_NAME"

# 复制 Info.plist
cp Info.plist "$APP_DIR/Contents/"

# 如果有图标文件，复制它
if [ -f "Assets/AppIcon.icns" ]; then
    cp Assets/AppIcon.icns "$APP_DIR/Contents/Resources/"
fi

echo "构建完成！"
echo "应用程序位置: $APP_DIR"
echo ""
echo "运行应用程序:"
echo "open \"$APP_DIR\""
