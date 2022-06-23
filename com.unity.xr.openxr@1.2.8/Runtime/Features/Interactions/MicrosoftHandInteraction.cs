using System.Collections.Generic;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;

#if UNITY_EDITOR
using UnityEditor;
#endif

using PoseControl = UnityEngine.XR.OpenXR.Input.PoseControl;

namespace UnityEngine.XR.OpenXR.Features.Interactions
{
    /// <summary>
    /// This <see cref="OpenXRInteractionFeature"/> enables the use of eye gaze interaction profiles in OpenXR. It enables <see href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_MSFT_hand_interaction">XR_MSFT_hand_interaction</see> in the underyling runtime.
    /// This creates a new <see cref="InputDevice"/> with the <see cref="InputDeviceCharacteristics.HandTracking"/> characteristic.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.XR.OpenXR.Features.OpenXRFeature(UiName = "Microsoft Hand Interaction Profile",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA, BuildTargetGroup.Android },
        Company = "Unity",
        Desc = "Allows for mapping input to the hand interaction profile. Will register the controller map for hand interaction if enabled.",
        DocumentationLink = Constants.k_DocumentationManualURL + "features/microsofthandinteraction.html",
        Version = "0.0.1",
        OpenxrExtensionStrings = extensionString,
        Category = UnityEditor.XR.OpenXR.Features.FeatureCategory.Interaction,
        FeatureId = featureId)]
