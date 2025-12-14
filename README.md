# HP Multiplayer Mod

A P2P (Peer-to-Peer) multiplayer mod for House Party that allows multiple players to play together in the same game world.

**Author:** l0cks-acab

## Features

- **P2P Networking**: Direct player-to-player connection using UDP
- **Player Synchronization**: Real-time position and rotation sync
- **Host/Join**: Easy hosting and joining of multiplayer sessions
- **Simple UI**: Press M to open the multiplayer menu

### Prerequisites
1. **House Party** installed via Steam
2. **MelonLoader** installed in House Party
   - Download from: https://github.com/LavaGang/MelonLoader/releases
   - Run the installer and select `HouseParty.exe`
   - Launch the game once to initialize MelonLoader

## Installation

1. Copy `HPMultiplayer.dll` to `Steam\steamapps\common\House Party\Mods\`
2. Launch the game

## Usage

1. **Hosting a Game**:
   - Press `M` to open the multiplayer UI
   - Click "Host Game"
   - Share your IP address with friends
   - Port will default to 7777

2. **Joining a Game**:
   - Press `M` to open the multiplayer UI
   - Enter the host's IP address
   - Enter the port (default: 7777)
   - Click "Join Game"

3. **Playing Together**:
   - Once connected, synchronization happens automatically!
   - You'll see your friend as a colored capsule in the game world:
     - **Blue capsule** = Player 2 (the person who joined)
     - **Green capsule** = Player 1 (the host)
   - Both players' positions sync in real-time (~30 updates per second)
   - Make sure both players are in the game world (not in menus) and in the same scene/level
   - The remote player capsule will appear and move to match your friend's position

4. **Port Forwarding**:
   - If hosting from behind a router, you'll need to forward port 7777 (and 7778)
   - Or use a VPN/hamachi for easier connection

## Current Status

This is a **foundation/prototype** for multiplayer. Currently implemented:

✅ Basic UDP networking
✅ Player connection/disconnection
✅ Position synchronization
✅ Simple UI for hosting/joining

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

## Technical Notes

- Uses raw UDP sockets for P2P communication
- Player updates sent at ~30 Hz
- Smooth interpolation of remote player positions
- Minimal dependencies (just MelonLoader and Unity assemblies)

## Troubleshooting

- **Can't connect**: Check firewall settings, ensure ports are open
- **Players not appearing**: 
  - Verify both players have the mod installed
  - Make sure both players are in the game world (not in menus)
  - Both players should be in the same scene/level
  - Check MelonLoader logs for connection messages
  - The remote player capsule may spawn at (0,0,0) initially, then move to the correct position
- **Connection drops**: Check network stability, may need better error handling
- **Can't see friend's position**: The mod tries to find the player GameObject automatically. If it can't find it, it falls back to using the camera position.

## Contributing

This is an ambitious project! Contributions welcome for:
- Finding game objects/classes to sync
- Improving networking reliability
- Adding more synchronized features
- UI improvements

## License

MIT License - Feel free to use and modify!

