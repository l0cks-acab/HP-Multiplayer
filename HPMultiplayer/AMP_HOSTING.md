# Hosting House Party Multiplayer on AMP

## Current Architecture

This mod uses **P2P (Peer-to-Peer) networking**, where:
- The **host** is one of the players running the game
- The mod runs **inside House Party** using MelonLoader (a Unity mod loader)
- There is **no dedicated server executable** - the host's game instance acts as the server
- Players connect directly to the host's IP address via UDP (ports 7777/7778)

## Challenges with AMP Hosting

AMP (Application Management Panel) is designed for hosting **dedicated server applications**, which are typically:
- Headless (no graphics rendering required)
- Designed to run as standalone executables
- Built specifically for server environments

**House Party Multiplayer mod is NOT a dedicated server:**
- Requires the full House Party game to run
- Needs Unity rendering engine (graphics card/driver)
- Requires MelonLoader to be installed in the game
- The host must actually "play" the game to host

## Potential Workarounds

### Option 1: AMP Generic Application (Limited Success)

If AMP supports running generic Windows applications, you *might* be able to:

1. Install Steam on the AMP server
2. Install House Party via Steam
3. Install MelonLoader in House Party
4. Copy the mod DLL to the Mods folder
5. Configure AMP to launch `HouseParty.exe`

**Limitations:**
- House Party requires graphics rendering (DX11/OpenGL)
- The server would need a GPU or software rendering (very slow)
- You'd need to keep Steam logged in
- The host player would still need to control/play the game
- Not a true "dedicated server" - still requires a player presence

### Option 2: Dedicated Server Conversion (Requires Development)

To truly support AMP hosting, the mod would need to be refactored:

1. **Split architecture**: Separate client and server code
2. **Create standalone server executable**: A headless server that doesn't require the game
3. **Extract game state management**: Server manages NPCs, objects, game state
4. **Client connects to server**: Instead of P2P, clients connect to dedicated server
5. **Remove Unity dependencies**: Server should not require Unity rendering

This would require significant code changes:
- Separate `NetworkManager` into `ServerNetworkManager` and `ClientNetworkManager`
- Create a server-only executable (console app)
- Extract game state synchronization to server
- Handle server authority for game objects
- Create a proper server-client protocol

### Option 3: Use a VPS/Dedicated Server Instead

Instead of AMP, you could:

1. **Rent a Windows VPS** (AWS, Azure, DigitalOcean, etc.)
2. Install Steam and House Party
3. Install MelonLoader
4. Install the mod
5. Use Remote Desktop to control and host
6. Use port forwarding to expose port 7777

**Note:** This still has the same limitations - you'd be running the full game on a server, which isn't ideal.

### Option 4: Current P2P Solution (Recommended)

The current design works best when:
- One player hosts from their own computer
- Players connect directly via IP address
- Port forwarding is configured if needed (ports 7777/7778)
- Or use VPN/Hamachi for easier connections

## Master Server Alternative

The mod includes a **master server client** for game discovery. You could:

1. **Host a master server separately** (this IS suitable for AMP!)
   - The master server is just a simple UDP server (see `MASTER_SERVER_INFO.md`)
   - Can be written in Python, Node.js, or C#
   - This could easily run on AMP as a generic application
   - Players register their games and browse available games

2. **Benefits:**
   - Makes finding games easier
   - Doesn't require hosting the full game
   - Can run on any server platform
   - Simple UDP server implementation

See `MASTER_SERVER_INFO.md` for details on hosting the master server.

## Recommendations

**For now (P2P architecture):**
- Keep using direct IP connections
- One player hosts from their computer
- Use port forwarding or VPN for connectivity
- Host the master server separately if you want server discovery (this CAN run on AMP)

**For true AMP support (future):**
- Consider refactoring to dedicated server architecture
- Split client and server code
- Create a headless server executable
- This would be a significant rewrite of the networking layer

## Summary

**Short answer:** Hosting the House Party multiplayer server on AMP is **not practical** with the current P2P architecture because:
- The mod runs inside the game (requires full House Party installation)
- No standalone server executable exists
- Requires graphics rendering (not suitable for headless servers)

**Alternative:** You CAN host the **master server** on AMP (the game discovery server), which is a simple UDP application that helps players find games. This is much more suitable for AMP hosting.

**Long-term solution:** Refactor to dedicated server architecture if you want true AMP hosting support.
