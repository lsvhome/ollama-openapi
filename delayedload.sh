#!/bin/bash

sleep 60

date
echo delayedload.sh started
echo BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB
#/usr/sbin/sshd -D &

#cloudflared tunnel run --token ${CLOUDFLARE_TOKEN} &

#OLLAMA_HOST=http://localhost:11434 

rm -rf /ollama-openapi
cp -r /workspace/ollama-openapi /
#git clone --recursive /workspace/ollama-openapi/.git
cd /ollama-openapi/
#git checkout remotes/origin/master -b docker-local-master -f
git checkout master -b docker-local-master -f
git reset --hard



cd /workspace/


sleep 30
echo dotnet run --project /ollama-openapi/OllamaOpenApi.Service/OllamaOpenApi.Service.csproj --urls "http://*:${OLLAMA_API_PORT}" 
dotnet run --project /ollama-openapi/OllamaOpenApi.Service/OllamaOpenApi.Service.csproj --urls "http://*:${OLLAMA_API_PORT}" &

sleep 30
echo dotnet run --project /ollama-openapi/OllamaOpenApi.WebClient/OllamaOpenApi.WebClient.csproj  --urls "http://*:${OLLAMA_WEBCLIENT_PORT}"
dotnet run --project /ollama-openapi/OllamaOpenApi.WebClient/OllamaOpenApi.WebClient.csproj  --urls "http://*:${OLLAMA_WEBCLIENT_PORT}" &

date
echo delayedload.sh completed
echo BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB

