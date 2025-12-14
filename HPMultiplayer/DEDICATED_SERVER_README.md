# HP Multiplayer Dedicated Server

This document explains how to build and run the dedicated server for HP Multiplayer.

## Overview

The dedicated server is a **headless console application** that can run independently of the game client. This allows you to:
- Host servers on dedicated hardware (like AMP)
- Support more than 2 players (up to 16 by default)
- Keep servers running 24/7 without needing a player to host
- Better performance and reliability

## Building the Server

### Prerequisites
- Visual Studio 2019 or later (or Build Tools)
- .NET Framework 4.6.1 or later

### Build Steps

1. **Open the solution:**
   ```
   HPMultiplayer.sln
   ```

2. **Build the Server project:**
   - In Visual Studio: Right-click `HPMultiplayer.Server` → Build
   - Or use MSBuild: `msbuild Server\HPMultiplayer.Server.csproj /p:Configuration=Release`

3. **Output location:**
   - Debug: `Server\bin\Debug\HPMultiplayer.Server.exe`
   - Release: `Server\bin\Release\HPMultiplayer.Server.exe`

## Running the Server

### Basic Usage

```bash
HPMultiplayer.Server.exe
```

This will start the server on the default port **7777** with a maximum of **16 players**.

### Command Line Arguments

```bash
HPMultiplayer.Server.exe -port 7777 -maxplayers 16
```

Options:
- `-port <number>` - Server port (default: 7777)
- `-maxplayers <number>` - Maximum players (default: 16)
- `-help` or `-h` - Show help message

### Server Commands

While the server is running, you can type commands in the console:

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
players
Connected players (2):
  [1] Player - 192.168.1.100:52341
  [2] John - 192.168.1.101:52342
status
Server Status:
  Running: True
  Players: 2 / 16
  Port: 7777
```

## Connecting to the Server

### From the Game Client

1. **Press `M`** to open the multiplayer UI
2. **Select "Connect to Server"** (new option when using dedicated server mode)
3. Enter the server IP address and port
4. Click "Connect"

The client mod will automatically use `ClientNetworkManager` when connecting to a dedicated server, and `NetworkManager` (P2P) when hosting directly.

## Port Forwarding

If hosting from behind a router, you need to forward the server port:

- **Default port:** 7777 (UDP)
- Forward this port to your server's local IP address

## Running on AMP (Application Management Panel)

### Setup Steps

1. **Upload the server files:**
   - Copy `HPMultiplayer.Server.exe` to your AMP server directory
   - Copy any required DLLs (should be none - server is self-contained)

2. **Create a Generic Application in AMP:**
   - Application Type: `Generic Application`
   - Executable: `HPMultiplayer.Server.exe`
   - Arguments: `-port 7777 -maxplayers 16`
   - Working Directory: Path to server files

3. **Configure Network:**
   - Ensure port 7777 (or your chosen port) is open
   - AMP should handle port forwarding if configured

4. **Start the server:**
   - Use AMP's control panel to start the server
   - Monitor logs in AMP's console

### AMP Configuration Example

```
Application Name: HP Multiplayer Server
Type: Generic Application
Executable: HPMultiplayer.Server.exe
Startup Parameters: -port 7777 -maxplayers 16
Working Directory: C:\AMP\HPMultiplayerServer\
Auto Start: Enabled (optional)
Auto Restart: Enabled (optional)
```

## Server Features

### Current Features
- ✅ Multiple client connections (up to 16 players)
- ✅ Player position synchronization
- ✅ Player join/leave notifications
- ✅ Connection timeout handling
- ✅ Game state broadcasting (framework ready)
- ✅ Console commands for management

### Future Enhancements
- [ ] Configurable server settings file
- [ ] Password protection
- [ ] Server name/motd
- [ ] Player authentication
- [ ] Game state persistence
- [ ] Logging to file
- [ ] Web-based admin panel
- [ ] Statistics tracking

## Troubleshooting

### Server won't start
- Check if port 7777 is already in use: `netstat -an | findstr 7777`
- Run as administrator if needed
- Check Windows Firewall settings

### Players can't connect
- Verify server is running and listening on the correct port
- Check firewall rules (Windows Firewall, router firewall)
- Ensure port forwarding is configured if hosting remotely
- Verify server IP address is correct

### Players disconnect frequently
- Check network stability
- Increase timeout values in code if needed
- Check server CPU/memory usage

### Server crashes
- Check Windows Event Viewer for errors
- Ensure .NET Framework 4.6.1+ is installed
- Review server console for error messages

## Server vs P2P Comparison

| Feature | Dedicated Server | P2P (Current) |
|---------|-----------------|---------------|
| Max Players | 16 (configurable) | 2 |
| Host Required | No | Yes (one player hosts) |
| Server Stability | High | Depends on host |
| Setup Complexity | Medium | Low |
| Resource Usage | Server hardware | Player's computer |
| Best For | Public servers, large groups | Private games with friends |

## Technical Details

### Architecture
- **Protocol:** UDP
- **Tick Rate:** 30 updates per second
- **Connection Timeout:** 30 seconds
- **Message Types:** Connection, PlayerUpdate, GameState, Disconnect

### Network Protocol
The server uses the same protocol as the P2P mode for compatibility. See `NetworkProtocol.cs` for message format details.

### Server Performance
- CPU: Minimal (single-threaded)
- Memory: ~50-100 MB base + ~5 MB per player
- Network: ~1-2 KB/s per player (30 updates/sec)

## License

Same as the main mod - MIT License

