# Setting Up HP Multiplayer Server on AMP

This guide will walk you through setting up the HP Multiplayer dedicated server on AMP (Application Management Panel).

## Prerequisites

1. **AMP installed and configured** on your server
2. **Windows Server** or Windows with .NET Framework 4.6.1+
3. **Server executable** built from source (see DEDICATED_SERVER_README.md)

## Step-by-Step Setup

### Step 1: Build the Server

1. Build the server project (see `DEDICATED_SERVER_README.md`)
2. Locate `HPMultiplayer.Server.exe` in `Server\bin\Release\`

### Step 2: Upload to Server

1. Create a folder on your server for the HP Multiplayer server (e.g., `C:\AMP\HPMultiplayerServer\`)
2. Upload `HPMultiplayer.Server.exe` to this folder
3. Verify the file is in place

### Step 3: Create Generic Application in AMP

1. **Login to AMP** and navigate to your instance
2. **Create a new application:**
   - Click "Create Instance" or "New Application"
   - Select **"Generic"** or **"Generic Application"** as the application type

3. **Configure the application:**
   - **Application Name:** `HP Multiplayer Server`
   - **Executable:** `HPMultiplayer.Server.exe`
   - **Working Directory:** Path to your server folder (e.g., `C:\AMP\HPMultiplayerServer\`)
   - **Startup Parameters:** `-port 7777 -maxplayers 16`
     - Adjust `-port` if you want a different port
     - Adjust `-maxplayers` for maximum concurrent players

4. **Save the configuration**

### Step 4: Configure Network/Port

1. **In AMP settings:**
   - Navigate to your instance's settings
   - Find "Network" or "Ports" section
   - **Add port:** `7777` (UDP protocol)
   - **Port Name:** `Game Server Port`
   - Save settings

2. **If hosting remotely:**
   - Configure port forwarding on your router
   - Forward UDP port 7777 to your server's local IP
   - Or use AMP's built-in port forwarding if available

### Step 5: Configure Firewall

On your Windows server:

1. Open **Windows Defender Firewall**
2. Click **"Allow an app through firewall"**
3. Add `HPMultiplayer.Server.exe` or allow port 7777 (UDP)
4. Or run this PowerShell command as Administrator:
   ```powershell
   New-NetFirewallRule -DisplayName "HP Multiplayer Server" -Direction Inbound -Protocol UDP -LocalPort 7777 -Action Allow
   ```

### Step 6: Start the Server

1. In AMP, navigate to your HP Multiplayer Server instance
2. Click **"Start"** or **"Power On"**
3. Monitor the console output to verify it started correctly
4. You should see:
   ```
   [Server] Started on port 7777
   Server started successfully on port 7777
   ```

### Step 7: Test Connection

1. **Get your server's public IP address**
2. **From the game client:**
   - Press `M` to open multiplayer UI
   - Select "Connect to Server"
   - Enter your server IP and port (e.g., `123.456.789.0:7777`)
   - Click "Connect"

## AMP Configuration Details

### Recommended Settings

- **Auto Start:** Enabled (server starts when AMP starts)
- **Auto Restart:** Enabled (restarts if server crashes)
- **Restart Delay:** 5 seconds
- **CPU Priority:** Normal (or High if dedicated server)
- **Memory Limit:** 512 MB (should be plenty)

### Advanced Configuration

#### Custom Port

To use a different port:

1. Update **Startup Parameters** to: `-port 12345 -maxplayers 16`
2. Update firewall rule to allow the new port
3. Update port forwarding (if applicable)

#### Maximum Players

To allow more/fewer players:

1. Update **Startup Parameters** to: `-port 7777 -maxplayers 32`
2. Keep in mind server performance and network bandwidth

#### Scheduled Restarts

In AMP, you can configure scheduled tasks to restart the server:
- Useful for daily maintenance
- Prevents memory leaks over long uptime
- Can run at low-traffic times

## Monitoring

### AMP Console

- View server logs in real-time in AMP's console
- See player connections/disconnections
- Monitor for errors

### Server Commands

Connect to the server console and use these commands:
- `status` - Show server status
- `players` - List connected players
- `help` - Show all commands

### Resource Usage

Monitor these metrics in AMP:
- **CPU Usage:** Should be low (<5% for idle server)
- **Memory Usage:** ~50-100 MB base + ~5 MB per player
- **Network:** ~1-2 KB/s per connected player

## Troubleshooting

### Server won't start

1. **Check console for errors:**
   - Port already in use? Change port number
   - Missing dependencies? Ensure .NET Framework 4.6.1+ installed

2. **Check AMP logs:**
   - View instance logs in AMP
   - Look for startup errors

### Players can't connect

1. **Verify server is running:**
   - Check AMP status (should show "Running")
   - Check console for "Server started" message

2. **Check network:**
   - Verify firewall allows UDP port 7777
   - Test port with: `telnet <server-ip> 7777` (won't work for UDP, but verifies connectivity)
   - Use online port checker tool for UDP

3. **Verify IP/Port:**
   - Use server's public IP (not localhost)
   - Check port matches configuration
   - Verify port forwarding if behind router

### High CPU/Memory Usage

1. **Reduce max players** in startup parameters
2. **Check for memory leaks** (restart daily if needed)
3. **Monitor individual player impact**

### Connection Timeouts

1. **Check server logs** for disconnection reasons
2. **Verify network stability** between clients and server
3. **Increase timeout values** in server code if needed (requires rebuild)

## Best Practices

1. **Regular Backups:**
   - Backup server configuration
   - Backup any saved game states (if implemented)

2. **Monitoring:**
   - Set up alerts in AMP for crashes
   - Monitor resource usage trends
   - Keep server updated

3. **Security:**
   - Keep Windows/AMP updated
   - Use firewall rules (don't allow all ports)
   - Consider password protection (future feature)

4. **Performance:**
   - Run on dedicated server (not shared hosting)
   - Ensure adequate network bandwidth
   - Monitor for bottlenecks

## Next Steps

- Configure master server for game discovery (see `MASTER_SERVER_INFO.md`)
- Set up monitoring/alerting
- Configure automatic restarts
- Test with multiple players

## Support

For issues:
1. Check server console logs
2. Check AMP logs
3. Review `DEDICATED_SERVER_README.md` for server details
4. Check main `README.md` for client-side issues

