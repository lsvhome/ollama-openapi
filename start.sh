#!/bin/bash
echo "------------ AAAAAAAAAAA START -------------"

/bin/bash /delayedload.sh &

echo "------------ AAAAAAAAAAA2 START -------------"

count=10

# ( for i in $(seq $count); do /load-model.sh; done; )&

#ollama load ${OLLAMA_MODEL} --gpu --keepalive -1     

ollama serve