# HP Multiplayer Server - Deployment Files

This folder contains all the files you need to deploy and run the HP Multiplayer dedicated server on Linux.

## Required Files

1. **HPMultiplayer.Server.exe** - The server executable (build from source or download from releases)
2. **start-server.sh** - Simple launch script

## Optional Files

- **start-server-with-config.sh** - Launch script with config file support
- **server.conf** - Configuration file for easy settings management
- **hpmultiplayer.service** - Systemd service file for running as a background service

## Quick Start

### Step 1: Build or Download Server Executable

**Option A: Build from source:**
```bash
# On Windows with Visual Studio/MSBuild:
msbuild Server\HPMultiplayer.Server.csproj /p:Configuration=Release
# Output: Server\bin\Release\HPMultiplayer.Server.exe
```

**Option B: Download from repository:**
The server executable should be built from the source code in the repository.

### Step 2: Upload Files to Your Server

Upload these files to your Linux server:
- `HPMultiplayer.Server.exe`
- `start-server.sh`

You can upload via:
- SFTP/SCP
- File manager
- Any file transfer method

### Step 3: Install Mono Runtime

On your Linux server:
```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get install mono-runtime

# Verify installation
mono --version
```

See `../MONO_INSTALL_GUIDE.md` for detailed installation instructions.

### Step 4: Make Script Executable

```bash
chmod +x start-server.sh
```

### Step 5: Run the Server

```bash
# Use defaults (port 7777, 16 players)
./start-server.sh

# Or specify custom settings
./start-server.sh 7777 32
```

## Configuration Options

### Option 1: Command Line Arguments

```bash
./start-server.sh [port] [maxplayers]
```

Examples:
- `./start-server.sh` - Port 7777, 16 players (defaults)
- `./start-server.sh 8888` - Port 8888, 16 players
- `./start-server.sh 7777 32` - Port 7777, 32 players

### Option 2: Config File

1. Copy `server.conf` and edit it:
   ```bash
   cp server.conf my-server.conf
   nano my-server.conf
   ```

2. Edit the settings:
   ```
   PORT=7777
   MAX_PLAYERS=16
   ```

3. Use the config-enabled script:
   ```bash
   chmod +x start-server-with-config.sh
   ./start-server-with-config.sh my-server.conf
   ```

### Option 3: Environment Variables

```bash
export PORT=7777
export MAX_PLAYERS=16
./start-server.sh
```

## Running as a Service

To run the server as a systemd service:

1. **Edit the service file:**
   ```bash
   nano hpmultiplayer.service
   ```
   Update:
   - `WorkingDirectory`: Path to your server directory
   - `ExecStart`: Full path to mono and server executable
   - `User`/`Group`: Your server user
   - Port and max players in the ExecStart command

2. **Install the service:**
   ```bash
   sudo cp hpmultiplayer.service /etc/systemd/system/
   sudo systemctl daemon-reload
   sudo systemctl enable hpmultiplayer.service
   sudo systemctl start hpmultiplayer.service
   ```

3. **Manage the service:**
   ```bash
   sudo systemctl status hpmultiplayer.service
   sudo systemctl stop hpmultiplayer.service
   sudo systemctl restart hpmultiplayer.service
   ```

## Firewall Configuration

Allow UDP traffic on your server port:

```bash
# Ubuntu/Debian (ufw)
sudo ufw allow 7777/udp

# Verify
sudo ufw status
```

## Running in Background

### Using nohup

```bash
nohup ./start-server.sh > server.log 2>&1 &
```

### Using screen

```bash
screen -S hpmultiplayer ./start-server.sh
# Detach: Ctrl+A then D
# Reattach: screen -r hpmultiplayer
```

### Using tmux

```bash
tmux new-session -d -s hpmultiplayer './start-server.sh'
# Attach: tmux attach -t hpmultiplayer
```

## File Structure

After deployment, your server directory should look like:

```
/path/to/server/
├── HPMultiplayer.Server.exe
├── start-server.sh
├── start-server-with-config.sh (optional)
├── server.conf (optional)
└── hpmultiplayer.service (optional)
```

## Troubleshooting

### Server Won't Start

- **Check Mono is installed:** `mono --version`
- **Check executable exists:** `ls -la HPMultiplayer.Server.exe`
- **Check permissions:** `chmod +x start-server.sh`
- **Run manually to see errors:** `mono HPMultiplayer.Server.exe -port 7777 -maxplayers 16`

### Port Already in Use

```bash
sudo netstat -tulpn | grep 7777
sudo kill <PID>
```

### Players Can't Connect

1. Check firewall: `sudo ufw status`
2. Verify server is running: `ps aux | grep mono`
3. Check server is listening: `sudo netstat -tulpn | grep 7777`

## Additional Resources

- **Full Linux Launch Guide:** `../LINUX_SERVER_LAUNCH.md`
- **Server Setup Guide:** `../HPMultiplayer/SERVER_SETUP.md`
- **Mono Installation:** `../MONO_INSTALL_GUIDE.md`

## Support

For issues or questions, see the main repository:
https://github.com/l0cks-acab/HP-Multiplayer

