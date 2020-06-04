~~Note: This project is currently on hold during the college semester.~~

on hold forever, no support offered

# FFXIVHousingSim (fork)

## Current state
Fork of [perchbird's FFXIVHousingSim](https://github.com/lmcintyre/FFXIVHousingSim) repurposed to extract and load arbitrary zones into Unity.
Uses [ufx's fork of SaintCoinach](https://github.com/ufx/SaintCoinach).

Currently, the project has some functionality including (assume all functionality is incomplete):
- Extraction of FFXIV models for use with the Unity project
- Extraction of necessary FFXIV data such as model positions and some basic translation/rotation animations
- Extraction of some FFXIV lighting data
- Extraction of some VFX meshes (animations are not supported, these are only dumped for placeholders)

## How to use it
- Make sure you have the game installed and updated.
- Clone the repository, clone/init the submodule
- Open the `SaintCoinach` solution and restore the NuGet packages, then build `SaintCoinach`.
- Open the `FFXIVHSLauncher` solution and restore the NuGet packages.
  - Change the string `root` in `FFXIVHSLib.FFXIVHSPaths` to point to the folder you'd like the programs to use and build the solution.
    - Change the string `GameDirectory` in `FFXIVHSLib.FFXIVHSPaths` to your FFXIV game path e.g. `private const string GameDirectory = @"C:\Program Files (x86)\SquareEnix\FINAL FANTASY XIV - Heavensward\";`
  - If the reference to `UnityEngine.dll` is broken, at the top of `Visual Studio`, click `Tools > NuGet Package Manager > Package Manager Console`. Once it appears, enter `Add-Unity3DReference -AssemblyName UnityEngine -ProjectName FFXIVHSLib` and press enter. If this doesn't work, make sure you restored the NuGet packages.
  - A post-build script copies `FFXIVHSLib.dll` to the Unity project. If this doesn't take place, copy it manually.
- Select a territory (`search by typing the shorthand e.g. f1t1`) and click `Extract Map JSON` __twice__. This is what extracts models. 
  - For unlisted territories, enter the path in `Load Dat Path` box and click `Extract Map JSON` __twice__ (make sure no item is selected).
- Open the Unity project by opening the folder `FFXIVHousingSim` in the root directory of the repository you cloned. Click `File > Open > [navigate into] Scenes > MainScene.unity`.
- You may need to install the asset store packages used in the project. Open the asset store from `Window > General > Asset Store` and search for and import `json.net`.
  - Click `Window > Package Manager`. In the top left, click the "All" tab. Click `Cinemachine` in the left list box. Click "Install" in the top right.
- Main Unity Editor toolbar > `FFXIVHS` > `Load HS Export` > `<teri>` (e.g. f1t1) > `Import`
  - Note you may need to import, delete the object from the scene, and import again for animations such as rotating objects to display. Other animations are not supported at this time.
    - Alternatively copy the `FFXIVHSLib.FFXIVHSPaths/<teri>/<scripts>` folder contents to the `Assets` folder of the project before importing
- After a few seconds you should have a loaded map! Press play to preview any animations.


The above documentation may be incorrect at any time and is untested. Visual Studio is used for the Launcher, and JetBrains Rider is used for the Unity code.

## Other useful info
`FFXIVHousingSim/Assets/DataHandler.cs` is the script used to import the files into Unity Editor.

Use the `Dump all Bg files` button to (try) dump all bg paths to a `lgb_dump.txt` file in `FFXIVHSLauncher.exe` dir, find any `.lgb` strings and use these paths for the `Load Dat Path` box for any unlisted territories.