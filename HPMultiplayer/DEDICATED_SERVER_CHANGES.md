# Dedicated Server Implementation Summary

This document summarizes the changes made to add dedicated server support to HP Multiplayer.

## Overview

The mod now supports **two networking modes**:
1. **P2P Mode** (original) - One player hosts, supports 2 players
2. **Dedicated Server Mode** (new) - Standalone server, supports up to 16 players, suitable for AMP hosting

## Architecture Changes

### New Components

1. **NetworkProtocol.cs** - Shared protocol definitions (no Unity dependencies)
   - Message type enums
   - Message serialization/deserialization
   - Vector3F struct (Unity-independent Vector3 replacement)

2. **ClientNetworkManager.cs** - Client-side network manager for dedicated servers
   - Connects to dedicated server instead of P2P
   - Handles server-assigned player IDs
   - Processes server broadcasts (player join/leave, game state)

3. **Server Project** - New console application
   - `Program.cs` - Server entry point and console interface
   - `ServerNetworkManager.cs` - Server-side network handling (multiple clients)
   - `ServerPlayer.cs` - Server-side player representation
   - `GameStateManager.cs` - Server-side game state management

### Modified Components

- **NetworkManager.cs** - Still exists for P2P mode (backward compatible)
- **HPMultiplayer.csproj** - Added new source files
- **HPMultiplayer.sln** - Added server project

### Protocol Changes

The protocol now includes additional message types:
- `ConnectionAccepted` - Server assigns player ID
- `ConnectionRejected` - Server rejects connection (server full, etc.)
- `PlayerJoined` - Broadcast when player joins
- `PlayerLeft` - Broadcast when player leaves
- `ServerInfo` - Server information (for future use)

## File Structure

```
HPMultiplayer/
├── Networking/
│   ├── NetworkManager.cs          (P2P mode - unchanged)
│   ├── ClientNetworkManager.cs    (NEW - dedicated server client)
│   ├── NetworkProtocol.cs         (NEW - shared protocol)
│   └── ...
├── Server/                         (NEW - dedicated server)
│   ├── Program.cs
│   ├── ServerNetworkManager.cs
│   ├── ServerPlayer.cs
│   ├── GameStateManager.cs
│   └── HPMultiplayer.Server.csproj
├── DEDICATED_SERVER_README.md      (NEW)
├── AMP_SETUP_GUIDE.md              (NEW)
└── ...
```

## How It Works

### P2P Mode (Original)
1. Player A clicks "Host Game" → Starts NetworkManager in host mode
2. Player B enters Player A's IP → Connects via NetworkManager in client mode
3. Direct UDP connection between the two players

### Dedicated Server Mode (New)
1. Server administrator runs `HPMultiplayer.Server.exe`
2. Server listens on port 7777 (configurable)
3. Players connect to server using ClientNetworkManager
4. Server assigns player IDs and broadcasts updates to all clients
5. Server manages game state authoritatively

## Key Differences

| Aspect | P2P Mode | Dedicated Server |
|--------|----------|------------------|
| Max Players | 2 | 16 (configurable) |
| Host Required | Yes (must be in-game) | No (separate process) |
| Stability | Depends on host | High (dedicated) |
| Setup | Simple | Requires server setup |
| Best For | Friends playing together | Public servers, communities |

## Backward Compatibility

- **P2P mode is still fully functional** - existing functionality unchanged
- Both modes can coexist - players can choose which to use
- The mod will need UI updates to allow choosing between modes (future work)

## Next Steps (Future Enhancements)

1. **UI Integration** - Add toggle/option to choose between P2P and dedicated server
2. **Auto-Detection** - Detect if connecting to dedicated server vs P2P host
3. **Server Browser Integration** - Show dedicated servers separately from P2P hosts
4. **Advanced Server Features**:
   - Password protection
   - Server configuration file
   - Player authentication
   - Game state persistence
   - Admin commands

## Building

### Client Mod (unchanged)
```bash
msbuild HPMultiplayer.csproj /p:Configuration=Release
```

### Dedicated Server (new)
```bash
msbuild Server\HPMultiplayer.Server.csproj /p:Configuration=Release
```

Output: `Server\bin\Release\HPMultiplayer.Server.exe`

## Testing

1. **Build both projects**
2. **Start server**: Run `HPMultiplayer.Server.exe`
3. **Connect client**: Use ClientNetworkManager to connect (UI integration pending)
4. **Verify**: Players should see each other and positions should sync

## AMP Hosting

See `AMP_SETUP_GUIDE.md` for detailed instructions on setting up the server on AMP.

The server is designed to be:
- Headless (no graphics required)
- Lightweight (~50-100 MB memory)
- Easy to configure (command-line arguments)
- Suitable for 24/7 hosting

## Technical Notes

- **Protocol**: UDP-based, same as P2P mode
- **Tick Rate**: 30 updates per second
- **Connection Timeout**: 30 seconds
- **Thread Safety**: Uses locks for player dictionary access
- **No Unity Dependencies**: Server code is pure .NET, no Unity/Game dependencies

## Known Limitations

1. **UI Integration Pending**: Need to add UI for choosing server mode
2. **Game State Sync**: Basic framework exists, but full game state sync needs more work
3. **Player Names**: Currently generic "Player X", player name support partially implemented
4. **Error Handling**: Basic error handling, could be improved
5. **Logging**: Console logging only, file logging not implemented

## License

Same as main project - MIT License

