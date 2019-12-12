FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["POCs3Zip.csproj", "."]
RUN dotnet restore "./POCs3Zip.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "./POCs3Zip.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./POCs3Zip.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "./POCs3Zip.dll"]