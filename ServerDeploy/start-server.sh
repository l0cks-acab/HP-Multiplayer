#!/bin/bash

# HP Multiplayer Server Startup Script
# Usage: ./start-server.sh [port] [maxplayers]

# Default configuration
DEFAULT_PORT=7777
DEFAULT_MAX_PLAYERS=16
SERVER_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SERVER_EXE="HPMultiplayer.Server.exe"

# Configuration from command line or environment variables
PORT=${1:-${PORT:-$DEFAULT_PORT}}
MAX_PLAYERS=${2:-${MAX_PLAYERS:-$DEFAULT_MAX_PLAYERS}}

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
    echo "Please ensure HPMultiplayer.Server.exe is in the same directory as this script"
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

