using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Mock;
using UnityEngine.XR.OpenXR.TestHelpers;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

using XrSessionState = UnityEngine.XR.OpenXR.Features.Mock.MockDriver.XrSessionState;
using XrResult = UnityEngine.XR.OpenXR.Features.Mock.MockDriver.XrResult;

namespace UnityEngine.XR.OpenXR.Tests
{
    internal class MockDriverRuntimeTests : OpenXRLoaderSetup
    {
        [UnityTest]
        public IEnumerator TestMockDriverStateTransition()
        {
            var states = new List<XrSessionState>();
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                if (methodName == nameof(OpenXRFeature.OnSessionStateChange))
                {
                    var oldState = (XrSessionState)((MockRuntime.XrSessionStateChangedParams) param).OldState;
                    var newState = (XrSessionState)((MockRuntime.XrSessionStateChangedParams) param).NewState;
                    CheckValidStateTransition(oldState, newState);
                    states.Add(newState);
                }

                return true;
            };

            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.InitializeAndStart();

            yield return null;

            Assert.AreEqual(10, MockRuntime.Instance.XrInstance);

            bool ret = MockDriver.TransitionToState(MockRuntime.Instance.XrSession, XrSessionState.Visible, false) == XrResult.Success;

            yield return null;

            Assert.IsTrue(ret);
            Assert.AreEqual(XrSessionState.Visible, states[states.Count - 1]);
        }

        [UnityTest]
        public IEnumerator TestMockDriverForcedStateTransition()
        {
            var states = new List<XrSessionState>();
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                if (methodName == nameof(OpenXRFeature.OnSessionStateChange))
                {
                    var oldState = (XrSessionState)((MockRuntime.XrSessionStateChangedParams) param).OldState;
                    var newState = (XrSessionState)((MockRuntime.XrSessionStateChangedParams) param).NewState;
                    states.Add(newState);
                }

                return true;
            };

            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.InitializeAndStart();

            yield return null;

            Assert.AreEqual(10, MockRuntime.Instance.XrInstance);
            Assert.AreEqual(XrSessionState.Focused, states[states.Count - 1]);

            var wait = new WaitForLoaderShutdown(Loader);

            var ret = MockDriver.TransitionToState(MockRuntime.Instance.XrSession, XrSessionState.LossPending, true) == XrResult.Success;
            Assert.IsTrue(ret);

            yield return wait;

