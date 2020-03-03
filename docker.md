### 镜像 build

> 目前 Docker Hub 上已经推送了 build 好的镜像 `mritd/imewlconverter:v2.7.0` 可直接使用，在未来本镜像将由官方直接维护届时请切换到官方版本。

如果想自行编译镜像，仅需 clone 项目源码，并在源码根目录下执行以下命令编译既可

```sh
➜  imewlconverter git:(master) docker build -t imewlconverter .
```

### 镜像使用

**镜像默认的 ENTRYPOINT 为 `ImeWlConverterCmd`，所以使用时直接跟参数既可(以下命令假定 `/dict` 为词库目录)**

```sh
docker run --rm -it -v /dict:/dict imewlconverter -i:scel /dict/java常用.scel -os:linux -ct:pinyin -o:rime /dict/java常用.rime
```
