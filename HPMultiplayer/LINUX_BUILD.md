# Building HP Multiplayer Server for Linux

The HP Multiplayer server can run on Linux using Mono runtime (for .NET Framework 4.6.1) or by migrating to .NET Core/.NET 5+ for better cross-platform support.

## Option 1: Using Mono (Current .NET Framework Build)

### Prerequisites

```bash
# Install Mono runtime
sudo apt-get update
sudo apt-get install mono-complete mono-devel

# Verify installation
mono --version
```

### Building for Linux

The Windows build should work with Mono, but for best results:

1. **Build on Windows** (as normal):
   ```bash
   msbuild Server\HPMultiplayer.Server.csproj /p:Configuration=Release
   ```

2. **Copy to Linux server:**
   - Copy `Server\bin\Release\HPMultiplayer.Server.exe` to your Linux server
   - Ensure it has execute permissions: `chmod +x HPMultiplayer.Server.exe`

3. **Run with Mono:**
   ```bash
   mono HPMultiplayer.Server.exe -port 7777 -maxplayers 16
   ```

## Option 2: Migrate to .NET Core/.NET 5+ (Recommended for Linux)

For better Linux support, migrate the server to .NET Core/.NET 5+:

### Step 1: Update Project File

Create `Server/HPMultiplayer.Server.csproj` with .NET Core target:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
    <RuntimeIdentifiers>win-x64;linux-x64;osx-x64</RuntimeIdentifiers>
    <SelfContained>false</SelfContained>
  </PropertyGroup>
</Project>
```

### Step 2: Build for Linux

```bash
# Build for Linux
dotnet publish Server/HPMultiplayer.Server.csproj -c Release -r linux-x64 --self-contained false

# Output will be in: Server/bin/Release/net6.0/linux-x64/publish/
```

### Step 3: Run on Linux

```bash
# Install .NET Runtime (if not self-contained)
# See: https://dotnet.microsoft.com/download/dotnet/6.0

# Run the server
./HPMultiplayer.Server -port 7777 -maxplayers 16
```

## Quick Migration Guide

To migrate from .NET Framework to .NET Core:

1. **Change project file format** to SDK-style
2. **Update TargetFramework** to `net6.0` or `net8.0`
3. **Remove Unity-specific dependencies** (server doesn't need them)
4. **Test compilation** - should work as-is since we already avoided Unity dependencies

The server code is already designed to be Unity-independent, so migration should be straightforward.

## AMP Template Usage

The AMP template (`hpmultiplayerconfig.json`) is configured to work with both Mono and .NET Core:

- **Mono**: Uses `mono HPMultiplayer.Server.exe` (can be set in AMP settings)
- **.NET Core**: Uses `./HPMultiplayer.Server` directly

## Recommended Approach

For production Linux hosting, **Option 2 (.NET Core)** is recommended because:
- Better performance on Linux
- No Mono dependency
- Easier deployment
- Better tooling support
- Smaller runtime footprint

However, the current Mono approach works fine for getting started quickly.

