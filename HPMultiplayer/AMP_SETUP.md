# HP Multiplayer Server on AMP Setup Guide

Complete guide for hosting HP Multiplayer dedicated server on AMP (Application Management Panel).

## Overview

AMP is a game server management panel that simplifies hosting. This guide covers setting up the HP Multiplayer server on AMP for both Windows and Linux.

## Prerequisites

1. **AMP installed** on your server
2. **Server executable** built (see [SERVER_SETUP.md](SERVER_SETUP.md))
3. **Template files** (included in repository):
   - `hpmultiplayer.kvp`
   - `hpmultiplayerconfig.json`
   - `hpmultiplayermetaconfig.json`

## Quick Start

### Windows

1. Build the server: `msbuild Server\HPMultiplayer.Server.csproj /p:Configuration=Release`
2. Copy `Server\bin\Release\HPMultiplayer.Server.exe` to your AMP server directory
3. Upload template files to AMP
4. Create instance in AMP

### Linux

**Using Mono:**
1. Install Mono: `sudo apt-get install mono-complete`
2. Build server (same as Windows)
3. Upload `HPMultiplayer.Server.exe` to Linux server
4. Configure AMP to use `mono` as executable

**Using .NET Core:** (Requires project migration)
1. Build for Linux: `dotnet publish -r linux-x64`
2. Upload Linux build to server
3. Configure AMP to use `./HPMultiplayer.Server`

## Detailed Setup

### Step 1: Prepare Server Files

1. **Build the server** (see [SERVER_SETUP.md](SERVER_SETUP.md))
2. **Locate executable:**
   - Windows: `Server\bin\Release\HPMultiplayer.Server.exe`
   - Linux: `Server\bin\Release\HPMultiplayer.Server.exe` (for Mono) or Linux build

