#!/bin/bash

# 创建简单的应用图标
# 这个脚本会创建一个基本的 .icns 图标文件

ICON_DIR="src/ImeWlConverterMac"
ICON_NAME="AppIcon.icns"

echo "正在创建应用图标..."

# 检查是否有 sips 命令（macOS 内置）
if ! command -v sips &> /dev/null; then
    echo "警告: sips 命令不可用，跳过图标创建"
    exit 0
fi

# 创建临时目录
TEMP_DIR=$(mktemp -d)
ICONSET_DIR="$TEMP_DIR/AppIcon.iconset"
mkdir -p "$ICONSET_DIR"

# 创建一个简单的 PNG 图标（使用 sips 创建纯色图标）
# 这里创建一个蓝色的正方形作为临时图标
sips -s format png --out "$TEMP_DIR/base.png" -s dpiHeight 72 -s dpiWidth 72 -z 1024 1024 /System/Library/CoreServices/CoreTypes.bundle/Contents/Resources/GenericApplicationIcon.icns 2>/dev/null

if [ ! -f "$TEMP_DIR/base.png" ]; then
    # 如果上面的方法失败，创建一个简单的图标
    echo "创建默认图标..."
    # 使用 iconutil 的默认行为
    mkdir -p "$ICONSET_DIR"
    
    # 创建不同尺寸的图标文件（空文件，iconutil 会处理）
    for size in 16 32 128 256 512; do
        touch "$ICONSET_DIR/icon_${size}x${size}.png"
        touch "$ICONSET_DIR/icon_${size}x${size}@2x.png"
    done
else
    # 生成不同尺寸的图标
    sips -z 16 16 "$TEMP_DIR/base.png" --out "$ICONSET_DIR/icon_16x16.png"
    sips -z 32 32 "$TEMP_DIR/base.png" --out "$ICONSET_DIR/icon_16x16@2x.png"
    sips -z 32 32 "$TEMP_DIR/base.png" --out "$ICONSET_DIR/icon_32x32.png"
    sips -z 64 64 "$TEMP_DIR/base.png" --out "$ICONSET_DIR/icon_32x32@2x.png"
    sips -z 128 128 "$TEMP_DIR/base.png" --out "$ICONSET_DIR/icon_128x128.png"
    sips -z 256 256 "$TEMP_DIR/base.png" --out "$ICONSET_DIR/icon_128x128@2x.png"
    sips -z 256 256 "$TEMP_DIR/base.png" --out "$ICONSET_DIR/icon_256x256.png"
    sips -z 512 512 "$TEMP_DIR/base.png" --out "$ICONSET_DIR/icon_256x256@2x.png"
    sips -z 512 512 "$TEMP_DIR/base.png" --out "$ICONSET_DIR/icon_512x512.png"
    sips -z 1024 1024 "$TEMP_DIR/base.png" --out "$ICONSET_DIR/icon_512x512@2x.png"
fi

# 创建 .icns 文件
if command -v iconutil &> /dev/null; then
    iconutil -c icns "$ICONSET_DIR" -o "$ICON_DIR/$ICON_NAME"
    echo "✅ 图标已创建: $ICON_DIR/$ICON_NAME"
else
    echo "警告: iconutil 命令不可用，无法创建 .icns 文件"
fi

# 清理临时文件
rm -rf "$TEMP_DIR"

echo "图标创建完成！"