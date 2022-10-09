using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Valve.Newtonsoft.Json;
using Valve.VR;

namespace StanleyVr;

public static class ApplicationManifestHelper
{
    public static void UpdateManifest(string manifestPath, string appKey, string imagePath, string name, string description, int steamAppId = 0, bool steamBuild = false)
    {
        try
        {
            Debug.Log("test 1");
            var launchType = steamBuild ? GetSteamLaunchString(steamAppId) : GetBinaryLaunchString();
            Debug.Log("test 2");
            var appManifestContent = $@"{{
                                            ""source"": ""builtin"",
                                            ""applications"": [{{
                                                ""app_key"": {JsonConvert.ToString(appKey)},
                                                ""image_path"": {JsonConvert.ToString(imagePath)},
                                                {launchType}
                                                ""last_played_time"":""{CurrentUnixTimestamp()}"",
                                                ""strings"": {{
                                                    ""en_us"": {{
                                                        ""name"": {JsonConvert.ToString(name)}
                                                    }}
                                                }}
                                            }}]
                                        }}";
            Debug.Log("test 3");

            File.WriteAllText(manifestPath, appManifestContent);
            Debug.Log("test 4");

            var error = OpenVR.Applications.AddApplicationManifest(manifestPath, false);
            Debug.Log("test 5");
            if (error != EVRApplicationError.None)
            {
                Debug.LogError("Failed to set AppManifest " + error);
            }
            Debug.Log("test 6");

            var processId = System.Diagnostics.Process.GetCurrentProcess().Id;
            Debug.Log("test 8");
            Debug.Log("test 8");
            var applicationIdentifyErr = OpenVR.Applications.IdentifyApplication((uint) processId, appKey);
            if (applicationIdentifyErr != EVRApplicationError.None)
            {
                Debug.LogError("Error identifying application: " + applicationIdentifyErr);
            }
            Debug.Log("test 7");
        }
        catch (Exception exception)
        {
            Debug.LogError("Error updating AppManifest: " + exception);
        }
    }

    private static string GetSteamLaunchString(int steamAppId)
    {
        return $@"""launch_type"": ""url"",
                      ""url"": ""steam://launch/{steamAppId}/VR"",";
    }

    private static string GetBinaryLaunchString()
    {
        var workingDir = Directory.GetCurrentDirectory();
        var executablePath = Assembly.GetExecutingAssembly().Location;
        return $@"""launch_type"": ""binary"",
                      ""binary_path_windows"": {JsonConvert.ToString(executablePath)},
                      ""working_directory"": {JsonConvert.ToString(workingDir)},";
    }

    private static long CurrentUnixTimestamp()
    {
        var foo = DateTime.Now;
        return ((DateTimeOffset)foo).ToUnixTimeSeconds();
    }
}