            Assert.AreEqual(XrSessionState.LossPending, states[states.Count - 1]);
        }

        [UnityTest]
        public IEnumerator TestLossPendingCausesSessionDestroyAndRestart()
        {
            bool lossPendingReceived = false;
            bool sessionDestroyed = false;
            bool sessionCreated = false;
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                switch(methodName)
                {
                    case nameof(OpenXRFeature.OnSessionLossPending):
                        lossPendingReceived = true;
                        break;

                    case nameof(OpenXRFeature.OnSessionDestroy):
                        sessionDestroyed = true;
                        sessionCreated = false;
                        break;

                    case nameof(OpenXRFeature.OnSessionCreate):
                        sessionCreated = true;
                        break;
                }

                return true;
            };

            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.InitializeAndStart();

            yield return null;

            Assert.AreEqual(10, MockRuntime.Instance.XrInstance);

            var wait = new WaitForLoaderRestart(Loader);

            var ret = MockDriver.TransitionToState(MockRuntime.Instance.XrSession, XrSessionState.LossPending, true) == XrResult.Success;

            yield return wait;

            Assert.IsTrue(lossPendingReceived);
            Assert.IsTrue(sessionDestroyed);
            Assert.IsTrue(sessionCreated);

        }

        [UnityTest]
        public IEnumerator TestCreateSessionFailure()
        {
            bool sawCreateSession = false;
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                if (methodName == nameof(OpenXRFeature.OnSessionCreate))
                {
                    sawCreateSession = true;
                }

                return true;
            };

            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.Initialize();

            yield return null;

            MockDriver.SetReturnCodeForFunction("xrCreateSession", XrResult.RuntimeFailure);

            base.Start();

            yield return null;

            Assert.IsFalse(sawCreateSession);
        }

        static XrResult[] beginSessionSuccessResults = new XrResult[]
        {
            XrResult.Success,
            XrResult.LossPending
        };

        [UnityTest]
        public IEnumerator TestBeginSessionSuccessWithValues([ValueSource("beginSessionSuccessResults")]
            XrResult successResult)
        {
            var states = new List<XrSessionState>();
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                if (methodName == nameof(OpenXRFeature.OnSessionStateChange))
                {
                    var newState = (XrSessionState)((MockRuntime.XrSessionStateChangedParams) param).NewState;
                    states.Add(newState);
                }

                return true;
            };

            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.Initialize();

            yield return null;

            MockDriver.SetReturnCodeForFunction("xrBeginSession", successResult);

            base.Start();

            yield return null;

            Assert.IsTrue(base.IsRunning<XRDisplaySubsystem>());

            switch (successResult)
            {
                case XrResult.Success:
                    Assert.IsTrue(states.Contains(XrSessionState.Ready));
                    Assert.IsTrue(states.Contains(XrSessionState.Synchronized));
                    Assert.IsTrue(states.Contains(XrSessionState.Visible));
                    Assert.IsTrue(states.Contains(XrSessionState.Focused));
                    break;

                case XrResult.LossPending:
                    Assert.IsTrue(states.Contains(XrSessionState.Ready));
                    Assert.IsFalse(states.Contains(XrSessionState.Synchronized));
                    Assert.IsFalse(states.Contains(XrSessionState.Visible));
                    Assert.IsFalse(states.Contains(XrSessionState.Focused));
                    break;
            }
        }

        [UnityTest]
        public IEnumerator TestBeginSessionFailure()
        {
            var states = new List<XrSessionState>();
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                if (methodName == nameof(OpenXRFeature.OnSessionStateChange))
                {
                    var newState = (XrSessionState)((MockRuntime.XrSessionStateChangedParams) param).NewState;
                    states.Add(newState);
                }

                return true;
            };

            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.Initialize();

            yield return null;

            MockDriver.SetReturnCodeForFunction("xrBeginSession", XrResult.RuntimeFailure);

            base.Start();

            yield return null;

            Assert.IsTrue(base.IsRunning<XRDisplaySubsystem>());

            Assert.IsTrue(states.Contains(XrSessionState.Ready));
            Assert.IsFalse(states.Contains(XrSessionState.Synchronized));
            Assert.IsFalse(states.Contains(XrSessionState.Visible));
            Assert.IsFalse(states.Contains(XrSessionState.Focused));
        }

#if false
        // TEST WAS UNSTABLE, DISABLING FOR RELEASE ONLY
        [UnityTest]
        public IEnumerator TestRequestExitShutsdownSubsystems()
        {
            bool sawSessionDestroy = false;
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                if (methodName == nameof(OpenXRFeature.OnSessionDestroy))
                {
                    sawSessionDestroy = true;
                }

                return true;
            };

            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.InitializeAndStart();

            yield return null;

            Assert.IsTrue(base.IsRunning<XRDisplaySubsystem>());

            var wait = new WaitForLoaderShutdown(Loader);
            MockDriver.RequestExitSession(MockRuntime.Instance.XrSession);

            yield return wait;

            Assert.IsTrue(sawSessionDestroy);
        }
