# 多架构构建：TARGETARCH（amd64/arm64）由 buildx 自动注入，
# 对应 base/ 与 update/ 下按架构分层的目录；普通 docker build 在本机平台注入相应值
# 基础镜像用精确版本 tag 固定（9.0.19 发布后不可变；浮动 tag:9.0 会随 mcr patch 漂移），
# 多平台构建时按 TARGETARCH 自动解析对应平台子清单；升 .NET patch 时与 nas-deploy.sh 预热段同步换 tag
ARG TARGETARCH
FROM mcr.microsoft.com/dotnet/aspnet:9.0.19
ARG TARGETARCH
WORKDIR /app
EXPOSE 8080
COPY base/${TARGETARCH} .
COPY update/${TARGETARCH} .
ENTRYPOINT ["dotnet", "AutoFeedSiqi.dll"]
