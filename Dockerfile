FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app
COPY src src/
WORKDIR /app/src/Lightning.Metrics.App
RUN dotnet publish --runtime linux-x64 -c Release -v minimal -o out

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS runtime
WORKDIR /app
COPY --from=build /app/src/Lightning.Metrics.App/out ./
ENTRYPOINT ["/app/lnd-metrics"]