#endif
    public class MicrosoftHandInteraction : OpenXRInteractionFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.input.handtracking";

        /// <summary>
        /// An Input System device based off the hand interaction profile in the <see href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_MSFT_hand_interaction">Hand Interaction Extension</see>. Enabled through <see cref="MicrosoftHandInteraction"/>.
        /// </summary>
        [Preserve, InputControlLayout(displayName = "Hololens Hand (OpenXR)", commonUsages = new[] { "LeftHand", "RightHand" })]
        public class HoloLensHand : XRController
        {
            /// <summary>
            /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="MicrosoftHandInteraction.select"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "Primary", "selectbutton" })]
            public AxisControl select { get; private set; }

            /// <summary>
            /// An [AxisControl](xref:UnityEngine.InputSystem.Controls.AxisControl) that represents the <see cref="MicrosoftHandInteraction.select"/> OpenXR binding.
            /// </summary>
            [Preserve, InputControl(aliases = new[] { "Secondary" })]
            public AxisControl squeeze { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the Microsoft Hand Interaction devicePose OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0, alias = "device")]
            public PoseControl devicePose { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) that represents the Microsoft Hand Interaction pointer OpenXR binding.
            /// </summary>
            [Preserve, InputControl(offset = 0)]
            public PoseControl pointer { get; private set; }

            /// <summary>
            /// A [ButtonControl](xref:UnityEngine.InputSystem.Controls.ButtonControl) required for backwards compatibility with the XRSDK layouts. This represents the overall tracking state of the device. This value is equivalent to mapping devicePose/isTracked.
            /// </summary>
            [Preserve, InputControl(offset = 8)]
            new public ButtonControl isTracked { get; private set; }

            /// <summary>
            /// A [IntegerControl](xref:UnityEngine.InputSystem.Controls.IntegerControl) required for backwards compatibility with the XRSDK layouts. This represents the bit flag set to indicate what data is valid. This value is equivalent to mapping devicePose/trackingState.
            /// </summary>
            [Preserve, InputControl(offset = 12)]
            new public IntegerControl trackingState { get; private set; }

            /// <summary>
            /// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for backwards compatibility with the XRSDK layouts. This is the device position, or grip position. This value is equivalent to mapping devicePose/position.
            /// </summary>
            [Preserve, InputControl(offset = 16, aliases = new[] { "gripPosition" })]
            new public Vector3Control devicePosition { get; private set; }

            /// <summary>
            /// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the device orientation, or grip orientation. This value is equivalent to mapping devicePose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 28, aliases = new[] { "gripOrientation" })]
            new public QuaternionControl deviceRotation { get; private set; }

            /// <summary>
            /// A [Vector3Control](xref:UnityEngine.InputSystem.Controls.Vector3Control) required for backwards compatibility with the XRSDK layouts. This is the pointer position. This value is equivalent to mapping pointerPose/position.
            /// </summary>
            [Preserve, InputControl(offset = 76)]
            public Vector3Control pointerPosition { get; private set; }

            /// <summary>
            /// A [QuaternionControl](xref:UnityEngine.InputSystem.Controls.QuaternionControl) required for backwards compatibility with the XRSDK layouts. This is the pointer rotation. This value is equivalent to mapping pointerPose/rotation.
            /// </summary>
            [Preserve, InputControl(offset = 88, aliases = new[] { "pointerOrientation" })]
            public QuaternionControl pointerRotation { get; private set; }

            /// <summary>
            /// Internal call used to assign controls to the the correct element.
            /// </summary>
            protected override void FinishSetup()
            {
                base.FinishSetup();
                select = GetChildControl<AxisControl>("select");
                squeeze = GetChildControl<AxisControl>("squeeze");
                devicePose = GetChildControl<PoseControl>("devicePose");
                pointer = GetChildControl<PoseControl>("pointer");

                isTracked = GetChildControl<ButtonControl>("isTracked");
                trackingState = GetChildControl<IntegerControl>("trackingState");
                devicePosition = GetChildControl<Vector3Control>("devicePosition");
                deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");
                pointerPosition = GetChildControl<Vector3Control>("pointerPosition");
                pointerRotation = GetChildControl<QuaternionControl>("pointerRotation");
            }
        }

        /// <summary>The OpenXR Extension string. OpenXR uses this to check if this extension is available or enabled. See <see href="https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_MSFT_hand_interaction">hand interaction extension</see> documentation for more information on this OpenXR extension.</summary>
        public const string extensionString = "XR_MSFT_hand_interaction";

        /// <summary>
        /// OpenXR string that represents the hand interaction profile.
        /// </summary>
        public const string profile = "/interaction_profiles/microsoft/hand_interaction";

        /// <summary>
        /// Constant for a float interaction binding '.../input/select/value' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string select = "/input/select/value";

        /// <summary>
        /// Constant for a float interaction binding '.../input/menu/value' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string squeeze = "/input/squeeze/value";

        /// <summary>
        /// Constant for a pose interaction binding '.../input/grip/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string grip = "/input/grip/pose";

        /// <summary>
        /// Constant for a pose interaction binding '.../input/aim/pose' OpenXR Input Binding. Used by input subsystem to bind actions to physical inputs.
        /// </summary>
        public const string aim = "/input/aim/pose";

        private const string kDeviceLocalizedName = "HoloLens Hand OpenXR";

        /// <summary>
        /// Registers the <see cref="Hand"/> layout with the Input System.
        /// </summary>
        protected override void RegisterDeviceLayout()
        {
            InputSystem.InputSystem.RegisterLayout(typeof(HoloLensHand),
                        matches: new InputDeviceMatcher()
                        .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                        .WithProduct(kDeviceLocalizedName));
        }

        /// <summary>
        /// Removes the <see cref="Hand"/> layout from the Input System.
        /// </summary>
        protected override void UnregisterDeviceLayout()
        {
            InputSystem.InputSystem.RemoveLayout(typeof(HoloLensHand).Name);
        }

        /// <inheritdoc/>
        protected override void RegisterActionMapsWithRuntime()
        {
            ActionMapConfig actionMap = new ActionMapConfig()
            {
                name = "microsofthandinteraction",
                localizedName = kDeviceLocalizedName,
                desiredInteractionProfile = profile,
                manufacturer = "Microsoft",
                serialNumber = "",
                deviceInfos = new List<DeviceConfig>()
                {
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left),
                        userPath = UserPaths.leftHand
                    },
                    new DeviceConfig()
                    {
                        characteristics = (InputDeviceCharacteristics)(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right),
                        userPath = UserPaths.rightHand
                    }
                },
                actions = new List<ActionConfig>()
                {
                    // Select
                    new ActionConfig()
                    {
                        name = "select",
                        localizedName = "Select",
                        type = ActionType.Binary,
                        usages = new List<string>()
                        {
                            "PrimaryButton"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = select,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Squeeze
                    new ActionConfig()
                    {
                        name = "squeeze",
                        localizedName = "Squeeze",
                        type = ActionType.Axis1D,
                        usages = new List<string>()
                        {
                            "Grip"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = squeeze,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Device Pose
                    new ActionConfig()
                    {
                        name = "devicePose",
                        localizedName = "Device Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Device"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = grip,
                                interactionProfileName = profile,
                            }
                        }
                    },
                    // Pointer Pose
                    new ActionConfig()
                    {
                        name = "pointer",
                        localizedName = "Pointer Pose",
                        type = ActionType.Pose,
                        usages = new List<string>()
                        {
                            "Pointer"
                        },
                        bindings = new List<ActionBinding>()
                        {
                            new ActionBinding()
                            {
                                interactionPath = aim,
                                interactionProfileName = profile,
                            }
                        }
                    }
                }
            };

            AddActionMap(actionMap);
        }
    }
}
