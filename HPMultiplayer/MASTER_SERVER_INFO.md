# Master Server Information

## Overview

The mod includes a master server client that allows players to:
- Register their hosted games
- Browse available games
- Join games without needing to know IP addresses

## Current Implementation

The `MasterServerClient` is configured to connect to a master server, but **you need to host your own master server** for this to work.

### Default Configuration

- **Master Server Address**: `hpmultiplayer-master.herokuapp.com` (placeholder)
- **Master Server Port**: `27015`

This is currently a placeholder. You'll need to either:

1. **Host your own master server** (recommended)
2. **Use Steam Lobbies** (requires Steam SDK integration)
3. **Use a third-party service**

## Master Server Protocol

The master server uses a simple UDP-based protocol:

### Client → Master Server Messages

- `REGISTER|name|ip|port|steamid` - Register a game server
- `UNREGISTER|steamid` - Unregister a game server
- `LIST` - Request list of available servers

### Master Server → Client Messages

- `SERVERLIST|name1|ip1|port1|name2|ip2|port2|...` - List of available servers
- `ACK` - Acknowledgment of registration

## Hosting Your Own Master Server

You can create a simple master server using:

1. **Python** - Simple UDP server
2. **Node.js** - UDP server
3. **C#** - .NET UDP server (matches the client code)

### Example Python Master Server

```python
import socket
import threading
import time

servers = {}
TIMEOUT = 15

def handle_message(data, addr):
    message = data.decode('utf-8')
    parts = message.split('|')
    
    if parts[0] == 'REGISTER':
        server_id = parts[4]  # steamid
        servers[server_id] = {
            'name': parts[1],
            'ip': parts[2],
            'port': parts[3],
            'last_update': time.time()
        }
        sock.sendto(b'ACK', addr)
    
    elif parts[0] == 'UNREGISTER':
        server_id = parts[1]
        if server_id in servers:
            del servers[server_id]
    
    elif parts[0] == 'LIST':
        # Remove stale servers
        current_time = time.time()
        to_remove = [sid for sid, s in servers.items() 
                     if current_time - s['last_update'] > TIMEOUT]
        for sid in to_remove:
            del servers[sid]
        
        # Build server list
        server_list = ['SERVERLIST']
        for server in servers.values():
            server_list.extend([server['name'], server['ip'], server['port']])
        
        response = '|'.join(server_list)
        sock.sendto(response.encode('utf-8'), addr)

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(('0.0.0.0', 27015))

while True:
    data, addr = sock.recvfrom(1024)
    handle_message(data, addr)
```

## Alternative: Steam Integration

For a more robust solution, consider integrating with Steam's lobby system:

1. **Steamworks.NET** - C# wrapper for Steam API
2. **Steam Lobbies** - Built-in matchmaking
3. **Steam Networking** - NAT traversal and relay

This would require:
- Steam SDK
- Steamworks.NET DLL
- Steam API key

## Testing Without Master Server

If you don't have a master server set up, players can still:
- Use direct IP connection (available in both UIs)
- Use the server browser's "Direct Connect" section
- Share IP addresses manually

The master server is optional - the mod works fine with direct IP connections!

