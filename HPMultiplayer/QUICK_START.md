# Quick Start - Building the Mod

## Prerequisites

You need **MSBuild** to compile the mod. Install one of these (all FREE):

### Option 1: Visual Studio Build Tools (Recommended - Smallest)
- Download: https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022
- Install "Build Tools for Visual Studio 2022"
- Select ".NET desktop build tools" during installation
- ~500MB download

### Option 2: Visual Studio Community (Full IDE)
- Download: https://visualstudio.microsoft.com/downloads/
- Install "Visual Studio Community 2022"
- Select ".NET desktop development" during installation
- Open `HPMultiplayer.sln` and press Ctrl+Shift+B
- ~3-5GB download

## Building

**After installing build tools:**

1. **Option A:** Double-click `BUILD.bat` in the HPMultiplayer folder
2. **Option B:** Press `Ctrl+Shift+B` in VS Code
3. **Option C:** Open `HPMultiplayer.sln` in Visual Studio and press `Ctrl+Shift+B`

## After Building

The DLL will be automatically copied to:
`C:\Program Files (x86)\Steam\steamapps\common\House Party\Mods\HPMultiplayer.dll`

Launch House Party and press **M** to open the multiplayer menu!

