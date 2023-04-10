# StanleyVR Development Setup

:warning: These are instructions on how to set up StanleyVR for mod development, not for installing / playing. If you just want to install StanleyVR to play Firewatch in VR, follow the [instructions in the main readme](https://github.com/Raicuparta/stanley-vr#readme).

- Install Unity 2019.4.33f.
- Open the `StanleyVrUnity` Unity project and build it in to the `StanleyVrUnity/Build` folder.
- If you haven't already, [download and install the latest StanelyVR release](https://raicuparta.itch.io/two-forks-vr). You can use the Rai Manager and BepInEx builds included in this release to test your own local builds of the mod.
- Clone StanleyVR's source.
- Edit `Directory.build.props` (or create a .user file that overrides it):
  - `<PublishDir>` should in most cases point to your RaiManager `Mod` subfolder. You can use different folders for Release/Debug configurations, or just use the same for both.
- Open the project solution file `StanleyVR.sln` in Visual Studio (2022+) or Rider (2022+) or whatever else works (has to support C# 10).
- Check the Nuget packages, some times you might need to restore them manually.
- Select and build the `Debug | x64` configuration. It will be compiled and placed in the RaiManager mod folder.
- After this, you should be able to start The Stanley Parable, and it will run with your local build of StanleyVR, provided you installed it from RaiManager.

If some of these steps fail, you might need to do some of them manually:

- To fix the references, right-click "References" in the Solution Explorer > "Add Reference", and add all the missing DLLs (references with yellow warning icon). You can find these DLLs in the game's \_Data directory.
- If your IDE isn't able to automatically copy the files, you'll have to copy the built dlls manually to the BepInEx plugins folder.
