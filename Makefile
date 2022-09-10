cmd:
	dotnet build ./src/ImeWlConverterCmd

release:
	dotnet build --configuration Release ./src/ImeWlConverterCmd
	cd ./src/ImeWlConverterCmd/bin && tar czvf ./Release.tar.gz ./Release
