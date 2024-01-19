#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/TestTemplate9.Api/TestTemplate9.Api.csproj", "src/TestTemplate9.Api/"]
COPY ["src/TestTemplate9.Data/TestTemplate9.Data.csproj", "src/TestTemplate9.Data/"]
COPY ["src/TestTemplate9.Common/TestTemplate9.Common.csproj", "src/TestTemplate9.Common/"]
COPY ["src/TestTemplate9.Core/TestTemplate9.Core.csproj", "src/TestTemplate9.Core/"]
RUN dotnet restore "src/TestTemplate9.Api/TestTemplate9.Api.csproj"
COPY . .
WORKDIR "/src/src/TestTemplate9.Api"
RUN dotnet build "TestTemplate9.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestTemplate9.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestTemplate9.Api.dll"]