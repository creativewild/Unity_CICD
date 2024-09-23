using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

public class BuildScript
{
    public static void Build()
    {

        string[] args = Environment.GetCommandLineArgs();
        string outputPath = "Builds";
        BuildTarget targetPlatform = BuildTarget.StandaloneWindows64;
        BuildOptions buildOptions = BuildOptions.None;
        SceneList customSceneList = null;

        foreach (string arg in args)
        {
            if (arg.StartsWith("-outputPath"))
            {
                outputPath = arg.Split('=')[1];
            }
            else if (arg.StartsWith("-targetPlatform"))
            {
                switch (arg.Split('=')[1].ToLower())
                {
                    case "windows":
                        targetPlatform = BuildTarget.StandaloneWindows64;
                        break;
                    case "mac":
                        targetPlatform = BuildTarget.StandaloneOSX;
                        break;
                    case "linux":
                        targetPlatform = BuildTarget.StandaloneLinux64;
                        break;
                    case "android":
                        targetPlatform = BuildTarget.Android;
                        break;
                    case "ios":
                        targetPlatform = BuildTarget.iOS;
                        break;
                    default:
                        Debug.LogError("Unsupported platform specified.");
                        return;
                }
            }
            else if (arg.StartsWith("-dev"))
            {
                if (arg.Split('=')[1].ToLower() == "true")
                {
                    buildOptions |= BuildOptions.Development;
                }
            }
            else if (arg.StartsWith("-sceneList"))
            {
                string sceneListName = arg.Split('=')[1];
                customSceneList = Resources.Load<SceneList>(sceneListName);

                if (customSceneList == null)
                {
                    Debug.LogError($"Scene list '{sceneListName}' not found in Resources.");
                    return;
                }
            }
        }

        // Force all assets to be imported before building
        ForceAssetImport();

        string[] scenes = customSceneList != null ? customSceneList.scenes : GetEnabledScenes();

        BuildPipeline.BuildPlayer(scenes, outputPath, targetPlatform, buildOptions);
    }

    private static void ForceAssetImport()
    {

        AssetDatabase.Refresh();

        // Wait until all imports are done
        while (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            System.Threading.Thread.Sleep(100);
        }

        Debug.Log("All assets have been imported and are up-to-date.");
    }

    private static string[] GetEnabledScenes()
    {
        
        return Array.FindAll(EditorBuildSettings.scenes, scene => scene.enabled).Select(scene => scene.path).ToArray();
    }
}
