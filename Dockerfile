FROM mcr.microsoft.com/dotnet/core/runtime:3.1

LABEL maintainer="mritd <mritd@linux.com>"

ARG TZ="Asia/Shanghai"

ENV TZ ${TZ}
ENV LANG en_US.UTF-8
ENV LC_ALL en_US.UTF-8
ENV LANGUAGE en_US:en

ENV VERSION v2.7.0
ENV INSTALL_DIR /usr/local/imewlconverter
ENV DOWNLOAD_URL https://github.com/studyzy/imewlconverter/releases/download/${VERSION}/imewlconverter_Linux_Mac.tar.gz

ENV PATH ${INSTALL_DIR}:${PATH}

RUN set -ex \
    && apt update -y \
    && apt upgrade -y \
    && apt install tzdata locales curl -y \
    && curl -sSL ${DOWNLOAD_URL} > imewlconverter.tar.gz \
    && mkdir -p ${INSTALL_DIR} \
    && tar -zxf imewlconverter.tar.gz -C ${INSTALL_DIR} \
    && locale-gen --purge en_US.UTF-8 zh_CN.UTF-8 \
    && localedef -i en_US -c -f UTF-8 -A /usr/share/locale/locale.alias en_US.UTF-8 \
    && echo 'LANG="en_US.UTF-8"' > /etc/default/locale \
    && echo 'LANGUAGE="en_US:en"' >> /etc/default/locale \
    && ln -sf /usr/share/zoneinfo/${TZ} /etc/localtime \
    && echo ${TZ} > /etc/timezone \
    && apt autoremove -y \
    && apt autoclean -y \
    && rm -rf imewlconverter.tar.gz /var/lib/apt/lists/*

ENTRYPOINT ["ImeWlConverterCmd"]

CMD ["-h"]
