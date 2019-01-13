
FROM microsoft/dotnet:2.2-sdk AS build
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
RUN dotnet publish -c Release -o out


# test application -- see: dotnet-docker-unit-testing.md
# FROM build AS testrunner
# WORKDIR /app/tests
# COPY tests/. .
# ENTRYPOINT ["dotnet", "test", "--logger:trx"]




FROM microsoft/dotnet:2.2-runtime AS runtime
WORKDIR /app
COPY --from=build /app/Lightning.Metrics.App/out ./
ENTRYPOINT ["dotnet", "lnd-metrics.dll"]
