FROM ubuntu:20.04
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["trojan-modifier.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app --self-contained true -r linux-x64 /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
EXPOSE 5000
EXPOSE 5000
# Set the entry point for the application
ENTRYPOINT ["./trojan-modifier"]
