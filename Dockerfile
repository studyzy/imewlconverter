FROM mcr.microsoft.com/dotnet/runtime:10.0-slim

LABEL maintainer="mritd <mritd@linux.com>"

# Timezone can be overridden at build time with --build-arg TZ=...
ARG TZ="Asia/Shanghai"
# Optional build-time SHA256 of downloaded release artifact. If provided, the downloaded
# archive will be verified and the build will fail if the checksum does not match.
ARG DOWNLOAD_SHA256=""
# If BUILD_FROM_ARTIFACT=true, the build will use an artifact supplied in the build context
# (expecting ./imewlconverter.tar.gz) instead of downloading from the public release URL.
ARG BUILD_FROM_ARTIFACT="false"

ENV TZ ${TZ}
ENV LANG en_US.UTF-8
ENV LC_ALL en_US.UTF-8
ENV LANGUAGE en_US:en

ENV VERSION v2.7.0
ENV INSTALL_DIR /usr/local/imewlconverter
ENV DOWNLOAD_URL https://github.com/studyzy/imewlconverter/releases/download/${VERSION}/imewlconverter_Linux_Mac.tar.gz

ENV PATH ${INSTALL_DIR}:${PATH}

# Install only required packages, download release artifact and verify checksum when provided.
RUN set -ex \
    && apt-get update \
    && apt-get install -y --no-install-recommends tzdata locales curl ca-certificates gnupg \
    && mkdir -p ${INSTALL_DIR} \
    # If build context provides artifact, use it; otherwise download from release URL
    && if [ "${BUILD_FROM_ARTIFACT}" = "true" ] ; then \
         if [ -f ./imewlconverter.tar.gz ] ; then cp ./imewlconverter.tar.gz /tmp/imewlconverter.tar.gz ; else echo "Expected imewlconverter.tar.gz in build context" >&2 ; exit 1 ; fi ; \
       else \
         curl --fail --location --silent --show-error "${DOWNLOAD_URL}" -o /tmp/imewlconverter.tar.gz ; \
       fi \
    # If a checksum is supplied at build time, verify the artifact before extracting
    && if [ -n "${DOWNLOAD_SHA256}" ]; then echo "${DOWNLOAD_SHA256}  /tmp/imewlconverter.tar.gz" | sha256sum -c -; fi \
    && tar -zxf /tmp/imewlconverter.tar.gz -C ${INSTALL_DIR} \
    && locale-gen --purge en_US.UTF-8 zh_CN.UTF-8 || true \
    && localedef -i en_US -c -f UTF-8 -A /usr/share/locale/locale.alias en_US.UTF-8 || true \
    && echo 'LANG="en_US.UTF-8"' > /etc/default/locale \
    && echo 'LANGUAGE="en_US:en"' >> /etc/default/locale \
    && ln -sf /usr/share/zoneinfo/${TZ} /etc/localtime \
    && echo ${TZ} > /etc/timezone \
    && useradd --system --create-home --home-dir /home/imewl imewl \
    && chown -R imewl:imewl ${INSTALL_DIR} \
    && rm -rf /tmp/imewlconverter.tar.gz /var/lib/apt/lists/*

USER imewl

ENTRYPOINT ["ImeWlConverterCmd"]

CMD ["-h"]
