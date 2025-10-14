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
	./scripts/create-app-bundle.sh ./publish/mac-arm64 "IME WL Converter"
	@echo "✅ ARM64 版本的 .app 包已创建完成"

app-mac-x64: publish-mac
	./scripts/create-app-bundle.sh ./publish/mac-x64 "IME WL Converter"
	@echo "✅ x64 版本的 .app 包已创建完成"

# 创建通用 .app 包（推荐）
app-mac: app-mac-arm64
	@echo "🎉 macOS 应用包创建完成！"
	@echo "📁 位置: ./IME WL Converter.app"
	@echo "🚀 使用方法:"
	@echo "   - 双击运行: open './IME WL Converter.app'"
	@echo "   - 安装到应用程序文件夹: cp -r './IME WL Converter.app' /Applications/"

.PHONY: cmd release build-mac release-mac run-mac clean-mac publish-mac app-mac-arm64 app-mac-x64 app-mac
