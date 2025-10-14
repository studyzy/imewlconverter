#!/bin/bash

# 创建 macOS .app 包的脚本
# 使用方法: ./scripts/create-app-bundle.sh <publish-dir> <app-name>

set -e

PUBLISH_DIR="$1"
APP_NAME="$2"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

if [ -z "$PUBLISH_DIR" ] || [ -z "$APP_NAME" ]; then
    echo "使用方法: $0 <publish-dir> <app-name>"
    echo "例如: $0 ./publish/mac-arm64 \"IME WL Converter\""
    exit 1
fi

if [ ! -d "$PUBLISH_DIR" ]; then
    echo "错误: 发布目录 '$PUBLISH_DIR' 不存在"
    exit 1
fi

APP_BUNDLE="${APP_NAME}.app"
CONTENTS_DIR="${APP_BUNDLE}/Contents"
MACOS_DIR="${CONTENTS_DIR}/MacOS"
RESOURCES_DIR="${CONTENTS_DIR}/Resources"

echo "正在创建 .app 包: $APP_BUNDLE"

# 清理旧的 .app 包
if [ -d "$APP_BUNDLE" ]; then
    rm -rf "$APP_BUNDLE"
fi

# 创建 .app 目录结构
mkdir -p "$MACOS_DIR"
mkdir -p "$RESOURCES_DIR"

# 复制可执行文件和依赖
echo "复制应用程序文件..."
cp -r "$PUBLISH_DIR"/* "$MACOS_DIR/"

# 复制 Info.plist
echo "复制 Info.plist..."
if [ -f "$PROJECT_ROOT/src/ImeWlConverterMac/Info.plist" ]; then
    cp "$PROJECT_ROOT/src/ImeWlConverterMac/Info.plist" "$CONTENTS_DIR/"
else
    echo "警告: Info.plist 文件不存在，创建默认文件"
    cat > "$CONTENTS_DIR/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundleDisplayName</key>
    <string>$APP_NAME</string>
    <key>CFBundleIdentifier</key>
    <string>com.imewlconverter.mac</string>
    <key>CFBundleVersion</key>
    <string>1.0.0</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0.0</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleExecutable</key>
    <string>ImeWlConverterMac</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.15</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
EOF
fi

# 复制应用图标（如果存在）
if [ -f "$PROJECT_ROOT/src/ImeWlConverterMac/AppIcon.icns" ]; then
    echo "复制应用图标..."
    cp "$PROJECT_ROOT/src/ImeWlConverterMac/AppIcon.icns" "$RESOURCES_DIR/"
fi

# 设置可执行权限
chmod +x "$MACOS_DIR/ImeWlConverterMac"

# 创建 PkgInfo 文件
echo -n "APPL????" > "$CONTENTS_DIR/PkgInfo"

echo "✅ 成功创建 macOS 应用包: $APP_BUNDLE"
echo "📁 应用包位置: $(pwd)/$APP_BUNDLE"
echo ""
echo "🚀 您现在可以："
echo "   1. 双击 $APP_BUNDLE 运行应用"
echo "   2. 将 $APP_BUNDLE 拖拽到 Applications 文件夹"
echo "   3. 使用 'open $APP_BUNDLE' 命令运行"