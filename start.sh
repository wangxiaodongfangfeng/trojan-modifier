#!/bin/bash

# Start App1 in the background
./trojan-modifier &

# Start App2 in the background
./trojan/trojan -c ./trojan/config.json &

# Wait for background processes to keep the container running
wait
