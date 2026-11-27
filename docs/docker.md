> **⚠️ 注意：Docker 支持当前未维护。** 以下内容基于 v2.7.0 版本，Dockerfile 使用 .NET Core 3.1 运行时，与当前项目（.NET 10.0）不兼容。如需容器化部署，请自行基于 `mcr.microsoft.com/dotnet/runtime:10.0` 镜像构建。

### 镜像 build

> 目前 Docker Hub 上已经推送了 build 好的镜像 `mritd/imewlconverter:v2.7.0` 可直接使用，在未来本镜像将由官方直接维护届时请切换到官方版本。

如果想自行编译镜像，仅需 clone 项目源码，并在源码根目录下执行以下命令编译既可

```sh
➜  imewlconverter git:(master) docker build -t imewlconverter .
```

### 镜像使用

**镜像默认的 ENTRYPOINT 为 `ImeWlConverterCmd`，所以使用时直接跟参数既可(以下命令假定 `/dict` 为词库目录)**

```sh
docker run --rm -it -v /dict:/dict imewlconverter \
  -i scel -o rime -O /dict/java常用.rime \
  /dict/java常用.scel
```
