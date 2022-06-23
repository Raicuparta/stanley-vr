using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

[assembly:InternalsVisibleTo("Unity.XR.OpenXR.Tests")]
[assembly:InternalsVisibleTo("Unity.XR.OpenXR.Tests.Editor")]
namespace UnityEngine.XR.OpenXR.Features.Mock
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Mock Runtime",
        BuildTargetGroups = new []{UnityEditor.BuildTargetGroup.Standalone},
        Company = "Unity",
        Desc = "Mock runtime extension for automated testing.",
        DocumentationLink = Constants.k_DocumentationURL,
        CustomRuntimeLoaderBuildTargets = new [] { UnityEditor.BuildTarget.StandaloneWindows64, UnityEditor.BuildTarget.StandaloneOSX },
        OpenxrExtensionStrings = MockRuntime.XR_UNITY_null_gfx,
        Version = "0.0.2",
        FeatureId = featureId)]
#endif
    internal class MockRuntime : OpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.mockruntime";

        /// <summary>
        /// Don't fail to build if there are validation errors.
        /// </summary>
        public bool ignoreValidationErrors = false;

#if UNITY_INCLUDE_TESTS
        [NonSerialized] public Func<string, object, object> TestCallback = (methodName, param) => true;

        public const string XR_UNITY_mock_test = "XR_UNITY_mock_test";

        public const string XR_UNITY_null_gfx = "XR_UNITY_null_gfx";

        public ulong XrInstance { get; private set; } = 0ul;

        public ulong XrSession { get; private set; } = 0ul;

        public int XrSessionState { get; private set; } = 0;

        protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
        {
            var ret = TestCallback(MethodBase.GetCurrentMethod().Name, func);
            if (!(ret is IntPtr))
                return func;
            return (IntPtr)ret;
        }

        protected override void OnSystemChange(ulong xrSystem)
        {
            TestCallback(MethodBase.GetCurrentMethod().Name, xrSystem);
        }

        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            var result = (bool)TestCallback(MethodBase.GetCurrentMethod().Name, xrInstance);
            if (result)
            {
                XrInstance = xrInstance;
                XrSessionState = 0;
            }

            return result;
        }

        protected override void OnSessionCreate(ulong xrSession)
        {
            XrSession = xrSession;
            TestCallback(MethodBase.GetCurrentMethod().Name, xrSession);
        }

        protected override void OnSessionBegin (ulong xrSession)
        {
            TestCallback(MethodBase.GetCurrentMethod().Name, xrSession);
        }

        protected override void OnAppSpaceChange (ulong xrSpace)
        {
            TestCallback(MethodBase.GetCurrentMethod().Name, xrSpace);
        }

        protected override void OnSessionEnd (ulong xrSession)
        {
            TestCallback(MethodBase.GetCurrentMethod().Name, xrSession);
        }

        protected override void OnSessionDestroy (ulong session)
        {
            TestCallback(MethodBase.GetCurrentMethod().Name, session);
            XrSession = 0ul;
            XrSessionState = 0;
        }

        protected override void OnInstanceDestroy (ulong instance)
        {
            TestCallback(MethodBase.GetCurrentMethod().Name, instance);
            XrInstance = 0ul;
        }

        protected override void OnSessionLossPending (ulong xrSession)
        {
            TestCallback(MethodBase.GetCurrentMethod().Name, xrSession);
        }

        protected override void OnInstanceLossPending (ulong xrInstance)
        {
            TestCallback(MethodBase.GetCurrentMethod().Name, xrInstance);
        }

        protected override void OnSubsystemCreate()
        {
            TestCallback(MethodBase.GetCurrentMethod().Name, 0);
        }

        protected override void OnSubsystemStart()
        {
            TestCallback(MethodBase.GetCurrentMethod().Name, 0);
        }

        protected override void OnSessionExiting(ulong xrSession)
        {
            TestCallback(MethodBase.GetCurrentMethod().Name, 0);
        }

        protected override void OnSubsystemDestroy()
        {
            TestCallback(MethodBase.GetCurrentMethod().Name, 0);
        }

        protected override void OnSubsystemStop()
        {
            TestCallback(MethodBase.GetCurrentMethod().Name, 0);
        }

        protected override void OnFormFactorChange(int xrFormFactor)
        {
            TestCallback(MethodBase.GetCurrentMethod().Name, xrFormFactor);
        }

        protected override void OnEnvironmentBlendModeChange(int xrEnvironmentBlendMode)
        {
            TestCallback(MethodBase.GetCurrentMethod().Name, xrEnvironmentBlendMode);
        }

        protected override void OnViewConfigurationTypeChange(int xrViewConfigurationType)
        {
            TestCallback(MethodBase.GetCurrentMethod().Name, xrViewConfigurationType);
        }

        internal class XrSessionStateChangedParams
        {
            public int OldState;
            public int NewState;
        }

        protected override void OnSessionStateChange(int oldState, int newState)
        {
            XrSessionState = newState;
            TestCallback(MethodBase.GetCurrentMethod().Name, new XrSessionStateChangedParams() {OldState = oldState, NewState = newState});
        }

#if UNITY_EDITOR
        protected override void GetValidationChecks(List<ValidationRule> results, BuildTargetGroup target)
        {
            if (ignoreValidationErrors)
                results.Clear();
            TestCallback(MethodBase.GetCurrentMethod().Name, results);
        }
#endif

        public static MockRuntime Instance => OpenXRSettings.Instance.GetFeature<MockRuntime>();
#endif
    }
}
