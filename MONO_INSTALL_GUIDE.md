# Mono Installation Guide for Linux

If `mono-complete` package isn't found, the installation method depends on your Linux distribution.

## Quick Diagnosis

First, identify your Linux distribution:
```bash
cat /etc/os-release
```

## Installation by Distribution

### Ubuntu/Debian

**Option 1: Official Mono Repository (Recommended)**
```bash
# Add Mono repository
sudo apt-get update
sudo apt-get install apt-transport-https dirmngr gnupg ca-certificates
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb https://download.mono-project.com/repo/ubuntu stable-focal main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
sudo apt-get update
sudo apt-get install mono-complete
```

**Option 2: Try different package names**
```bash
sudo apt-get update
sudo apt-get install mono-runtime mono-devel
```

### CentOS/RHEL/Fedora

**Fedora (newer versions):**
```bash
sudo dnf install mono-complete
```

**Fedora (older versions) / CentOS/RHEL 7:**
```bash
# First enable EPEL repository (if not already enabled)
sudo yum install epel-release

# Then install Mono
sudo yum install mono-complete
```

**CentOS/RHEL 8+:**
```bash
# Try dnf first
sudo dnf install mono-complete

# If dnf not available, use yum
sudo yum install mono-complete
```

**If neither dnf nor yum work:**
```bash
# Check what package manager you have
which yum
which dnf
which apt-get

# For some minimal/server installations, you may need to install dnf/yum first
# Or use alternative installation methods below
```

### Alternative: Install Mono Runtime Only

If you only need to run the .exe (don't need to compile), you can install just the runtime:
```bash
# Ubuntu/Debian
sudo apt-get install mono-runtime

# Verify installation
mono --version
```

## Verify Installation

After installation, verify Mono is installed:
```bash
mono --version
```

You should see something like:
```
Mono JIT compiler version 6.12.0.xxx
```

## Alternative: Use .NET Core/.NET 5+ Instead

If Mono installation is problematic, you have these options:

### Option 1: Convert Server to .NET Core (Requires code changes)
- Migrate the server project to .NET Core
- Build native Linux executable
- No Mono needed

### Option 2: Use .NET 6.0 Runtime (May require project migration)
```bash
# Ubuntu/Debian
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-runtime-6.0
```

## Check What's Available

To see what Mono packages are available on your system:
```bash
# Ubuntu/Debian
apt-cache search mono | grep -i runtime

# CentOS/RHEL/Fedora
dnf search mono
# or
yum search mono
```

## Troubleshooting

**Package not found:**
- Update package lists: `sudo apt-get update` or `sudo dnf update`
- Check your Linux distribution version
- Try the official Mono repository method above

**Permission issues:**
- Ensure you're using `sudo` for installation commands
- Check if your user has sudo privileges

