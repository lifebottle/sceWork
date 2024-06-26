# About
The original TOD1RSCE module v0.3 by TTEMMA was generously donated by the Temple of Tales Translations team.
Please visit http://temple-tales.ru/translations.html for more Tales of translation projects.

The original binaries were decompiled, repackaged and further modified and made to be compatible with .NET 5.0.
The original project was just a subfolder in the DC repo, so the commit history of this repo is just cherry-picked commits from there, just to have a coherent timeline of the tool's changes, in addition, the original net-framework solution is available in the `legacy` branch.

# Compile Instructions
1. Open sceWork.sln with Visual Studio Community Edition (2019)
1. Build the solution (Debug/Release is fine)
1. You should have `sceWork.exe` and `TableModule.dll`
1. Extract raw data with `sceWork.exe`
1. Extract data with Japanese text, you also need `JPCODES.txt` in the same directory

# Linux / macOS Instructions
1. Install .NET 5.0 Runtime: `wget https://download.visualstudio.microsoft.com/download/pr/827b12a8-8dea-43da-92a2-2d24c4936236/d2d61b3ed4b5ba3f682de3e04fc4d243/aspnetcore-runtime-5.0.5-linux-x64.tar.gz`
1. Create a new install directory for .NET 5.0: `mkdir dotnet-64`
1. Extract and install .NET 5.0: `tar zxf aspnetcore-runtime-5.0.5-linux-x64.tar.gz -C $HOME/dotnet-64`
1. To make the dotnet command run everywhere, put new path ahead of $PATH to increase the priority:
1. `export DOTNET_ROOT=$HOME/dotnet-64`
1. `export PATH=$HOME/dotnet-64:$PATH`
1. Enter the following to make sure .NET 5.0 is working: `dotnet --info`

# Usage Instructions (General)
1. Use `//` to comment out lines that do not need to be inserted

# Usage Instructions (Skits)
1. Use `pakcomposer.exe` to extract `.pak1` files
1. BAT Example: `for %%i in (*.pak1) do pakcomposer.exe -d "%%i" -1 -x -u -v`
1. The `.pak1` filex extracts to a folder which has `.tod1rsce4` files inside
1. Put `sceWork.exe`, `TableModule.dll`, and `JPCODES.txt` in the same directory
1. Use Command Prompt to extract text from `.tod1rsce4` file
1. BAT Example: `for %%i in (*.tod1rsce4) do sceWork.exe -e "%%i"`
1. Edit the `.txt` file, view with SHIFT-JIS encoding
1. Insert the tex back into the `.tod1rsce4` file
1. BAT Example: `for %%i in (*.tod1rsce4) do sceWork.exe -r "%%i" -as 12 -ae`
1. Move/copy the `.tod1rsce4` file into the folder where it was originally extracted
1. Pack the files in this folder into a `.pak1` file again
1. BAT Example: `for /D %%i in (*) do pakcomposer.exe -c %%i -1 -x -u -tod2_ps2_skit_padding`
1. Need to fix `.pak1` file.  Open with HEX Editor to fix issue with skits.
1. Look at offset `0x00000004` if it is `24` change it to `30` and save.
1. Luckily, we have a script that does this for all `.pak1` files in the folder.
1. BAT Example: `python modPAK1Files.py`
1. Copy `.pak1` files into `DAT_FILES` and pack into `DAT.BIN` and `DAT.TBL`
1. Create a new ISO with the new files and try it out

# Usage Instructions (Scenarios)
1. Use `sceWork.exe` to extract `.txt` from `.rsce` files
1. Use with `TableModule.dll`, and `JPCODES.txt` to see Japanese, otherwise RAW dump
1. BAT Example: `for %%i in (*.rsce) do sceWork.exe -e "%%i"`
1. Edit `.txt` file and save with the current encoding, else game crash
1. Insert text back into the the `.rsce` file.
1. BAT Example: `for %%i in (*.rsce) do sceWork.exe -r "%%i"`
1. Replace the `.rsce` in the `.mglk` folder that was extracted with `ToDDCTools_v1.1.exe`
1. Pack it up into a new `.mglk` file
1. BAT Example: `for %%i in (*.mglk) do ToDDCTools_v1.1.exe mglk "%%i"`
1. Move/copy the newly packed `.mglk` file into `DAT_FILES`
1. Use `ToDDCTools_v1.1.exe` to repack `DAT.BIN` and `DAT.TBL`
1. Create a new ISO with the new files and try it out
