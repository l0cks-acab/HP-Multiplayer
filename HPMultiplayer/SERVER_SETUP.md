# HP Multiplayer Dedicated Server Setup

Complete guide for building, running, and hosting the HP Multiplayer dedicated server.

## Overview

The dedicated server is a **headless console application** that runs independently of the game client. It supports:
- Up to 16 players (configurable)
- 24/7 hosting without a player present
- Better performance and stability
- Suitable for public servers

## Prerequisites

- **Windows:** .NET Framework 4.6.1 or later
- **Linux:** Mono runtime or .NET Core/.NET 5+
- **Visual Studio 2019+** or Build Tools (for building)

## Building the Server

### Windows

1. **Install Build Tools:**
   - Download: https://visualstudio.microsoft.com/downloads/
   - Install "Build Tools for Visual Studio 2022"
   - Select ".NET desktop build tools"

2. **Build:**
   ```bash
   msbuild Server\HPMultiplayer.Server.csproj /p:Configuration=Release
   ```

3. **Output:** `Server\bin\Release\HPMultiplayer.Server.exe`

### Linux

**Option 1: Using Mono (Quick Setup)**

1. Install Mono:
   ```bash
   sudo apt-get update
   sudo apt-get install mono-complete mono-devel
   ```

2. Build on Windows (same as above) or use Mono compiler:
   ```bash
   mcs -out:HPMultiplayer.Server.exe Server/*.cs Networking/NetworkProtocol.cs
   ```

3. Run with Mono:
   ```bash
   mono HPMultiplayer.Server.exe -port 7777 -maxplayers 16
   ```

**Option 2: Using .NET Core (Recommended)**

1. Migrate project to .NET Core/.NET 6 (requires code changes)
2. Build for Linux:
   ```bash
   dotnet publish Server/HPMultiplayer.Server.csproj -r linux-x64 -c Release
   ```

3. Run directly:
   ```bash
   ./HPMultiplayer.Server -port 7777 -maxplayers 16
   ```

## Running the Server

### Basic Usage

```bash
HPMultiplayer.Server.exe
```

Starts on port **7777** with max **16 players** (defaults).

### Command Line Arguments

```bash
HPMultiplayer.Server.exe -port 7777 -maxplayers 16
```

- `-port <number>` - Server port (default: 7777)
- `-maxplayers <number>` - Maximum players (default: 16)
- `-help` or `-h` - Show help message

### Server Commands

While running, type commands in the console:
- `help` - Show available commands
- `status` or `info` - Show server status
- `players` or `list` - List connected players
- `quit` or `exit` - Stop the server

### Example Session

```
========================================
HP Multiplayer Dedicated Server
========================================

Server Configuration:
  Port: 7777
  Max Players: 16

[Server] Started on port 7777
Server started successfully on port 7777
Type 'help' for commands, 'quit' to stop the server

[Server] Player 1 (Player) connected from 192.168.1.100:52341
[Server] Player 2 (John) connected from 192.168.1.101:52342
```

## Connecting to the Server

From the game client:
1. Press **M** to open the multiplayer UI
2. Enter the server IP address and port (e.g., `192.168.1.50:7777`)
3. Click **"Join Game"**

The client mod will automatically use the dedicated server protocol when connecting.

## Port Forwarding

If hosting from behind a router:

1. Forward UDP port 7777 to your server's local IP
2. Configure firewall to allow UDP port 7777
3. Test with an external connection

**Windows Firewall:**
```powershell
New-NetFirewallRule -DisplayName "HP Multiplayer Server" -Direction Inbound -Protocol UDP -LocalPort 7777 -Action Allow
```

**Linux Firewall (ufw):**
```bash
sudo ufw allow 7777/udp
```

## Hosting on AMP

For hosting on AMP (Application Management Panel), see [AMP_SETUP.md](AMP_SETUP.md).

## Server Configuration

### Settings

Currently configurable via command-line arguments:
- Port number
- Maximum players

Future versions may support:
- Configuration file
- Password protection
- Server name/motd
- Player authentication

### Performance

**System Requirements:**
- CPU: Minimal (<5% for idle server)
- Memory: ~50-100 MB base + ~5 MB per player
- Network: ~1-2 KB/s per player

**Recommended:**
- Dedicated server hardware
- Stable network connection
- Sufficient bandwidth for your player count

## Troubleshooting

### Server Won't Start

- **Port in use:** Change port number or stop conflicting service
- **Permission denied:** Run as administrator (Windows) or with sudo (Linux)
- **Missing dependencies:** Ensure .NET Framework 4.6.1+ (Windows) or Mono/.NET Runtime (Linux)

### Players Can't Connect

- Verify server is running and listening on correct port
- Check firewall rules (Windows Firewall, router firewall)
- Ensure port forwarding is configured if hosting remotely
- Verify server IP address is correct
- Test port with: `telnet <server-ip> 7777` (won't work for UDP, but verifies connectivity)

### High CPU/Memory Usage

- Reduce max players
- Check for memory leaks (restart daily if needed)
- Monitor individual player impact

### Connection Timeouts

- Check server logs for disconnection reasons
- Verify network stability
- Increase timeout values in code if needed (requires rebuild)

## Server vs P2P Comparison

| Feature | Dedicated Server | P2P Mode |
|---------|-----------------|----------|
| Max Players | 16 (configurable) | 2 |
| Host Required | No | Yes (one player hosts) |
| Server Stability | High | Depends on host |
| Setup Complexity | Medium | Low |
| Resource Usage | Server hardware | Player's computer |
| Best For | Public servers, large groups | Private games with friends |

## Next Steps

- Set up automatic startup (Windows Service, systemd, etc.)
- Configure monitoring/alerting
- Set up regular backups
- Configure master server for game discovery (see [MASTER_SERVER_INFO.md](MASTER_SERVER_INFO.md))

## License

Same as main project - MIT License

