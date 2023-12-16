#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/MediaService.Api/MediaService.Api.csproj", "src/MediaService.Api/"]
COPY ["src/MediaService.Business/MediaService.Business.csproj", "src/MediaService.Business/"]
COPY ["src/MediaService.Shared/MediaService.Shared.csproj", "src/MediaService.Shared/"]
RUN dotnet restore "src/MediaService.Api/MediaService.Api.csproj"
COPY . .
WORKDIR "/src/src/MediaService.Api"
RUN dotnet build "MediaService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MediaService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
VOLUME ["/app/images"]
ENTRYPOINT ["dotnet", "MediaService.Api.dll"]