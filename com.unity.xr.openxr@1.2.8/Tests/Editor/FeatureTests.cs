using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.Mock;
using UnityEngine.XR.OpenXR.Tests;
using Assert = UnityEngine.Assertions.Assert;

namespace UnityEditor.XR.OpenXR.Tests
{
    internal class FeatureTests : OpenXRLoaderSetup
    {
        [Test]
        public void EnableFeatures()
        {
            var extInfo = FeatureHelpersInternal.GetAllFeatureInfo(BuildTargetGroup.Standalone);
            extInfo.Features.SingleOrDefault(ext => ext.Attribute.UiName == "Mock Runtime").Feature.enabled = true;
            Assert.IsTrue(MockRuntime.Instance.enabled);

            extInfo.Features.SingleOrDefault(ext => ext.Attribute.UiName == "Mock Runtime").Feature.enabled = false;
            Assert.IsFalse(MockRuntime.Instance.enabled);
        }

        [Test]
        public void CheckDefaultValues()
        {
            var extInfo = FeatureHelpersInternal.GetAllFeatureInfo(BuildTargetGroup.Standalone);
            var mockExtInfo = extInfo.Features.SingleOrDefault(ext => ext.Attribute.UiName == "Mock Runtime");

            Assert.AreEqual(mockExtInfo.Attribute.UiName, mockExtInfo.Feature.nameUi);
            Assert.AreEqual(mockExtInfo.Attribute.Version, mockExtInfo.Feature.version);
            Assert.AreEqual(mockExtInfo.Attribute.OpenxrExtensionStrings, mockExtInfo.Feature.openxrExtensionStrings);
        }

        [Test]
        public void ValidationError()
        {
            bool errorFixed = false;

            // Set up a validation check ...
            MockRuntime.Instance.TestCallback = (s, o) =>
            {
                if (s == "GetValidationChecks")
                {
                    var validationChecks = o as List<OpenXRFeature.ValidationRule>;
                    validationChecks?.Add(new OpenXRFeature.ValidationRule
                    {
                        message = "Mock Validation Fail",
                        checkPredicate = () => errorFixed,
                        fixIt = () => errorFixed = true,
                        error = true
                    });
                }

                return true;
            };

            // Try to build the player ...
            var report = zBuildHookTests.BuildMockPlayer();

            // It will fail because of the above validation issue ...
            Assert.AreEqual(BuildResult.Failed, report.summary.result);

            // There's one validation issue ...
            var validationIssues = new List<OpenXRFeature.ValidationRule>();
            OpenXRProjectValidation.GetCurrentValidationIssues(validationIssues, BuildTargetGroup.Standalone);
            Assert.AreEqual(1, validationIssues.Count);

            // Fix it ...
            Assert.IsFalse(errorFixed);
            validationIssues[0].fixIt.Invoke();
            Assert.IsTrue(errorFixed);

            // Now there's zero validation issues ...
            OpenXRProjectValidation.GetCurrentValidationIssues(validationIssues, BuildTargetGroup.Standalone);
            Assert.AreEqual(0, validationIssues.Count);

            // Close the validation window ...
            OpenXRProjectValidationWindow.CloseWindow();
        }

        [Test]
        public void GetFeatureByFeatureId()
        {
            var feature = FeatureHelpers.GetFeatureWithIdForActiveBuildTarget(MockRuntime.featureId);
            Assert.IsNotNull(feature);
        }

        [Test]
        public void GetFeatureByUnknownFeatureIdReturnsNull()
        {
            var feature = FeatureHelpers.GetFeatureWithIdForActiveBuildTarget("some.unknown.feature.id");
            Assert.IsNull(feature);

            feature = FeatureHelpers.GetFeatureWithIdForActiveBuildTarget("");
            Assert.IsNull(feature);

            feature = FeatureHelpers.GetFeatureWithIdForActiveBuildTarget(null);
            Assert.IsNull(feature);
        }

        [Test]
        public void GetFeaturesWithIdsReturnsFeatures()
        {
            var featureIds = new string[] { MockRuntime.featureId, EyeGazeInteraction.featureId };
            var features = FeatureHelpers.GetFeaturesWithIdsForActiveBuildTarget(featureIds);
            Assert.IsNotNull(features);
            Assert.IsTrue(features.Length == 2);

            var expectedTypes = new Type[]{ typeof(MockRuntime), typeof(EyeGazeInteraction)};
            foreach (var feature in features)
            {
                Assert.IsTrue(Array.IndexOf(expectedTypes, feature.GetType()) > -1);
            }
        }

        [Test]
        public void InteractionFeatureLayoutRegistration()
        {
            var packageSettings = OpenXRSettings.GetPackageSettings();
            Assert.IsNotNull(packageSettings);

            // Ignore the test if there is not more than 1 build target.
            var features = packageSettings.GetFeatures<OculusTouchControllerProfile>().Select(f => f.feature).ToArray();
            if(features.Length < 2)
                return;

            // Disable all of the oculus interaction features
            foreach (var feature in features)
            {
                feature.enabled = false;
            }

            // Make sure the oculus device layout is not registered
            NUnit.Framework.Assert.Throws(typeof(ArgumentException), () => UnityEngine.InputSystem.InputSystem.LoadLayout<OculusTouchControllerProfile.OculusTouchController>());

            // Enable one of the features and make sure the layout is registered
            features[0].enabled = true;
            NUnit.Framework.Assert.DoesNotThrow(() => UnityEngine.InputSystem.InputSystem.LoadLayout<OculusTouchControllerProfile.OculusTouchController>());
            NUnit.Framework.Assert.DoesNotThrow(() => UnityEngine.InputSystem.InputSystem.LoadLayout<OculusTouchControllerProfile.OculusTouchController>());

            // Enable a second feature and make sure the layout is still enabled
            features[1].enabled = true;
            NUnit.Framework.Assert.DoesNotThrow(() => UnityEngine.InputSystem.InputSystem.LoadLayout<OculusTouchControllerProfile.OculusTouchController>());

            // Disable the first feature and make sure the layout is still enabled
            features[0].enabled = false;
            NUnit.Framework.Assert.DoesNotThrow(() => UnityEngine.InputSystem.InputSystem.LoadLayout<OculusTouchControllerProfile.OculusTouchController>());

            // Disable the second feature and make sure the layout is no longer
            features[1].enabled = false;
            NUnit.Framework.Assert.Throws(typeof(ArgumentException), () => UnityEngine.InputSystem.InputSystem.LoadLayout<OculusTouchControllerProfile.OculusTouchController>());
        }
    }
}