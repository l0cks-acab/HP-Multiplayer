# HP Multiplayer Mod

A P2P (Peer-to-Peer) multiplayer mod for House Party that allows multiple players to play together in the same game world.

**Author:** l0cks-acab

## Features

- **P2P Networking**: Direct player-to-player connection using UDP
- **Dedicated Server Support**: Standalone server application for hosting (supports up to 16 players)
- **Player Synchronization**: Real-time position and rotation sync
- **Host/Join**: Easy hosting and joining of multiplayer sessions
- **Server Browser**: Browse and join available games (Press B)
- **Master Server Support**: Automatic game registration and discovery
- **Connection Retry**: Automatic retry logic for reliable connections
- **Player Nametags**: See player names above remote players
- **Scene Change Handling**: Automatic player object recreation when changing scenes/levels
- **Simple UI**: Press M to open the multiplayer menu, Press B for server browser

### Server Modes

- **P2P Mode** (Default): One player hosts, another joins directly (2 players max)
- **Dedicated Server Mode**: Connect to a standalone server (up to 16 players)

See [SERVER_SETUP.md](SERVER_SETUP.md) for dedicated server setup.

## Installation

**Quick Install:**
1. Install MelonLoader in House Party (download from https://github.com/LavaGang/MelonLoader/releases)
2. Download `HPMultiplayer.dll` from [Release Build](https://github.com/l0cks-acab/HP-Multiplayer/tree/main/HPMultiplayer/obj/Release)
3. Copy `HPMultiplayer.dll` to `Steam\steamapps\common\House Party\Mods\`
4. Launch the game

**Building from Source:**
See the main [README.md](../README.md) for detailed build instructions.

## Usage

1. **Hosting a Game**:
   - Press `M` to open the multiplayer UI
   - Click "Host Game"
   - Your game will automatically register with the master server (if available)
   - Share your IP address with friends, or they can find you in the server browser
   - Port will default to 7777

2. **Joining a Game** (Server Browser):
   - Press `B` to open the server browser
   - Click "Refresh" to find available games
   - Double-click a server or click "Join" to connect
   - Games are automatically discovered from the master server

3. **Joining a Game** (Direct IP):
   - Press `M` to open the multiplayer UI
   - Enter the host's IP address
   - Enter the port (default: 7777)
   - Click "Join Game"
   - Or use the "Direct Connect" section in the server browser (Press B)

4. **Playing Together**:
   - Once connected, synchronization happens automatically!
   - You'll see your friend as a colored capsule (or game model if found) in the game world:
     - **Blue capsule** = Player 2 (the person who joined)
     - **Green capsule** = Player 1 (the host)
   - Player nametags appear above remote players showing "Player 1" or "Player 2"
   - Both players' positions sync in real-time (~30 updates per second)
   - Make sure both players are in the game world (not in menus) and in the same scene/level
   - The remote player representation will appear and move to match your friend's position
   - Scene changes are handled automatically - player objects will be recreated when moving between levels

5. **Port Forwarding**:
   - If hosting from behind a router, you'll need to forward port 7777 (and 7778)
   - Or use a VPN/hamachi for easier connection

## Current Status

This is a **foundation/prototype** for multiplayer. Currently implemented:

✅ **Networking & Connection**
- Basic UDP P2P networking
- Player connection/disconnection
- Connection retry logic for reliable connections
- Improved connection handshake
- Server browser with game discovery
- Master server client (requires hosted master server)

✅ **Player Synchronization**
- Real-time position and rotation sync (~30 Hz)
- Player nametags above remote players
- Scene change handling (automatic player object recreation)
- Uses actual player model for remote player representations
- Smooth interpolation for remote player movement

✅ **Game State Synchronization** (Work in Progress)
- Framework for NPC and interactive object synchronization
- Periodic full sync (every 1 second) + change-based sync
- Proper interpolation pattern following Unity best practices
- Host-authoritative game state (only host sends NPC/object updates)

✅ **UI & User Experience**
- Simple UI for hosting/joining (Press M)
- Server browser (Press B)
- Copy/paste support for IP/Port fields
- Connection status indicators
- Display host IP address and port

## TODO / Next Steps

- [ ] Find actual player GameObject in House Party
- [ ] Sync player animations
- [ ] Sync NPC states (partially implemented)
- [ ] Sync interactive objects (items, doors, etc.) (partially implemented)
- [ ] Improve player representation (use actual player model instead of capsules)
- [ ] Add chat system
- [ ] Add ping/latency display
- [ ] Better error handling
- [ ] NAT traversal / punch-through for easier connections
- [ ] Host public master server or integrate Steam Lobbies
- [ ] Add server name customization when hosting

## Technical Notes

**Networking:**
- Uses raw UDP sockets for P2P communication
- Player updates sent at ~30 Hz (~33ms intervals)
- Connection handshake with automatic retry (up to 5 attempts)
- Default port: 7777 (host), 7778 (client)
- Master server uses UDP protocol for game registration/discovery

**Player Synchronization:**
- Smooth interpolation of remote player positions/rotations
- Uses actual player model from game (cloned and stripped of scripts)
- Automatic player object recreation on scene changes
- Position synchronization via PlayerModelFinder (searches for player by name/tag/camera)

**Game State Synchronization:**
- Host-authoritative architecture (only host sends NPC/object updates)
- Periodic full sync every 1 second + change-based sync (every 200ms)
- Follows Unity state synchronization best practices
- Proper interpolation pattern (network updates queued, applied in Update loop)
- Object registration via GameObjectFinder (searches by naming patterns)

**Architecture:**
- Minimal dependencies (just MelonLoader and Unity assemblies)
- Thread-safe network operations (Unity API calls queued to main thread)
- Il2Cpp-compatible (avoids stripped methods, uses manual UI layout)

## Troubleshooting

- **Can't connect**: 
  - Check firewall settings, ensure ports are open
  - Verify the connection handshake completes (check logs)
  - Try using direct IP connection instead of server browser
  - Connection will automatically retry up to 5 times

- **Players not appearing**: 
  - Verify both players have the mod installed
  - Make sure both players are in the game world (not in menus)
  - Both players should be in the same scene/level
  - Check MelonLoader logs for connection messages
  - The remote player capsule may spawn at (0,0,0) initially, then move to the correct position

- **Server browser shows no games**:
  - Master server may not be available (this is normal - use direct IP instead)
  - Click "Refresh" to update the server list
  - You can still use "Direct Connect" at the bottom of the browser
  - See `MASTER_SERVER_INFO.md` for information on hosting your own master server

- **Connection drops**: Check network stability, connection will attempt to retry automatically

    - **Can't see friend's position**: 
      - The mod tries to find the player GameObject automatically using PlayerModelFinder
      - Check logs for "Found player by name: ..." messages
      - If player object not found, it falls back to using the camera position
      - Make sure both players are in the game world (not in menus)
      - Remote player should appear as a cloned player model (or capsule if model not found)

    - **Connection says "connected" but nothing happens**: 
      - Wait a few seconds for the connection handshake to complete
      - Check that both players are actually in-game (not in menus)
      - Verify ports 7777 and 7778 are not blocked by firewall
      - Check MelonLoader logs for "Created NetworkPlayer for remote Player X" messages

    - **NPCs/interactive objects not syncing**:
      - This feature is still in development
      - Check logs for "Registered NPC for sync: ..." messages
      - NPCs must match naming patterns (contain "npc", "character", "guest", "person")
      - Only the host sends NPC updates; clients receive and apply them
      - If NPCs aren't being found, their names may not match the search patterns

## Contributing

This is an ambitious project! Contributions welcome for:
- Finding game objects/classes to sync
- Improving networking reliability
- Adding more synchronized features
- UI improvements

## License

MIT License - Feel free to use and modify!

