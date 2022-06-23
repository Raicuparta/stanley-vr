using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
#endif

[assembly:InternalsVisibleTo("Unity.XR.OpenXR.Tests")]
namespace UnityEngine.XR.OpenXR.Features.Mock
{
    /// <summary>
    /// Mock driver that allows tests to change the state of the mock runtime.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Mock Driver",
        Hidden = true,
        BuildTargetGroups = new []{UnityEditor.BuildTargetGroup.Standalone},
        Company = "Unity",
        Desc = "Mock driver that allows tests to change the state of the mock runtime.",
        DocumentationLink = Constants.k_DocumentationURL,
        OpenxrExtensionStrings = "XR_UNITY_mock_driver",
        Version = "0.0.1",
        FeatureId = featureId)]
#endif
    internal class MockDriver : OpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.mockdriver";

        public enum ScriptEvent
        {
            Unknown,
            EndFrame,
            HapticImpulse,
            HapticStop
        }

        public delegate void ScriptEventDelegate (ScriptEvent evt, ulong param);

        public static event ScriptEventDelegate onScriptEvent;

        [AOT.MonoPInvokeCallback(typeof(ScriptEventDelegate))]
        private static void ReceiveScriptEvent (ScriptEvent evt, ulong param) => onScriptEvent?.Invoke(evt, param);

        /// <summary>
        /// Reset the mock driver to its default state
        /// </summary>
        public static void ResetDefaults()
        {
            onScriptEvent = null;
        }

        /// <inheritdoc />
        protected override bool OnInstanceCreate(ulong instance)
        {
            if (!OpenXRRuntime.IsExtensionEnabled("XR_UNITY_mock_driver"))
            {
                Debug.LogWarning("XR_UNITY_mock_driver is not enabled, disabling Mock Driver.");
                return false;
            }

            InitializeNative(xrGetInstanceProcAddr, instance, 0ul, 0ul);

            MockDriver_RegisterScriptEventCallback(ReceiveScriptEvent);

            return true;
        }

        /// <inheritdoc />
        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            ShutdownNative(0);
        }

        internal enum XrViewConfigurationType
        {
            PrimaryMono = 1,
            PrimaryStereo = 2,
            PrimaryQuadVarjo = 1000037000,
            SecondaryMonoFirstPersonObserver = 1000054000
        }

        [Flags]
        internal enum XrSpaceLocationFlags
        {
            None = 0,
            OrientationValid = 1,
            PositionValid = 2,
            OrientationTracked = 4,
            PositionTracked = 8
        }

        [Flags]
        internal enum XrViewStateFlags
        {
            None = 0,
            OrientationValid = 1,
            PositionValid = 2,
            OrientationTracked = 4,
            PositionTracked = 8
        }

        internal enum XrResult
        {
            Success = 0,
            TimeoutExpored = 1,
            LossPending = 3,
            EventUnavailable = 4,
            SpaceBoundsUnavailable = 7,
            SessionNotFocused = 8,
            FrameDiscarded = 9,
            ValidationFailure = -1,
            RuntimeFailure = -2,
            OutOfMemory = -3,
            ApiVersionUnsupported = -4,
            InitializationFailed = -6,
            FunctionUnsupported = -7,
            FeatureUnsupported = -8,
            ExtensionNotPresent = -9,
            LimitReached = -10,
            SizeInsufficient = -11,
            HandleInvalid = -12,
            InstanceLOst = -13,
            SessionRunning = -14,
            SessionNotRunning = -16,
            SessionLost = -17,
            SystemInvalid = -18,
            PathInvalid = -19,
            PathCountExceeded = -20,
            PathFormatInvalid = -21,
            PathUnsupported = -22,
            LayerInvalid = -23,
            LayerLimitExceeded = -24,
            SpwachainRectInvalid = -25,
            SwapchainFormatUnsupported = -26,
            ActionTypeMismatch = -27,
            SessionNotReady = -28,
            SessionNotStopping = -29,
            TimeInvalid = -30,
            ReferenceSpaceUnsupported = -31,
            FileAccessError = -32,
            FileContentsInvalid = -33,
            FormFactorUnsupported = -34,
            FormFactorUnavailable = -35,
            ApiLayerNotPresent = -36,
            CallOrderInvalid = -37,
            GraphicsDeviceInvalid = -38,
            PoseInvalid = -39,
            IndexOutOfRange = -40,
            ViewConfigurationTypeUnsupported = -41,
            EnvironmentBlendModeUnsupported = -42,
            NameDuplicated = -44,
            NameInvalid = -45,
            ActionsetNotAttached = -46,
            ActionsetsAlreadyAttached = -47,
            LocalizedNameDuplicated = -48,
            LocalizedNameInvalid = -49,
            AndroidThreadSettingsIdInvalidKHR = -1000003000,
            AndroidThreadSettingsdFailureKHR = -1000003001,
            CreateSpatialAnchorFailedMSFT = -1000039001,
            SecondaryViewConfigurationTypeNotEnabledMSFT = -1000053000,
            MaxResult = 0x7FFFFFFF
        }

        /// <summary>
        /// Enumerates the possible session lifecycle states.
        /// </summary>
        internal enum XrSessionState
        {
            /// <summary>
            /// An unknown state. The runtime must not return this value in an XrEventDataSessionStateChanged event.
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// The initial state after calling xrCreateSession or returned to after calling xrEndSession.
            /// </summary>
            Idle = 1,

            /// <summary>
            /// The application is ready to call xrBeginSession and sync its frame loop with the runtime.
            /// </summary>
            Ready = 2,

            /// <summary>
            /// The application has synced its frame loop with the runtime but is not visible to the user.
            /// </summary>
            Synchronized = 3,

            /// <summary>
            /// The application has synced its frame loop with the runtime and is visible to the user but cannot receive XR input.
            /// </summary>
            Visible = 4,

            /// <summary>
            /// The application has synced its frame loop with the runtime, is visible to the user and can receive XR input.
            /// </summary>
            Focused = 5,

            /// <summary>
            /// The application should exit its frame loop and call xrEndSession.
            /// </summary>
            Stopping = 6,

            /// <summary>
            /// The session is in the process of being lost. The application should destroy the current session and can optionally recreate it.
            /// </summary>
            LossPending = 7,

            /// <summary>
            /// The application should end its XR experience and not automatically restart it.
            /// </summary>
            Exiting = 8,
        };

        internal enum XrReferenceSpaceType
        {
            View = 1,
            Local = 2,
            Stage = 3,
            UnboundedMSFT = 1000038000,
            MaxEnum = 0x7FFFFFFF
        }

        const string extLib = "mock_driver";
        [DllImport(extLib, EntryPoint = "script_initialize")]
        private static extern IntPtr InitializeNative(IntPtr xrGetInstanceProcAddr, ulong xrInstance, ulong session, ulong sceneSpace);

        [DllImport(extLib, EntryPoint = "script_shutdown")]
        private static extern IntPtr ShutdownNative(ulong xrInstance);

        [DllImport(extLib, EntryPoint="MockDriver_TransitionMockToState")]
        internal static extern XrResult TransitionToState(ulong xrSession, XrSessionState state, bool forceTransition);

        [DllImport(extLib, EntryPoint="MockDriver_SetReturnCodeForFunction")]
        internal static extern void SetReturnCodeForFunction([MarshalAs(UnmanagedType.LPStr)]string functionName, XrResult result);

        [DllImport(extLib, EntryPoint = "MockDriver_RequestExitSession")]
        internal static extern void RequestExitSession(ulong session);

        [DllImport(extLib, EntryPoint = "MockDriver_SetBlendModeOpaque")]
        internal static extern void SetBlendModeOpaque(bool opaque);

        [DllImport(extLib, EntryPoint = "MockDriver_SetReferenceSpaceBoundsRect")]
        internal static extern void SetReferenceSpaceBoundsRect(ulong session, XrReferenceSpaceType referenceSpace, Vector2 bounds);

        [DllImport(extLib, EntryPoint = "MockDriver_CauseInstanceLoss")]
        internal static extern void CauseInstanceLoss(ulong instance);

        [DllImport(extLib, EntryPoint = "MockDriver_SetSpacePose")]
        internal static extern XrResult SetSpacePose (Quaternion orientation, Vector3 position, XrSpaceLocationFlags locationSpaceFlags);

        [DllImport(extLib, EntryPoint = "MockDriver_SetViewPose")]
        internal static extern XrResult SetViewPose (int viewIndex, Quaternion orientation, Vector3 position, Vector4 fov, XrViewStateFlags viewStateFlags);

        [DllImport(extLib, EntryPoint = "MockDriver_GetEndFrameStats")]
        internal static extern XrResult GetEndFrameStats(out int primaryLayerCount, out int secondaryLayerCount);

        [DllImport(extLib, EntryPoint = "MockDriver_ActivateSecondaryView")]
        internal static extern XrResult ActivateSecondaryView(XrViewConfigurationType viewConfigurationType, bool activate);

        [DllImport(extLib, EntryPoint = "MockDriver_RegisterScriptEventCallback")]
        private static extern XrResult MockDriver_RegisterScriptEventCallback (ScriptEventDelegate callback);
    }
}
