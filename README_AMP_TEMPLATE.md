# HP Multiplayer AMP Template

This directory contains the AMP (Application Management Panel) template files for hosting the HP Multiplayer dedicated server.

## Files

- **hpmultiplayer.kvp** - Template metadata and identification
- **hpmultiplayerconfig.json** - Main configuration file for AMP
- **hpmultiplayermetaconfig.json** - Additional metadata, dependencies, and documentation

## Quick Start

### For Windows

1. Build the server: `msbuild Server\HPMultiplayer.Server.csproj /p:Configuration=Release`
2. Copy `Server\bin\Release\HPMultiplayer.Server.exe` to your AMP server directory
3. Upload template files to AMP's GenericTemplates directory
4. Create new instance in AMP using "HP Multiplayer Server" template

### For Linux

**Option A: Using Mono (Quick Setup)**
1. Install Mono: `sudo apt-get install mono-complete`
2. Build server on Windows (same as above)
3. Upload `HPMultiplayer.Server.exe` to Linux server
4. In AMP instance settings, set:
   - **Executable:** `mono`
   - **Parameters:** `HPMultiplayer.Server.exe -port ${Port} -maxplayers ${MaxPlayers}`
5. Upload template files and create instance

**Option B: Using .NET Core (Recommended)**
1. Migrate server project to .NET Core (see `LINUX_BUILD.md`)
2. Build for Linux: `dotnet publish -r linux-x64`
3. Upload Linux build to server
4. In AMP, set executable to `./HPMultiplayer.Server`
5. Upload template files and create instance

## Template Installation

1. **Copy template files to AMP:**
   ```
   /home/amp/.config/AMP/Plugins/ADSModule/GenericTemplates/
   ```
   Files needed:
   - hpmultiplayer.kvp
   - hpmultiplayerconfig.json
   - hpmultiplayermetaconfig.json

2. **Restart AMP:**
   ```bash
   sudo systemctl restart amp
   ```

3. **Create instance:**
   - Login to AMP web interface
   - Create new instance
   - Select "HP Multiplayer Server" template
   - Configure settings (port, max players)
   - Start the server

## Configuration

The template includes configurable settings:

- **Port** (default: 7777) - Server listening port (UDP)
- **MaxPlayers** (default: 16) - Maximum concurrent players

These can be changed in AMP's instance settings after creation.

## Port Configuration

The template automatically configures UDP port 7777. Ensure:
- Firewall allows UDP port 7777
- Port forwarding is configured if hosting remotely
- Port is available (not in use by another service)

## Testing

After installation:

1. **Start the server** from AMP interface
2. **Verify console output:**
   - Should see: "Server started successfully on port 7777"
   - Status should show "Ready"

3. **Test connection:**
   - From game client, connect to server IP:port
   - Use HP Multiplayer mod's "Connect to Server" feature

## Troubleshooting

### Template Not Appearing
- Verify file names are lowercase: `hpmultiplayer.*`
- Check file location in GenericTemplates directory
- Restart AMP after adding templates

### Server Won't Start (Linux)
- Ensure Mono is installed: `mono --version`
- Or install .NET Runtime for .NET Core build
- Check executable permissions: `chmod +x HPMultiplayer.Server.exe`

### Players Can't Connect
- Verify firewall: `sudo ufw allow 7777/udp`
- Check server console for connection attempts
- Verify port matches in settings

## Contributing

To submit this template to the [AMPTemplates repository](https://github.com/CubeCoders/AMPTemplates):

1. Test on both Windows and Linux
2. Ensure files are lowercase (already done)
3. Create pull request with the three template files
4. Follow repository contribution guidelines

## References

- [AMP Generic Module Documentation](https://github.com/CubeCoders/AMP/wiki/Configuring-the-'Generic'-AMP-module)
- [AMP Templates Repository](https://github.com/CubeCoders/AMPTemplates)
- [AMP Configuration Generator](https://config.getamp.sh/)

## License

Same as main project - MIT License

