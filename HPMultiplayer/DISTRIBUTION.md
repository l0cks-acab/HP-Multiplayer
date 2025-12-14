# How to Distribute the Mod

**Author:** l0cks-acab

## For Users (What to Share)

You only need to share **ONE file** with other users:

### The DLL File

**File to share:** `HPMultiplayer.dll`

**Location on your computer:**
- After building: `C:\Program Files (x86)\Steam\steamapps\common\House Party\Mods\HPMultiplayer.dll`
- Or from build folder: `HPMultiplayer\obj\Release\HPMultiplayer.dll`

## Distribution Methods

### Method 1: Share the DLL directly
1. Find `HPMultiplayer.dll` (see location above)
2. Upload it to:
   - Google Drive / Dropbox
   - Discord (as attachment)
   - GitHub Releases
   - Mod hosting site (ModDB, Nexus Mods, etc.)
3. Share the download link

### Method 2: Create a simple zip package
1. Create a folder named `HPMultiplayer-Mod`
2. Copy `HPMultiplayer.dll` into it
3. Add this `INSTALL.txt` file with instructions:
   ```
   HP Multiplayer Mod - Installation Instructions
   
   1. Make sure MelonLoader is installed in House Party
   2. Copy HPMultiplayer.dll to:
      [Steam Installation]\steamapps\common\House Party\Mods\
   3. Launch House Party
   4. Press M in-game to open the multiplayer menu
   ```
4. Zip the folder and share it

## Installation Instructions for Recipients

### Prerequisites
1. **House Party** installed via Steam
2. **MelonLoader** installed in House Party
   - Download from: https://github.com/LavaGang/MelonLoader/releases
   - Run the installer and select `HouseParty.exe`
   - Launch the game once to initialize MelonLoader

### Installing the Mod

1. **Find your House Party installation folder:**
   - Usually: `C:\Program Files (x86)\Steam\steamapps\common\House Party\`
   - Or right-click House Party in Steam → Properties → Local Files → Browse

2. **Copy the DLL:**
   - Copy `HPMultiplayer.dll` to the `Mods` folder:
   - `[House Party Folder]\Mods\HPMultiplayer.dll`

3. **Launch the game:**
   - Start House Party
   - The mod will load automatically
   - Press **M** in-game to open the multiplayer menu

## Quick Distribution Checklist

- [ ] Build the mod in Release mode (using `BUILD.bat`)
- [ ] Locate `HPMultiplayer.dll` in the Mods folder or `obj\Release\`
- [ ] (Optional) Create a zip package with instructions
- [ ] Share the file/package
- [ ] Include installation instructions (prerequisites + steps)

## Notes for Distributors

- **Only share the DLL** - users don't need source code
- **Include installation instructions** - many users won't know where the Mods folder is
- **Mention MelonLoader requirement** - this is the most common missing dependency
- **Recommend Release build** - Debug builds are larger and slower

## Troubleshooting for Users

If users report issues:

1. **Mod doesn't load:**
   - Verify MelonLoader is installed
   - Check that DLL is in the correct `Mods` folder (not a subfolder)
   - Check MelonLoader console for errors

2. **Can't connect:**
   - Both players need the same version of the mod
   - Check firewall settings
   - Ensure port forwarding if needed (port 7777)

3. **Game crashes:**
   - Ensure MelonLoader is up to date
   - Check game version compatibility
   - Review MelonLoader log files

