FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 8010

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["./TcpWebGateway.csproj", "TcpWebGateway/"]
RUN dotnet restore "TcpWebGateway/TcpWebGateway.csproj"
COPY . TcpWebGateway/
WORKDIR "/src/TcpWebGateway"
RUN dotnet build "TcpWebGateway.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "TcpWebGateway.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TcpWebGateway.dll"]
