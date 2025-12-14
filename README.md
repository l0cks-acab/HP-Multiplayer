# HP Multiplayer Mod

A P2P (Peer-to-Peer) multiplayer mod for House Party that allows multiple players to play together in the same game world.

**Author:** l0cks-acab

## Features

- **P2P Networking**: Direct player-to-player connection using UDP
- **Player Synchronization**: Real-time position and rotation sync
- **Host/Join**: Easy hosting and joining of multiplayer sessions
- **Server Browser**: Browse and join available games (Press B)
- **Master Server Support**: Automatic game registration and discovery
- **Connection Retry**: Automatic retry logic for reliable connections
- **Simple UI**: Press M to open the multiplayer menu, Press B for server browser

## Installation

1. Build the mod using Visual Studio (targets .NET Framework 4.7.2)
2. Copy `HPMultiplayer.dll` to `Steam\steamapps\common\House Party\Mods\`
3. Make sure MelonLoader is installed in House Party
4. Launch the game

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
   - You'll see your friend as a colored capsule in the game world:
     - **Blue capsule** = Player 2 (the person who joined)
     - **Green capsule** = Player 1 (the host)
   - Both players' positions sync in real-time (~30 updates per second)
   - Make sure both players are in the game world (not in menus) and in the same scene/level
   - The remote player capsule will appear and move to match your friend's position

5. **Port Forwarding**:
   - If hosting from behind a router, you'll need to forward port 7777 (and 7778)
   - Or use a VPN/hamachi for easier connection

## Current Status

This is a **foundation/prototype** for multiplayer. Currently implemented:

✅ Basic UDP networking
✅ Player connection/disconnection
✅ Position synchronization
✅ Simple UI for hosting/joining
✅ Server browser with game discovery
✅ Master server client (requires hosted master server)
✅ Connection retry logic for reliable connections
✅ Improved connection handshake

## TODO / Next Steps

- [ ] Find actual player GameObject in House Party
- [ ] Sync player animations
- [ ] Sync NPC states
- [ ] Sync interactive objects (items, doors, etc.)
- [ ] Improve player representation (use actual player model)
- [ ] Add name tags above players
- [ ] Add chat system
- [ ] Handle scene changes
- [ ] Add ping/latency display
- [ ] Better error handling
- [ ] NAT traversal / punch-through for easier connections
- [ ] Host public master server or integrate Steam Lobbies
- [ ] Add server name customization when hosting

## Technical Notes

- Uses raw UDP sockets for P2P communication
- Player updates sent at ~30 Hz
- Smooth interpolation of remote player positions
- Minimal dependencies (just MelonLoader and Unity assemblies)
- Master server uses UDP protocol for game registration/discovery
- Connection handshake with automatic retry (up to 5 attempts)
- Default port: 7777 (host), 7778 (client)

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

- **Can't see friend's position**: The mod tries to find the player GameObject automatically. If it can't find it, it falls back to using the camera position.

- **Connection says "connected" but nothing happens**: 
  - Wait a few seconds for the connection handshake to complete
  - Check that both players are actually in-game (not in menus)
  - Verify ports 7777 and 7778 are not blocked by firewall

## Contributing

This is an ambitious project! Contributions welcome for:
- Finding game objects/classes to sync
- Improving networking reliability
- Adding more synchronized features
- UI improvements

## License

MIT License - Feel free to use and modify!

