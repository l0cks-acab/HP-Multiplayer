# AMP Template Setup Guide

This guide explains how to use the AMP template files for HP Multiplayer Server.

## Template Files

The following files have been created:

1. **hpmultiplayer.kvp** - Template metadata
2. **hpmultiplayerconfig.json** - Main configuration file
3. **hpmultiplayermetaconfig.json** - Additional metadata and dependencies

## Installation Steps

### Step 1: Prepare Server Files

1. **Build the server** (see `LINUX_BUILD.md`)
2. **Upload to your server**:
   - Create directory: `/home/amp/instances/hpmultiplayer/`
   - Upload `HPMultiplayer.Server.exe` (or Linux build)

### Step 2: Install Template Files

1. **Upload template files to AMP:**
   ```bash
   # On your AMP server
   cd /home/amp/.config/AMP/Plugins/ADSModule/GenericTemplates/
   
   # Upload the three template files:
   # - hpmultiplayer.kvp
   # - hpmultiplayerconfig.json
   # - hpmultiplayermetaconfig.json
   ```

2. **Set proper permissions:**
   ```bash
   chmod 644 hpmultiplayer.kvp
   chmod 644 hpmultiplayerconfig.json
   chmod 644 hpmultiplayermetaconfig.json
   ```

3. **Restart AMP:**
   ```bash
   sudo systemctl restart amp
   # Or use AMP's web interface to restart
   ```

### Step 3: Install Dependencies (Linux Only)

If running on Linux with Mono:

```bash
sudo apt-get update
sudo apt-get install mono-complete mono-devel
```

Or if using .NET Core:

```bash
# Install .NET 6.0 Runtime
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-runtime-6.0
```

### Step 4: Create Instance in AMP

1. **Login to AMP Web Interface**
2. **Create New Instance:**
   - Click "Create Instance"
   - Select "HP Multiplayer Server" from the template list
   - Choose a name for your instance
   - Select Linux platform

3. **Configure Instance:**
   - **Executable Path:** `HPMultiplayer.Server.exe` (or `./HPMultiplayer.Server` for .NET Core)
   - **Working Directory:** Path where you uploaded the server files
   - **Startup Parameters:** Will use `${Port}` and `${MaxPlayers}` from settings
   
   **For Mono (Linux):**
   - You may need to set a custom startup command:
   - **Executable:** `mono`
   - **Parameters:** `HPMultiplayer.Server.exe -port ${Port} -maxplayers ${MaxPlayers}`

   **For .NET Core (Linux):**
   - **Executable:** `./HPMultiplayer.Server`
   - **Parameters:** `-port ${Port} -maxplayers ${MaxPlayers}`

4. **Configure Settings:**
   - **Server Port:** 7777 (or your preferred port)
   - **Maximum Players:** 16 (or your preferred limit)

5. **Configure Network:**
   - AMP should automatically detect port 7777 (UDP)
   - Ensure port forwarding is configured if needed

### Step 5: Test the Server

1. **Start the instance** from AMP web interface
2. **Monitor console output:**
   - Should see: "Server started successfully on port 7777"
   - Status should show "Ready"

3. **Test connection:**
   - From game client, connect to server IP:port
   - Verify players can connect and sync

## Troubleshooting

### Server Won't Start

**Check logs:**
- View AMP instance logs
- Check for missing dependencies (Mono/.NET)

**Common issues:**
- Missing Mono runtime on Linux
- Incorrect executable path
- Port already in use
- Missing execute permissions

### Template Not Appearing

1. **Verify file names** are lowercase: `hpmultiplayer.*`
2. **Check file location:** `/home/amp/.config/AMP/Plugins/ADSModule/GenericTemplates/`
3. **Restart AMP** after adding templates
4. **Check AMP logs** for template loading errors

### Server Starts But Players Can't Connect

1. **Check firewall:**
   ```bash
   sudo ufw allow 7777/udp
   ```

2. **Verify port in settings** matches actual port
3. **Check server console** for connection attempts
4. **Test locally first** before testing remotely

## Customizing the Template

You can customize the template by editing `hpmultiplayerconfig.json`:

- **Default port:** Change `"Default": 7777` in Port setting
- **Default max players:** Change `"Default": 16` in MaxPlayers setting
- **Ready state pattern:** Adjust regex pattern in `ReadyState`
- **Startup command:** Modify `StartupParameters`

## Contributing to AMP Templates Repository

If you want to submit this template to the official repository:

1. **Test thoroughly** on both Windows and Linux
2. **Follow repository guidelines:**
   - Files must be lowercase
   - No directories
   - Only include `.kvp`, `*config.json`, and `*metaconfig.json`
3. **Create pull request** to https://github.com/CubeCoders/AMPTemplates

## Notes

- The template is configured for **StandardIO** console mode
- Server must output to console for AMP to monitor status
- UDP port 7777 is the default (configurable)
- Supports both Mono (Linux) and native .NET Core builds

