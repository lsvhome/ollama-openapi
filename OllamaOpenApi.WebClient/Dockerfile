FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files to restore dependencies
COPY ["OllamaOpenApi.WebClient/OllamaOpenApi.WebClient.csproj", "OllamaOpenApi.WebClient/"]
COPY ["OllamaOpenApi.ServiceClient/OllamaOpenApi.ServiceClient.csproj", "OllamaOpenApi.ServiceClient/"]
COPY ["OllamaOpenApi.Dto/OllamaOpenApi.Dto.csproj", "OllamaOpenApi.Dto/"]
COPY ["ollamasharp/src/OllamaSharp/OllamaSharp.csproj", "ollamasharp/src/OllamaSharp/"]
COPY ["ollamasharp/src/SourceGenerators/OllamaSharp.SourceGenerators.csproj", "ollamasharp/src/SourceGenerators/"]
COPY ["ollamasharp/Directory.Build.targets", "ollamasharp/"]
COPY ["ollamasharp/Directory.Packages.props", "ollamasharp/"]

RUN dotnet restore "OllamaOpenApi.WebClient/OllamaOpenApi.WebClient.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/OllamaOpenApi.WebClient"
RUN dotnet publish "OllamaOpenApi.WebClient.csproj" -c Release -o /app/publish

# Use Nginx to serve the Blazor WebAssembly app
FROM docker.io/library/nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .

# Copy custom nginx configuration if needed, but for simple apps default is fine.
# However, for Blazor WASM, we need to handle routing.
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
