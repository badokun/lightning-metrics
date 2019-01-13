FROM microsoft/dotnet:2.2-sdk-stretch-arm32v7 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY src/Lightning.Metrics.App/*.csproj ./Lightning.Metrics.App/
COPY src/Lightning.Metrics/*.csproj ./Lightning.Metrics/
WORKDIR /app/Lightning.Metrics.App
# RUN dotnet restore
RUN dotnet restore -r linux-arm

# copy and publish app and libraries
WORKDIR /app/
COPY src/Lightning.Metrics.App/. ./Lightning.Metrics.App/
COPY src/Lightning.Metrics/. ./Lightning.Metrics/
WORKDIR /app/Lightning.Metrics.App
RUN dotnet publish -c Release -r linux-arm -o out

FROM microsoft/dotnet:2.2-runtime-deps-stretch-slim-arm32v7 AS runtime
WORKDIR /app
COPY --from=build /app/Lightning.Metrics.App/out ./
ENTRYPOINT ["./lnd-metrics"]
