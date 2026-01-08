using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class BuildSetup
{
    private const string ConnectScenePortraitPath = "Assets/Scenes/ConnectScene-Portrait.unity";
    private const string ConnectScenePath = "Assets/Scenes/ConnectScene.unity";

    static BuildSetup()
    {
        BuildPlayerWindow.RegisterGetBuildPlayerOptionsHandler(ProcessBuildOptions);
    }

    private static BuildPlayerOptions ProcessBuildOptions(BuildPlayerOptions options)
    {
        // "options" are empty. Fill them in by calling the default getter.
        // NOTE: this will open a dialog where the user can select the location of the build to be made.
        options = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(options);

        // Read and parse AppConfig.json
        var configFilePath = "Assets/Resources/configs/AppConfig.json";
        var jsonContent = File.ReadAllText(configFilePath);
        var config = JObject.Parse(jsonContent);
        var isTelegram = config["gamePlatform"]!.Value<string>() == "TON";

        // Set the first and second scenes based on the isTelegram value
        var firstScene = isTelegram ? ConnectScenePortraitPath : ConnectScenePath;

        // Loop through all scenes to find the matching scene
        for (int i = 0; i < options.scenes.Length; i++)
        {
            if (options.scenes[i] == firstScene)
            {
                // Swap the scenes to set the first scene at index 0
                (options.scenes[0], options.scenes[i]) = (options.scenes[i], options.scenes[0]);
                break;
            }
        }

        return options;
    }
}