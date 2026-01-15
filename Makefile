cmd:
	dotnet build ./src/ImeWlConverterCmd

release:
	dotnet build --configuration Release ./src/ImeWlConverterCmd
	cd ./src/ImeWlConverterCmd/bin && tar czvf ./Release.tar.gz ./Release

build-mac:
	dotnet build ./src/ImeWlConverterMac

release-mac:
	dotnet build --configuration Release ./src/ImeWlConverterMac
	cd ./src/ImeWlConverterMac/bin && tar czvf ./Release-Mac.tar.gz ./Release

run-mac:
	cd ./src/ImeWlConverterMac && dotnet run

clean-mac:
	dotnet clean ./src/ImeWlConverterMac

publish-mac:
	dotnet publish ./src/ImeWlConverterMac --configuration Release --self-contained true --runtime osx-arm64 --output ./publish/mac-arm64
	dotnet publish ./src/ImeWlConverterMac --configuration Release --self-contained true --runtime osx-x64 --output ./publish/mac-x64

# 创建 macOS .app 包
app-mac-arm64: publish-mac
	./scripts/create-app-bundle.sh ./publish/mac-arm64 "深蓝词库转换"
	@echo "✅ ARM64 版本的 .app 包已创建完成"

app-mac-x64: publish-mac
	./scripts/create-app-bundle.sh ./publish/mac-x64 "深蓝词库转换"
	@echo "✅ x64 版本的 .app 包已创建完成"

# 创建通用 .app 包（推荐）
app-mac: app-mac-arm64 app-mac-x64
	@echo "🎉 macOS 应用包创建完成！"
	@echo "📁 ARM64 版本: ./深蓝词库转换-arm64.app"
	@echo "📁 x64 版本: ./深蓝词库转换-x64.app"
	@echo "🚀 使用方法:"
	@echo "   - 双击运行: open './深蓝词库转换-arm64.app' 或 open './深蓝词库转换-x64.app'"
	@echo "   - 安装到应用程序文件夹: cp -r './深蓝词库转换-arm64.app' /Applications/ 或 cp -r './深蓝词库转换-x64.app' /Applications/"

# 打包发布版本
package-mac: app-mac
	@echo "📦 正在打包 macOS 应用..."
	zip -r "深蓝词库转换-arm64.zip" "深蓝词库转换-arm64.app"
	zip -r "深蓝词库转换-x64.zip" "深蓝词库转换-x64.app"
	@echo "✅ 打包完成！"
	@echo "📁 ARM64 包: ./深蓝词库转换-arm64.zip"
	@echo "📁 x64 包: ./深蓝词库转换-x64.zip"

# 清理生成的文件
clean-packages:
	@echo "🧹 清理生成的包文件..."
	rm -rf "深蓝词库转换.app" "深蓝词库转换-arm64.app" "深蓝词库转换-x64.app"
	rm -f "深蓝词库转换-arm64.zip" "深蓝词库转换-x64.zip"
	rm -rf ./publish
	@echo "✅ 清理完成！"

# 显示当前版本号（由 MinVer 从 Git tag 自动生成）
version:
	@echo "📌 当前版本号:"
	@cd src/ImeWlConverterCore && dotnet msbuild -getProperty:Version -nologo || echo "⚠️  无法获取版本号，请确保已安装 .NET SDK"

.PHONY: cmd release build-mac release-mac run-mac clean-mac publish-mac app-mac-arm64 app-mac-x64 app-mac package-mac clean-packages version