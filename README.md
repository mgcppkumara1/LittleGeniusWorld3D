# Little Genius World 3D (Unity 6)

Port of Little Genius World 2.0 to Unity 6 with a real 3D island, 3D toys, and Android TTS (English first, Sinhala second).

## Build the APK
1. Unity Hub: install Unity 6 (6000.0 LTS) with the **Android Build Support** module (incl. OpenJDK + SDK/NDK).
2. Hub > Add > Add project from disk > select this folder. Pick your installed Unity 6 editor when asked.
3. Wait for scripts to compile (Console must show no errors).
4. Menu **LGW > 2 - Build APK**. Output: `Builds/LittleGeniusWorld3D.apk`.
   (Or run **LGW > 1 - Setup** and use File > Build Profiles / Build Settings yourself.)
5. Copy the APK to the phone and install (allow unknown sources).

## Notes
- All art is generated from Unity primitives (placeholder). Replace `Toys.cs` with real models later.
- Sinhala: voice uses the phone's Text-to-Speech Sinhala voice if installed. Sinhala *text* in Unity's legacy Text has no complex-script shaping, so some letters may render wrongly. If so, use a Sinhala-capable shaping solution (e.g. TextMeshPro + a shaping plugin) or pre-rendered Sinhala sprites.
- Package id is `com.littlegenius.world` (same as 2.0). Uninstall 2.0 first if install says "signature conflict".
- Stars are saved with PlayerPrefs. Parent corner has a simple maths gate.
