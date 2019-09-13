FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["k8DP/k8DP.csproj", "k8DP/"]
COPY ["KubeClient.Extensions.DataProtection/KubeClient.Extensions.DataProtection.csproj", "KubeClient.Extensions.DataProtection/"]
RUN dotnet restore "k8DP/k8DP.csproj"
COPY . .
WORKDIR "/src/k8DP"
RUN dotnet build "k8DP.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "k8DP.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "k8DP.dll"]