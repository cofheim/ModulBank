FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src
COPY ["ModulBank/ModulBank.csproj", "ModulBank/"]
COPY ["ModulBank.Application/ModulBank.Application.csproj", "ModulBank.Application/"]
COPY ["ModulBank.DataAccess/ModulBank.DataAccess.csproj", "ModulBank.DataAccess/"]
COPY ["ModulBank.Domain/ModulBank.Domain.csproj", "ModulBank.Domain/"]
RUN dotnet restore "ModulBank/ModulBank.csproj"
COPY . .
WORKDIR "/src/ModulBank"
RUN dotnet build "ModulBank.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ModulBank.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ModulBank.dll"]