#endif

        [UnityTest]
        public IEnumerator RestartAfterExitSession()
        {
            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.InitializeAndStart();

            yield return null;

            Assert.AreEqual(OpenXRLoader.LoaderState.Started, Loader.currentLoaderState);

            var wait = new WaitForLoaderShutdown(Loader);

            MockDriver.RequestExitSession(MockRuntime.Instance.XrSession);

            yield return wait;

            Assert.AreEqual(OpenXRLoader.LoaderState.Uninitialized, Loader.currentLoaderState);

            base.InitializeAndStart();

            yield return null;

            Assert.AreEqual(OpenXRLoader.LoaderState.Started, Loader.currentLoaderState);
        }


        [UnityTest]
        public IEnumerator CheckInstanceLossRecieved()
        {
            bool instanceLost = false;
            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                if (methodName == "OnInstanceLossPending")
                {
                    instanceLost = true;
                }

                return true;
            };

            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.InitializeAndStart();

            yield return null;

            MockDriver.CauseInstanceLoss(MockRuntime.Instance.XrInstance);

            yield return new WaitForLoaderShutdown(Loader);

            Assert.IsTrue(instanceLost);
        }

        [UnityTest]
        public IEnumerator DisplayTransparent()
        {
            AddExtension(MockRuntime.XR_UNITY_mock_test);

            MockRuntime.Instance.TestCallback = (methodName, param) =>
            {
                if (methodName == nameof(OpenXRFeature.OnInstanceCreate))
                {
                    MockDriver.SetBlendModeOpaque(false);
                }

                return true;
            };

            base.InitializeAndStart();

            yield return null;
            var displays = new List<XRDisplaySubsystem>();
            SubsystemManager.GetInstances(displays);
            Assert.AreEqual(false, displays[0].displayOpaque);
        }

        [UnityTest]
        public IEnumerator DisplayOpaque()
        {
            base.InitializeAndStart();

            yield return null;
            var displays = new List<XRDisplaySubsystem>();
            SubsystemManager.GetInstances(displays);
            Assert.AreEqual(true, displays[0].displayOpaque);
        }

        [UnityTest]
        public IEnumerator XRInputReturnsCorrectBoundaryPoints()
        {
            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.InitializeAndStart();

            yield return null;

            Assert.IsTrue(base.IsRunning<XRInputSubsystem>(), "Input subsystem failed to properly start!");

            MockDriver.SetReferenceSpaceBoundsRect(MockRuntime.Instance.XrSession, MockDriver.XrReferenceSpaceType.Stage, new Vector2 {x = 1.0f, y = 3.0f});

            yield return null;

            var input = Loader.GetLoadedSubsystem<XRInputSubsystem>();
            Assert.That(() => input, Is.Not.Null);

            input.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);

            yield return null;

            var points = new List<Vector3>();
            Assert.IsTrue(input.TryGetBoundaryPoints(points), "Failed to get boundary points!");
            Assert.That(() => points.Count, Is.EqualTo(4), "Incorrect number of boundary points received!");

            var comparer = new Vector3EqualityComparer(10e-6f);

            Assert.That(points[0], Is.EqualTo(new Vector3 {x = -1.0f, y = 0.0f, z = -3.0f}).Using(comparer));
            Assert.That(points[1], Is.EqualTo(new Vector3 {x = -1.0f, y = 0.0f, z = 3.0f}).Using(comparer));
            Assert.That(points[2], Is.EqualTo(new Vector3 {x = 1.0f, y = 0.0f, z = 3.0f}).Using(comparer));
            Assert.That(points[3], Is.EqualTo(new Vector3 {x = 1.0f, y = 0.0f, z = -3.0f}).Using(comparer));
        }

        [UnityTest]
        public IEnumerator XRInputReturnsNothingIfNoBoundaryPointsAreSet()
        {
            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.InitializeAndStart();

            yield return null;

            Assert.IsTrue(base.IsRunning<XRInputSubsystem>(), "Input subsystem failed to properly start!");

            var input = Loader.GetLoadedSubsystem<XRInputSubsystem>();
            Assert.That(() => input, Is.Not.Null);

            input.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);

            yield return null;

            var points = new List<Vector3>();
            Assert.IsTrue(input.TryGetBoundaryPoints(points), "Failed to get boundary points!");
            Assert.That(() => points.Count, Is.EqualTo(0), "Incorrect number of boundary points received!");
        }

        [UnityTest]
        public IEnumerator XRInputReturnsTheCorrectBoundaryForTheTrackingOriginMode()
        {
            AddExtension(MockRuntime.XR_UNITY_mock_test);

            base.InitializeAndStart();

            yield return null;

            Assert.IsTrue(base.IsRunning<XRInputSubsystem>(), "Input subsystem failed to properly start!");

            MockDriver.SetReferenceSpaceBoundsRect(MockRuntime.Instance.XrSession, MockDriver.XrReferenceSpaceType.Stage, new Vector2 {x = 1.0f, y = 3.0f});

            yield return null;

            var input = Loader.GetLoadedSubsystem<XRInputSubsystem>();
            Assert.That(() => input, Is.Not.Null);

            input.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);

            yield return null;

            var points = new List<Vector3>();
            Assert.IsTrue(input.TryGetBoundaryPoints(points), "Failed to get boundary points!");
            Assert.That(() => points.Count, Is.EqualTo(4), "Incorrect number of boundary points received!");

            input.TrySetTrackingOriginMode(TrackingOriginModeFlags.Device);

            yield return null;

            points.Clear();
            Assert.IsTrue(input.TryGetBoundaryPoints(points), "Failed to get boundary points!");
            Assert.That(() => points.Count, Is.EqualTo(0), "Incorrect number of boundary points received!");
        }
    }
}
