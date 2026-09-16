FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Copy csproj files to restore dependencies
COPY ["OllamaOpenApi.Service/OllamaOpenApi.Service.csproj", "OllamaOpenApi.Service/"]
COPY ["OllamaOpenApi.Dto/OllamaOpenApi.Dto.csproj", "OllamaOpenApi.Dto/"]
COPY ["ollamasharp/src/OllamaSharp/OllamaSharp.csproj", "ollamasharp/src/OllamaSharp/"]
COPY ["ollamasharp/src/SourceGenerators/OllamaSharp.SourceGenerators.csproj", "ollamasharp/src/SourceGenerators/"]

RUN dotnet restore "OllamaOpenApi.Service/OllamaOpenApi.Service.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/OllamaOpenApi.Service"
RUN dotnet build "OllamaOpenApi.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OllamaOpenApi.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OllamaOpenApi.Service.dll"]
