#!/bin/bash

# HP Multiplayer Server Startup Script (with config file support)
# Usage: ./start-server-with-config.sh [config-file]

# Default configuration file
CONFIG_FILE="${1:-server.conf}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Default values
DEFAULT_PORT=7777
DEFAULT_MAX_PLAYERS=16
SERVER_EXE="HPMultiplayer.Server.exe"

# Load configuration from file if it exists
if [ -f "$CONFIG_FILE" ]; then
    echo "Loading configuration from: $CONFIG_FILE"
    source "$CONFIG_FILE"
fi

# Use config values or defaults
PORT=${PORT:-$DEFAULT_PORT}
MAX_PLAYERS=${MAX_PLAYERS:-$DEFAULT_MAX_PLAYERS}
SERVER_DIR=${SERVER_DIR:-$SCRIPT_DIR}

# Validate port
if ! [[ "$PORT" =~ ^[0-9]+$ ]] || [ "$PORT" -lt 1024 ] || [ "$PORT" -gt 65535 ]; then
    echo "Error: Port must be a number between 1024 and 65535"
    exit 1
fi

# Validate max players
if ! [[ "$MAX_PLAYERS" =~ ^[0-9]+$ ]] || [ "$MAX_PLAYERS" -lt 2 ] || [ "$MAX_PLAYERS" -gt 64 ]; then
    echo "Error: Max players must be a number between 2 and 64"
    exit 1
fi

# Check if Mono is installed
if ! command -v mono &> /dev/null; then
    echo "Error: Mono is not installed!"
    echo "Install it with: sudo apt-get install mono-runtime"
    exit 1
fi

# Check if server executable exists
if [ ! -f "$SERVER_DIR/$SERVER_EXE" ]; then
    echo "Error: Server executable not found: $SERVER_DIR/$SERVER_EXE"
    echo "Please ensure HPMultiplayer.Server.exe is in the correct directory"
    exit 1
fi

# Change to server directory
cd "$SERVER_DIR"

# Display startup information
echo "========================================"
echo "HP Multiplayer Dedicated Server"
echo "========================================"
echo "Port: $PORT"
echo "Max Players: $MAX_PLAYERS"
echo "Directory: $SERVER_DIR"
echo "========================================"
echo ""

# Start the server
mono "$SERVER_EXE" -port "$PORT" -maxplayers "$MAX_PLAYERS"