3. **Create server directory** on your AMP server:
   - Windows: `C:\AMP\HPMultiplayerServer\`
   - Linux: `/home/amp/instances/hpmultiplayer/`

4. **Upload executable** to the server directory

### Step 2: Install Template Files

1. **Upload template files to AMP:**
   - Windows: `C:\ProgramData\AMP\Plugins\ADSModule\GenericTemplates\`
   - Linux: `/home/amp/.config/AMP/Plugins/ADSModule/GenericTemplates/`

2. **Files needed:**
   - `hpmultiplayer.kvp`
   - `hpmultiplayerconfig.json`
   - `hpmultiplayermetaconfig.json`

3. **Set permissions (Linux):**
   ```bash
   chmod 644 hpmultiplayer.*
   ```

4. **Restart AMP:**
   - Windows: Restart AMP service
   - Linux: `sudo systemctl restart amp`

### Step 3: Install Dependencies (Linux Only)

**For Mono:**
```bash
sudo apt-get update
sudo apt-get install mono-complete mono-devel
```

**For .NET Core:**
```bash
# Install .NET 6.0 Runtime
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-runtime-6.0
```

### Step 4: Create Instance in AMP

1. **Login to AMP** web interface

2. **Create New Instance:**
   - Click "Create Instance"
   - Select "HP Multiplayer Server" from template list
   - Choose a name for your instance
   - Select platform (Windows/Linux)

3. **Configure Instance:**

   **For Windows:**
   - **Executable:** `HPMultiplayer.Server.exe`
   - **Working Directory:** Path to server folder
   - **Startup Parameters:** `-port ${Port} -maxplayers ${MaxPlayers}`

   **For Linux (Mono):**
   - **Executable:** `mono`
   - **Working Directory:** Path to server folder
   - **Startup Parameters:** `HPMultiplayer.Server.exe -port ${Port} -maxplayers ${MaxPlayers}`

   **For Linux (.NET Core):**
   - **Executable:** `./HPMultiplayer.Server`
   - **Working Directory:** Path to server folder
   - **Startup Parameters:** `-port ${Port} -maxplayers ${MaxPlayers}`

4. **Configure Settings:**
   - **Server Port:** 7777 (or your preferred port)
   - **Maximum Players:** 16 (or your preferred limit)

5. **Configure Network:**
   - AMP should automatically detect port 7777 (UDP)
   - Verify port forwarding if hosting remotely

### Step 5: Configure Firewall

**Windows:**
```powershell
New-NetFirewallRule -DisplayName "HP Multiplayer Server" -Direction Inbound -Protocol UDP -LocalPort 7777 -Action Allow
```

**Linux:**
```bash
sudo ufw allow 7777/udp
```

### Step 6: Test the Server

1. **Start the instance** from AMP web interface
2. **Monitor console output:**
   - Should see: "Server started successfully on port 7777"
   - Status should show "Ready"

3. **Test connection:**
   - From game client, connect to server IP:port
   - Verify players can connect and sync

## AMP Configuration Details

### Recommended Settings

- **Auto Start:** Enabled (server starts when AMP starts)
- **Auto Restart:** Enabled (restarts if server crashes)
- **Restart Delay:** 5 seconds
- **CPU Priority:** Normal (or High if dedicated server)
- **Memory Limit:** 512 MB (should be plenty)

### Custom Port

To use a different port:
1. Update **Startup Parameters** to: `-port 12345 -maxplayers 16`
2. Update firewall rule to allow the new port
3. Update port forwarding (if applicable)
4. Update AMP port configuration

### Maximum Players

To allow more/fewer players:
1. Update **Startup Parameters** to: `-port 7777 -maxplayers 32`
2. Keep in mind server performance and network bandwidth

### Scheduled Restarts

Configure scheduled tasks in AMP to restart the server:
- Useful for daily maintenance
- Prevents memory leaks over long uptime
- Can run at low-traffic times

## Troubleshooting

### Server Won't Start

**Check logs:**
- View AMP instance logs
- Check for missing dependencies (Mono/.NET)
- Verify executable path is correct

**Common issues:**
- Missing Mono runtime on Linux
- Incorrect executable path
- Port already in use
- Missing execute permissions (Linux: `chmod +x`)

### Template Not Appearing

1. Verify file names are lowercase: `hpmultiplayer.*`
2. Check file location in GenericTemplates directory
3. Restart AMP after adding templates
4. Check AMP logs for template loading errors

### Server Starts But Players Can't Connect

1. Check firewall:
   ```bash
   # Linux
   sudo ufw status
   sudo ufw allow 7777/udp
   ```

2. Verify port in settings matches actual port
3. Check server console for connection attempts
4. Test locally first before testing remotely
5. Verify port forwarding if hosting remotely

### High CPU/Memory Usage

1. Reduce max players in startup parameters
2. Check for memory leaks (restart daily if needed)
3. Monitor individual player impact

## Monitoring

### AMP Console

- View server logs in real-time
- See player connections/disconnections
- Monitor for errors

### Server Commands

Connect to server console and use:
- `status` - Show server status
- `players` - List connected players
- `help` - Show all commands

### Resource Usage

Monitor in AMP:
- **CPU Usage:** Should be low (<5% for idle server)
- **Memory Usage:** ~50-100 MB base + ~5 MB per player
- **Network:** ~1-2 KB/s per connected player

## Contributing Template to AMP Repository

To submit to [AMPTemplates repository](https://github.com/CubeCoders/AMPTemplates):

1. Test thoroughly on both Windows and Linux
2. Follow repository guidelines:
   - Files must be lowercase
   - No directories
   - Only include `.kvp`, `*config.json`, and `*metaconfig.json`
3. Create pull request

## References

- [AMP Generic Module Documentation](https://github.com/CubeCoders/AMP/wiki/Configuring-the-'Generic'-AMP-module)
- [AMP Templates Repository](https://github.com/CubeCoders/AMPTemplates)
- [Server Setup Guide](SERVER_SETUP.md)

## License

Same as main project - MIT License

