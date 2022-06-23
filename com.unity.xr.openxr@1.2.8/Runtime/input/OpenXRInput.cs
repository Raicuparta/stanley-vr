using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.OpenXR.Features;

namespace UnityEngine.XR.OpenXR.Input
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    internal static class OpenXRInput
    {
        static List<OpenXRInteractionFeature.ActionMapConfig> s_ActionMapsToLoad = new List<OpenXRInteractionFeature.ActionMapConfig>();

        static bool started { get; set; }

        static OpenXRInput()
        {
#if UNITY_EDITOR
            // In the editor we need to make sure the OpenXR layouts get registered even if the user doesn't
            // navigate to the project settings.  The following code will register the base layouts as well
            // as any enabled interaction features.
            RegisterLayouts();

            // Find all enabled interaction features and force them to register their device layouts
            var packageSettings = OpenXRSettings.GetPackageSettings();
            if (null == packageSettings)
                return;

            foreach (var feature in packageSettings.GetFeatures<OpenXRInteractionFeature>().Where(f => f.feature.enabled).Select(f => f.feature))
                feature.OnEnabledChange();
#endif
        }

        internal static void AddActionMap(OpenXRInteractionFeature.ActionMapConfig map)
        {
            if (started)
            {
                Debug.LogWarning(
                    "OpenXRLoader.AddActionMap called after the action maps have already been sent to OpenXR. These will be ignored.");
                return;
            }

            s_ActionMapsToLoad.Add(map);
        }

        internal static void RegisterLayouts ()
        {
            InputSystem.InputSystem.RegisterLayout<PoseControl>("Pose");
            InputSystem.InputSystem.RegisterLayout<OpenXRDevice>();
            InputSystem.InputSystem.RegisterLayout<OpenXRHmd>(matches: new InputDeviceMatcher()
                .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                .WithProduct(@"Head Tracking - OpenXR")
                .WithManufacturer(@"OpenXR"));
        }

        internal static void Start()
        {
            started = true;
        }

        internal static void Stop()
        {
            s_ActionMapsToLoad.Clear();
            started = false;
        }

        static bool CheckActionName(String str, bool allowUpper = false)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!Char.IsLetter(str[i]) || (!allowUpper && Char.IsUpper(str[i])))
                    return false;
            }
            return true;
        }

        internal static bool SendActionDataToProvider()
        {
            return SendToOpenXR(s_ActionMapsToLoad);
        }

        internal static bool InstanceHasChanged()
        {
            return Input_InstanceHasChanged();
        }

        internal static bool CreateActionsAndSuggestedBindings()
        {
            return Input_CreateActionsAndSuggestedBindings();
        }

        internal static bool AttachActionSetsToSession()
        {
            return Input_AttachActionSetsToSession();
        }

        private static unsafe bool SendToOpenXR(List<OpenXRInteractionFeature.ActionMapConfig> actionMaps)
        {
            using (MemoryStream buffer = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(buffer))
                {
                    writer.Write(actionMaps.Count);
                    foreach (OpenXRInteractionFeature.ActionMapConfig map in actionMaps)
                    {
                        if (!CheckActionName(map.name))
                        {
                            Debug.LogWarning($"ActionMap named {map.name} contains invalid characters. Action Map names can only contain lower case, a-z characters. This will cause OpenXR to ignore this action map.");
                        }
                        writer.Write(map.name ?? "");
                        writer.Write(map.localizedName ?? "");
                        writer.Write(map.desiredInteractionProfile ?? "");
                        writer.Write(map.manufacturer ?? "");
                        writer.Write(map.serialNumber ?? "");

                        writer.Write(map.deviceInfos?.Count ?? 0);
                        if (map.deviceInfos != null)
                        {
                            foreach (OpenXRInteractionFeature.DeviceConfig deviceInfo in map.deviceInfos)
                            {
                                writer.Write((int)deviceInfo.characteristics);
                                writer.Write(deviceInfo.userPath ?? "");
                            }
                        }

                        writer.Write(map.actions.Count);
                        foreach (OpenXRInteractionFeature.ActionConfig action in map.actions)
                        {
                            if (!CheckActionName(action.name, true))
                            {
                                Debug.LogWarning($"Action named {action.name} contains invalid characters. Action names can only contain uppercase or lowercase letters (a-z | A-Z). This will cause OpenXR to ignore this action.");
                            }
                            writer.Write(action.name ?? "");
                            writer.Write(action.localizedName ?? "");
                            writer.Write((int)action.type);

                            //Usages
                            if (action.usages != null)
                            {
                                writer.Write(action.usages.Count);
                                foreach (string usage in action.usages)
                                {
                                    writer.Write(usage ?? "");
                                }
                            }
                            else
                            {
                                // Usages Count
                                writer.Write(0);
                            }


                            //Bindings
                            if(action.bindings != null)
                            {
                                writer.Write(action.bindings.Count);
                                foreach (OpenXRInteractionFeature.ActionBinding binding in action.bindings)
                                {
                                    writer.Write(binding.interactionProfileName ?? "");
                                    writer.Write(binding.interactionPath ?? "");

                                    if (binding.userPaths != null)
                                    {
                                        writer.Write(binding.userPaths.Count);
                                        foreach (string path in binding.userPaths)
                                        {
                                            writer.Write(path);
                                        }
                                    }
                                    else
                                    {
                                        writer.Write(map.deviceInfos?.Count ?? 0);
                                        if (map.deviceInfos != null)
                                        {
                                            foreach (OpenXRInteractionFeature.DeviceConfig deviceInfo in map.deviceInfos)
                                            {
                                                writer.Write(deviceInfo.userPath ?? "");
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Bindings Count
                                writer.Write(0);
                            }
                        }
                    }
                }
                ArraySegment<byte> data;

                if (buffer.TryGetBuffer(out data))
                {
                    return Input_SendActionDataToProvider(data.Array, (uint)data.Count);
                }
                return false;
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        const string Library = "UnityOpenXR";
        const int kMaxPathSize = 256;

        [DllImport(Library, EntryPoint = "Input_SendActionDataToProvider")]
        static extern bool Input_SendActionDataToProvider([MarshalAs(UnmanagedType.LPArray)]byte[] actionMapData, uint actionMapDataLength);

        [DllImport(Library, EntryPoint = "Input_CreateActionsAndSuggestedBindings")]
        static extern bool Input_CreateActionsAndSuggestedBindings();

        [DllImport(Library, EntryPoint = "Input_AttachActionSetsToSession")]
        static extern bool Input_AttachActionSetsToSession();

        [DllImport(Library, EntryPoint = "Input_InstanceHasChanged")]
        static extern bool Input_InstanceHasChanged();


    }
}
