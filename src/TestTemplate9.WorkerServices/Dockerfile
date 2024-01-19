#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/TestTemplate9.WorkerServices/TestTemplate9.WorkerServices.csproj", "src/TestTemplate9.WorkerServices/"]
COPY ["src/TestTemplate9.Core/TestTemplate9.Core.csproj", "src/TestTemplate9.Core/"]
COPY ["src/TestTemplate9.Common/TestTemplate9.Common.csproj", "src/TestTemplate9.Common/"]
RUN dotnet restore "src/TestTemplate9.WorkerServices/TestTemplate9.WorkerServices.csproj"
COPY . .
WORKDIR "/src/src/TestTemplate9.WorkerServices"
RUN dotnet build "TestTemplate9.WorkerServices.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestTemplate9.WorkerServices.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["src/TestTemplate9.WorkerServices/entrypoint.sh", "src/entrypoint.sh"]
COPY ["src/TestTemplate9.WorkerServices/wait-for-it.sh", "src/wait-for-it.sh"]
CMD ["/bin/bash", "src/entrypoint.sh"]
