# HP Multiplayer Mod

A multiplayer mod for House Party that allows multiple players to play together in the same game world. Supports both P2P hosting and dedicated servers.

**Author:** l0cks-acab

## Features

- **P2P Networking**: Direct player-to-player connection using UDP (2 players)
- **Dedicated Server**: Standalone server application for hosting (up to 16 players)
- **Player Synchronization**: Real-time position and rotation sync (~30 Hz)
- **Server Browser**: Browse and join available games (Press B)
- **Master Server Support**: Automatic game registration and discovery
- **Player Nametags**: See player names above remote players
- **Scene Change Handling**: Automatic player object recreation when changing scenes

## Quick Start

### For Players (Client Installation)

1. **Install MelonLoader:**
   - Download from: https://github.com/LavaGang/MelonLoader/releases
   - Run the installer and select `HouseParty.exe`
   - Launch House Party once to initialize MelonLoader

2. **Install the Mod:**
   - Download `HPMultiplayer.dll` from [Release Build](https://github.com/l0cks-acab/HP-Multiplayer/tree/main/HPMultiplayer/obj/Release) or build from source
   - Copy `HPMultiplayer.dll` to: `Steam\steamapps\common\House Party\Mods\`
   - Launch the game

3. **Playing:**
   - Press **M** to open the multiplayer UI
   - **Host:** Click "Host Game" (for P2P mode)
   - **Join:** Enter server IP:port and click "Join Game"
   - Press **B** to open the server browser

### For Server Hosts (Dedicated Server)

See [SERVER_SETUP.md](HPMultiplayer/SERVER_SETUP.md) for detailed instructions.

**Quick setup:**
1. Build the server: `msbuild Server\HPMultiplayer.Server.csproj /p:Configuration=Release`
2. Run: `Server\bin\Release\HPMultiplayer.Server.exe -port 7777 -maxplayers 16`
3. Players connect using your server's IP address and port

**For Linux server setup:** See [LINUX_SERVER_LAUNCH.md](LINUX_SERVER_LAUNCH.md)

## Server Modes

### P2P Mode (Default)
- One player hosts from their game
- Up to 2 players
- Simple setup, no server needed
- Press M → "Host Game"

### Dedicated Server Mode
- Standalone server application
- Up to 16 players
- Better for public servers
- Runs independently (headless)
- See [SERVER_SETUP.md](HPMultiplayer/SERVER_SETUP.md) for setup

## Building from Source

### Prerequisites

1. **House Party** installed via Steam
2. **MelonLoader** installed in House Party
3. **Visual Studio 2019+** or Build Tools with .NET Framework 4.7.2 support
4. **House Party DLLs** - Start House Party once with MelonLoader to generate required DLLs

### Building the Client Mod

1. Update DLL paths in `HPMultiplayer.csproj` to match your Steam installation:
   ```
   Replace: ..\..\..\Steam\steamapps\common\House Party\
   With: C:\Program Files (x86)\Steam\steamapps\common\House Party\
   ```

2. Build:
   ```bash
   # Using Visual Studio
   Open HPMultiplayer.sln → Build → Build Solution
   
   # Using MSBuild
   msbuild HPMultiplayer.csproj /p:Configuration=Release
   ```

3. Output: `obj\Release\HPMultiplayer.dll`

### Building the Server

1. Build the server project:
   ```bash
   msbuild Server\HPMultiplayer.Server.csproj /p:Configuration=Release
   ```

2. Output: `Server\bin\Release\HPMultiplayer.Server.exe`

## Usage

### Hosting a Game (P2P)

1. Press **M** to open the multiplayer UI
2. Click **"Host Game"**
3. Share your IP address with friends
4. Default port: 7777

### Joining a Game

1. Press **M** to open the multiplayer UI
2. Enter the host/server IP address and port
3. Click **"Join Game"**

### Server Browser

1. Press **B** to open the server browser
2. Click **"Refresh"** to find available games
3. Double-click a server or click **"Join"** to connect

### Playing Together

- Players appear as colored models in the game world
- Positions sync in real-time (~30 updates per second)
- Make sure both players are in the game world (not in menus) and in the same scene
- Scene changes are handled automatically

## Port Forwarding

If hosting from behind a router, forward:
- **P2P Mode:** Ports 7777 (UDP) and 7778 (UDP)
- **Dedicated Server:** Port 7777 (UDP)

Alternatively, use a VPN (like Hamachi) for easier connections.

## Troubleshooting

- **Can't connect:** Check firewall settings, ensure ports are open, verify IP address
- **Players not appearing:** Ensure both players have the mod installed and are in-game
- **Server browser shows no games:** Master server may not be available (use direct IP connection)
- **Connection drops:** Check network stability

For detailed troubleshooting, see the [main documentation](HPMultiplayer/README.md).

## Documentation

- **[Client Mod README](HPMultiplayer/README.md)** - Full mod documentation
- **[Server Setup Guide](HPMultiplayer/SERVER_SETUP.md)** - Dedicated server installation and configuration
- **[Linux Server Launch Guide](LINUX_SERVER_LAUNCH.md)** - Running the server on Linux
- **[Master Server Info](HPMultiplayer/MASTER_SERVER_INFO.md)** - Setting up a master server for game discovery
- **[Distribution Guide](HPMultiplayer/DISTRIBUTION.md)** - Sharing and distributing the mod

## License

MIT License - Feel free to use and modify!
