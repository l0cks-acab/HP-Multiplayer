# HP Multiplayer Mod - Setup Instructions

## Prerequisites

Before building this mod, make sure you have:

1. **House Party** installed via Steam
2. **MelonLoader** installed in House Party (run the installer and select HouseParty.exe)
3. **Visual Studio** (2019 or later) with .NET Framework 4.7.2 support
4. Started House Party at least once with MelonLoader to generate the required DLLs

## Important: Update DLL Paths

The `.csproj` file contains placeholder paths for the required DLLs. You **MUST** update these paths to match your actual Steam installation path.

### Steps to Update:

1. Open `HPMultiplayer.csproj` in a text editor
2. Find this section (around line 16-44):
   ```xml
   <Reference Include="MelonLoader">
     <HintPath>..\..\..\Steam\steamapps\common\House Party\MelonLoader\MelonLoader.dll</HintPath>
   ```
3. Replace `..\..\..\Steam\steamapps\common\House Party\` with your actual path:
   - Example: `C:\Program Files (x86)\Steam\steamapps\common\House Party\`
   - Example: `D:\Steam\steamapps\common\House Party\`

4. Update **ALL** the HintPath entries for:
   - `MelonLoader.dll`
   - `UnityEngine.CoreModule.dll`
   - `Il2Cppmscorlib.dll`
   - `UnityEngine.InputLegacyModule.dll`
   - `UnityEngine.InputSystem.dll`
   - `UnhollowerBaseLib.dll`
   - `Assembly-CSharp.dll`

## Building the Mod

1. Open `HPMultiplayer.sln` in Visual Studio
2. Set build configuration to **Release** (or Debug for testing)
3. Right-click the project → **Properties** → **Build** tab
4. Set **Output path** to: `C:\Path\To\Steam\steamapps\common\House Party\Mods\`
   (Replace with your actual House Party installation path)
5. Build the project (Build → Build Solution or F6)
6. The DLL will be automatically copied to the Mods folder if output path is set correctly

## Testing

1. Launch House Party
2. In-game, press **M** to open the multiplayer UI
3. One player clicks **Host Game**
4. Other players enter the host's IP address and click **Join Game**
5. You should see other players as colored capsules in the game

## Troubleshooting

- **Build errors about missing DLLs**: Update the HintPath in `.csproj` file
- **Mod doesn't load**: Check `MelonLoader\Logs\` for error messages
- **Can't connect**: 
  - Check firewall settings
  - Ensure ports 7777-7778 are open
  - Verify IP address is correct
  - Both players need the mod installed

## Next Steps for Development

To make this fully functional, you'll need to:

1. **Find the actual player GameObject**: 
   - Use dnSpy to inspect `Assembly-CSharp.dll`
   - Look for player controller classes
   - Find the actual player prefab/model

2. **Replace placeholder player representation**:
   - Currently uses primitive capsules
   - Should use actual player model from the game

3. **Add more synchronization**:
   - NPC states
   - Interactive objects
   - Inventory items
   - Dialogue states

4. **Improve networking**:
   - Add NAT punch-through for easier connections
   - Better error handling
   - Reconnection logic
   - Lag compensation

## Development Tips

- Use dnSpy to reverse engineer game classes
- Check `MelonLoader\Managed\` folder for available assemblies
- Look at other mods on GitHub for examples
- Join the Eek Discord for modding community support

