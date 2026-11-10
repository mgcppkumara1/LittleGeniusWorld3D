using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LGWBuild
{
    const string ScenePath = "Assets/Scenes/Main.unity";

    [MenuItem("LGW/1 - Setup scene + Android settings")]
    public static void Setup()
    {
        Directory.CreateDirectory("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("LGW").AddComponent<LGWGame>();
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        var t = NamedBuildTarget.Android;
        PlayerSettings.companyName = "LittleGenius";
        PlayerSettings.productName = "Little Genius World 3D";
        PlayerSettings.SetApplicationIdentifier(t, "com.littlegenius.world");
        PlayerSettings.bundleVersion = "3.0";
        PlayerSettings.Android.bundleVersionCode = 3;
        PlayerSettings.SetScriptingBackend(t, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

        // This project uses the legacy Input Manager (Input.GetMouseButton*, StandaloneInputModule).
        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
        if (assets != null && assets.Length > 0)
        {
            var so = new SerializedObject(assets[0]);
            var p = so.FindProperty("activeInputHandler");
            if (p != null && p.intValue != 0) { p.intValue = 0; so.ApplyModifiedProperties(); }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("LGW setup done.");
    }

    [MenuItem("LGW/2 - Build APK")]
    public static void BuildApk()
    {
        Setup();
        Directory.CreateDirectory("Builds");
        EditorUserBuildSettings.buildAppBundle = false;
        var opts = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = "Builds/LittleGeniusWorld3D.apk",
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(opts);
        Debug.Log("LGW build result: " + report.summary.result + " -> " + report.summary.outputPath);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            EditorUtility.RevealInFinder(report.summary.outputPath);
    }
}
