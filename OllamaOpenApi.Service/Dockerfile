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
COPY ["ollamasharp/Directory.Build.targets", "ollamasharp/"]
COPY ["ollamasharp/Directory.Packages.props", "ollamasharp/"]

RUN dotnet restore "OllamaOpenApi.Service/OllamaOpenApi.Service.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/OllamaOpenApi.Service"
RUN dotnet build "OllamaOpenApi.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OllamaOpenApi.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
USER root


# Base packages
RUN apt-get update && \
  apt-get install --no-install-recommends -q -y \
  apt-transport-https \
  tree \
  ca-certificates \
  gnupg \
  wget \
  curl \
  unzip \
  git \
  less \
  build-essential \
  zstd \
  markdown \
  mc \
  python3-pip \
  golang-go \
  dotnet-sdk-10.0 \
  ocl-icd-libopencl1 \
  libhwloc15 \
  screen \
  openssh-server


ENV PATH="${PATH}:/root/.dotnet/tools"

RUN dotnet tool install --global dotnet-skills
RUN dotnet tool install --global dotnet-agents
RUN dotnet tool install --global agents
RUN dotnet tool restore

RUN curl -fsSL https://pkg.cloudflare.com/cloudflare-main.gpg | tee /usr/share/keyrings/cloudflare-main.gpg >/dev/null

RUN echo 'deb [signed-by=/usr/share/keyrings/cloudflare-main.gpg] https://pkg.cloudflare.com/cloudflared any main' | tee /etc/apt/sources.list.d/cloudflared.list


RUN apt-get update && \
  apt-get install --no-install-recommends -q -y \
  cloudflared \
  && rm -rf /var/lib/apt/lists/*

RUN echo "PermitRootLogin yes" >> /etc/ssh/sshd_config

COPY ./authorized_keys /root/.ssh/authorized_keys



WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OllamaOpenApi.Service.dll"]
