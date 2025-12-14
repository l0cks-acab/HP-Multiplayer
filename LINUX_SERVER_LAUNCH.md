# HP Multiplayer Server - Linux Launch Guide

Simple guide for running the HP Multiplayer server on Linux without AMP.

## Quick Start

### Option 1: Simple Launch Script

1. **Upload files to your server:**
   - `HPMultiplayer.Server.exe`
   - `start-server.sh`

2. **Make script executable:**
   ```bash
   chmod +x start-server.sh
   ```

3. **Run the server:**
   ```bash
   # Use defaults (port 7777, max 16 players)
   ./start-server.sh

   # Or specify port and max players
   ./start-server.sh 7777 16
   ```

### Option 2: Launch with Config File

1. **Upload files:**
   - `HPMultiplayer.Server.exe`
   - `start-server-with-config.sh`
   - `server.conf`

2. **Edit `server.conf`:**
   ```bash
   nano server.conf
   ```
   Set your desired port and max players:
   ```
   PORT=7777
   MAX_PLAYERS=16
   ```

3. **Make script executable:**
   ```bash
   chmod +x start-server-with-config.sh
   ```

4. **Run the server:**
   ```bash
   ./start-server-with-config.sh
   ```

### Option 3: Run as Systemd Service

1. **Edit the service file:**
   ```bash
   nano hpmultiplayer.service
   ```
   Update paths and settings:
   - `WorkingDirectory`: Path to your server directory
   - `ExecStart`: Full path to mono and server executable
   - `User`/`Group`: Your server user (usually `amp` or your username)
   - Port and max players in the ExecStart command

2. **Install the service:**
   ```bash
   sudo cp hpmultiplayer.service /etc/systemd/system/
   sudo systemctl daemon-reload
   sudo systemctl enable hpmultiplayer.service
   sudo systemctl start hpmultiplayer.service
   ```

3. **Check status:**
   ```bash
   sudo systemctl status hpmultiplayer.service
   ```

4. **View logs:**
   ```bash
   sudo journalctl -u hpmultiplayer.service -f
   ```

5. **Stop/Start/Restart:**
   ```bash
   sudo systemctl stop hpmultiplayer.service
   sudo systemctl start hpmultiplayer.service
   sudo systemctl restart hpmultiplayer.service
   ```

## Prerequisites

### Install Mono Runtime

```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get install mono-runtime

# Verify installation
mono --version
```

See `MONO_INSTALL_GUIDE.md` for detailed installation instructions.

## Configuration

### Command Line Arguments

```bash
./start-server.sh [port] [maxplayers]
```

Examples:
```bash
./start-server.sh              # Port 7777, 16 players (defaults)
./start-server.sh 8888         # Port 8888, 16 players
./start-server.sh 7777 32      # Port 7777, 32 players
```

### Environment Variables

You can also set environment variables:

```bash
export PORT=7777
export MAX_PLAYERS=16
./start-server.sh
```

### Config File

Edit `server.conf`:
```bash
PORT=7777
MAX_PLAYERS=16
```

## Firewall Configuration

Allow UDP traffic on your server port:

```bash
# Ubuntu/Debian (ufw)
sudo ufw allow 7777/udp

# Or for specific IP
sudo ufw allow from 192.168.1.0/24 to any port 7777 proto udp

# Check status
sudo ufw status
```

## Running in Background

### Using nohup

```bash
nohup ./start-server.sh > server.log 2>&1 &
```

### Using screen

```bash
# Install screen if needed
sudo apt-get install screen

# Start server in screen session
screen -S hpmultiplayer ./start-server.sh

# Detach: Press Ctrl+A then D
# Reattach: screen -r hpmultiplayer
```

### Using tmux

```bash
# Install tmux if needed
sudo apt-get install tmux

# Start server in tmux session
tmux new-session -d -s hpmultiplayer './start-server.sh'

# Attach: tmux attach -t hpmultiplayer
# Detach: Press Ctrl+B then D
```

## Troubleshooting

### Server Won't Start

**Check Mono is installed:**
```bash
which mono
mono --version
```

**Check executable exists:**
```bash
ls -la HPMultiplayer.Server.exe
```

**Check permissions:**
```bash
chmod +x start-server.sh
chmod +x HPMultiplayer.Server.exe
```

**Run manually to see errors:**
```bash
mono HPMultiplayer.Server.exe -port 7777 -maxplayers 16
```

### Port Already in Use

```bash
# Check what's using the port
sudo netstat -tulpn | grep 7777
# or
sudo ss -tulpn | grep 7777

# Kill the process if needed
sudo kill <PID>
```

### Players Can't Connect

1. **Check firewall:**
   ```bash
   sudo ufw status
   ```

2. **Verify server is running:**
   ```bash
   ps aux | grep mono
   ```

3. **Check server is listening:**
   ```bash
   sudo netstat -tulpn | grep 7777
   ```

4. **Test locally first:**
   ```bash
   # On server, test connection
   nc -u localhost 7777
   ```

## File Structure

```
/home/amp/instances/hpmultiplayer/
├── HPMultiplayer.Server.exe
├── start-server.sh
├── start-server-with-config.sh
├── server.conf
└── hpmultiplayer.service (optional, for systemd)
```

## Quick Commands Reference

```bash
# Start server (simple)
./start-server.sh

# Start with custom settings
./start-server.sh 7777 32

# Start with config file
./start-server-with-config.sh

# Run in background
nohup ./start-server.sh > server.log 2>&1 &

# Check if running
ps aux | grep mono

# View logs (if using nohup)
tail -f server.log

# Stop server
pkill -f HPMultiplayer.Server.exe
```

## Next Steps

- Set up automatic startup (systemd service)
- Configure log rotation
- Set up monitoring
- Configure port forwarding if hosting remotely

