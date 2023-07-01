FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App

COPY . ./
RUN dotnet build ./src/ImeWlConverterCmd
RUN dotnet build --configuration Release ./src/ImeWlConverterCmd

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /App
COPY --from=build-env /App/src/ImeWlConverterCmd/bin/Release/net6.0 .
ENTRYPOINT ["./ImeWlConverterCmd"]

CMD ["-?"]
