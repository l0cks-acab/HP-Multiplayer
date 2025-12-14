# How to Install Build Tools

To build the mod, you need MSBuild. Here's the easiest way:

## Install Visual Studio Build Tools (FREE)

1. **Download Visual Studio Build Tools:**
   - Go to: https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022
   - Scroll down to "Tools for Visual Studio 2022"
   - Click "Download" under "Build Tools for Visual Studio 2022"

2. **Run the installer:**
   - Double-click the downloaded file
   - Select "Desktop development with C++" workload
   - OR just select ".NET desktop build tools" (lighter option)
   - Click "Install"

3. **After installation:**
   - Restart VS Code or your terminal
   - Run the build script again (BUILD.bat or Ctrl+Shift+B)

That's it! The build tools are free and won't take up too much space (~500MB-2GB depending on what you install).

## Alternative: Install Full Visual Studio Community (FREE)

If you want a full IDE:
1. Go to: https://visualstudio.microsoft.com/downloads/
2. Download "Visual Studio Community 2022" (it's free)
3. During installation, select ".NET desktop development"
4. Open HPMultiplayer.sln in Visual Studio
5. Press Ctrl+Shift+B to build

