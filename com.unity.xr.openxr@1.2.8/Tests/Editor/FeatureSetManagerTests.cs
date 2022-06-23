using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Tests;
using Assert = UnityEngine.Assertions.Assert;

namespace UnityEditor.XR.OpenXR.Tests
{

    internal class FeatureSetTests : OpenXRLoaderSetup
    {
        const string k_KnownFeatureSetName = "Known Test";
        const string k_TestFeatureSetName = "Test Feature Set";
        const string k_TestFeatureSetNameHandAndEye = "Test Feature Set Hand and Eye Tracking";
        const string k_TestFeatureSetNameHand = "Test Feature Set Hand Tracking";
        const string k_TestFeatureSetDescription = "Test feature set";
        const string k_TestFeatureSetId = "com.unity.xr.test.featureset";
        const string k_TestFeatureSetIdTwo = "com.unity.xr.test.featureset2";
        const string k_TestFeatureSetIdThree = "com.unity.xr.test.featureset3";
        const string k_TestFeatureSetIdFour = "com.unity.xr.test.featureset4";

        [OpenXRFeatureSet(
            FeatureIds = new string[] {
                MicrosoftHandInteraction.featureId
                },
            UiName = k_TestFeatureSetName,
            Description = k_TestFeatureSetDescription,
            FeatureSetId = k_TestFeatureSetId,
            SupportedBuildTargets = new BuildTargetGroup[]{ BuildTargetGroup.Standalone },
            RequiredFeatureIds = new string[] {
                MicrosoftHandInteraction.featureId
                }
        )]
        [OpenXRFeatureSet(
            FeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
                EyeGazeInteraction.featureId,
                },
            UiName = k_TestFeatureSetNameHandAndEye,
            Description = k_TestFeatureSetDescription,
            FeatureSetId = k_TestFeatureSetIdTwo,
            SupportedBuildTargets = new BuildTargetGroup[]{ BuildTargetGroup.WSA },
            RequiredFeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
                EyeGazeInteraction.featureId,
                }
        )]
        [OpenXRFeatureSet(
            FeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
                },
            UiName = k_TestFeatureSetNameHand,
            Description = k_TestFeatureSetDescription,
            FeatureSetId = k_TestFeatureSetIdThree,
            SupportedBuildTargets = new BuildTargetGroup[]{ BuildTargetGroup.WSA },
            RequiredFeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
                }
        )]
        [OpenXRFeatureSet(
            FeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
                EyeGazeInteraction.featureId,
                },
            UiName = k_TestFeatureSetName,
            Description = k_TestFeatureSetDescription,
            FeatureSetId = k_TestFeatureSetId,
            SupportedBuildTargets = new BuildTargetGroup[]{ BuildTargetGroup.Android },
            RequiredFeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
                EyeGazeInteraction.featureId,
                }
        )]
        [OpenXRFeatureSet(
            FeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
                EyeGazeInteraction.featureId,
                HTCViveControllerProfile.featureId,
                OculusTouchControllerProfile.featureId,
                },
            UiName = k_TestFeatureSetName,
            Description = k_TestFeatureSetDescription,
            FeatureSetId = k_TestFeatureSetIdFour,
            SupportedBuildTargets = new BuildTargetGroup[]{ BuildTargetGroup.Standalone },
            RequiredFeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
                EyeGazeInteraction.featureId,
                },
            DefaultFeatureIds = new string[]{
                HTCViveControllerProfile.featureId,
            }
        )]
        sealed class TestFeatureSet {}

        Dictionary<BuildTargetGroup, Dictionary<string, bool>> cachedFeatureStates = new Dictionary<BuildTargetGroup, Dictionary<string, bool>>();


        bool FeatureIsOptional(OpenXRFeatureSetManager.FeatureSetInfo featureSet, FeatureHelpersInternal.FeatureInfo feature)
        {
            return Array.IndexOf(featureSet.featureIds, feature.Attribute.FeatureId) > -1 &&
                (featureSet.requiredFeatureIds == null || Array.IndexOf(featureSet.requiredFeatureIds, feature.Attribute.FeatureId) == -1) &&
                (featureSet.defaultFeatureIds == null || Array.IndexOf(featureSet.defaultFeatureIds, feature.Attribute.FeatureId) == -1);
        }

        public override void BeforeTest()
        {
            base.BeforeTest();
            OpenXRFeature.canSetFeatureDisabled = null;

            cachedFeatureStates.Clear();
            var buildTargetGroups = Enum.GetValues(typeof(BuildTargetGroup));
            foreach (BuildTargetGroup buildTargetGroup in buildTargetGroups)
            {
                // Enum has duplicates in it so skip those.
                if (cachedFeatureStates.ContainsKey(buildTargetGroup))
                    continue;

                var cachedFeatureState = new Dictionary<string, bool>();

                var extInfo = FeatureHelpersInternal.GetAllFeatureInfo(buildTargetGroup);
                foreach (var ext in extInfo.Features)
                {
                    cachedFeatureState.Add(ext.Attribute.FeatureId, ext.Feature.enabled);
                    ext.Feature.enabled = false;
                }

                if (cachedFeatureState.Count > 0)
                    cachedFeatureStates.Add(buildTargetGroup, cachedFeatureState);
            }

            OpenXRFeatureSetManager.InitializeFeatureSets(true);
        }

        public override void AfterTest()
        {
            base.AfterTest();
            OpenXRFeature.canSetFeatureDisabled = OpenXRFeatureSetManager.CanFeatureBeDisabled;

        }

        [Test]
        public void NoFeatureSetsReturnsEmptyList()
        {
            var featureSets = OpenXRFeatureSetManager.FeatureSetsForBuildTarget(BuildTargetGroup.iOS);
            Assert.AreEqual(0, featureSets.Count);
        }

        [Test]
        [Ignore("unstable")]
        public void FoundExpectedFeatureSets()
        {
            string[] expectedFeatuerSets = new string[]{
                k_TestFeatureSetId,
                KnownFeatureSetsContent.s_MicrosoftFeatureSetId,
#if USE_MOCK_FEATURE_SET
                "com.unity.openxr.featureset.mockruntime",
#endif //USE_MOCK_FEATURE_SET
            };

            var featureSets = OpenXRFeatureSetManager.FeatureSetsForBuildTarget(BuildTargetGroup.Standalone);
            Assert.IsNotNull(featureSets);
            Assert.AreEqual(expectedFeatuerSets.Length, featureSets.Count);

            foreach (var featureSet in featureSets)
            {
                if (Array.IndexOf(expectedFeatuerSets, featureSet.featureSetId) == -1)
                    Assert.IsTrue(false, $"Found unexpected feature set id {featureSet.featureSetId}!");
            }
        }

        [Test]
        public void UnknownFeatureSetRerturnNull()
        {
            OpenXRFeatureSetManager.InitializeFeatureSets();
            var foundFeatureSet = OpenXRFeatureSetManager.GetFeatureSetWithId(BuildTargetGroup.iOS, k_TestFeatureSetId);
            Assert.IsNull(foundFeatureSet);
            foundFeatureSet = OpenXRFeatureSetManager.GetFeatureSetWithId(BuildTargetGroup.Standalone, "BAD FEATURE SET ID");
            Assert.IsNull(foundFeatureSet);
        }

        [Test]
        public void OverrideKnownTestFeatureSet()
        {
            var foundFeatureSet = OpenXRFeatureSetManager.GetFeatureSetWithId(BuildTargetGroup.Standalone, k_TestFeatureSetId);
            Assert.IsNotNull(foundFeatureSet);
            Assert.AreEqual(0, String.Compare(foundFeatureSet.name, k_TestFeatureSetName, true));
        }


        [Test]
        public void NonoverrideKnownTestFeatureSet()
        {
            OpenXRFeatureSetManager.InitializeFeatureSets(true);
            var foundFeatureSet = OpenXRFeatureSetManager.GetFeatureSetWithId(BuildTargetGroup.WSA, k_TestFeatureSetId);
            Assert.IsNotNull(foundFeatureSet);
            Assert.AreEqual(0, String.Compare(foundFeatureSet.name, k_KnownFeatureSetName, true));
        }

        [Test]
        public void EnableFeatureSetEnablesFeatures()
        {
            OpenXRFeatureSetManager.InitializeFeatureSets(true);
            var extInfo = FeatureHelpersInternal.GetAllFeatureInfo(BuildTargetGroup.Standalone);
            foreach (var ext in extInfo.Features)
            {
                Assert.IsFalse(ext.Feature.enabled);
            }

            var foundFeatureSet = OpenXRFeatureSetManager.GetFeatureSetInfoWithId(BuildTargetGroup.Standalone, k_TestFeatureSetId);
            Assert.IsNotNull(foundFeatureSet);
            foundFeatureSet.isEnabled = true;
            foundFeatureSet.wasChanged = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Standalone);

            foreach (var ext in extInfo.Features)
            {
                if (String.Compare(ext.Attribute.FeatureId, MicrosoftHandInteraction.featureId, true) == 0)
                {
                    Assert.IsTrue(ext.Feature.enabled);
                }
                else
                {
                    Assert.IsFalse(ext.Feature.enabled);
                }
            }

        }

        [Test]
        public void DisableFeatureSetDisabledFeatures()
        {
            OpenXRFeatureSetManager.InitializeFeatureSets(true);
            var extInfo = FeatureHelpersInternal.GetAllFeatureInfo(BuildTargetGroup.Standalone);
            foreach (var ext in extInfo.Features)
            {
                Assert.IsFalse(ext.Feature.enabled);
            }


            var foundFeatureSet = OpenXRFeatureSetManager.GetFeatureSetInfoWithId(BuildTargetGroup.Standalone, k_TestFeatureSetId);
            Assert.IsNotNull(foundFeatureSet);
            foundFeatureSet.isEnabled = true;
            foundFeatureSet.wasChanged = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Standalone);

            foreach (var ext in extInfo.Features)
            {
                if (String.Compare(ext.Attribute.FeatureId, MicrosoftHandInteraction.featureId, true) == 0)
                {
                    Assert.IsTrue(ext.Feature.enabled);
                }
                else
                {
                    Assert.IsFalse(ext.Feature.enabled);
                }
            }

            foundFeatureSet.isEnabled = false;
            foundFeatureSet.wasChanged = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Standalone);

            foreach (var ext in extInfo.Features)
            {
                Assert.IsFalse(ext.Feature.enabled);
            }

        }

        [Test]
        public void DisableSharedFeaturesLeaveSharedFeaturesEnabled()
        {
            string[] allFeatureIds = new string[]{
                MicrosoftHandInteraction.featureId,
                EyeGazeInteraction.featureId,
            };

            string[] sharedFeatureIds = new string[] {
                MicrosoftHandInteraction.featureId,
            };

            OpenXRFeatureSetManager.InitializeFeatureSets(true);
            var extInfo = FeatureHelpersInternal.GetAllFeatureInfo(BuildTargetGroup.WSA);
            foreach (var ext in extInfo.Features)
            {
                Assert.IsFalse(ext.Feature.enabled);
            }

            var featureSets = OpenXRFeatureSetManager.FeatureSetInfosForBuildTarget(BuildTargetGroup.WSA);
            foreach (var featureSet in featureSets)
            {
                featureSet.isEnabled = true;
                featureSet.wasChanged = true;
            }
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.WSA);

            foreach (var ext in extInfo.Features)
            {
                if (Array.IndexOf(allFeatureIds, ext.Attribute.FeatureId) >= 0)
                {
                    Assert.IsTrue(ext.Feature.enabled);
                }
                else
                {
                    Assert.IsFalse(ext.Feature.enabled);
                }
            }

            var featureSetToDisable = OpenXRFeatureSetManager.GetFeatureSetInfoWithId(BuildTargetGroup.WSA, k_TestFeatureSetIdTwo);
            Assert.IsNotNull(featureSetToDisable);
            featureSetToDisable.isEnabled = false;
            featureSetToDisable.wasChanged = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.WSA);

            foreach (var ext in extInfo.Features)
            {
                if (Array.IndexOf(sharedFeatureIds, ext.Attribute.FeatureId) >= 0)
                {
                    Assert.IsTrue(ext.Feature.enabled);
                }
                else
                {
                    Assert.IsFalse(ext.Feature.enabled);
                }
            }


            featureSets = OpenXRFeatureSetManager.FeatureSetInfosForBuildTarget(BuildTargetGroup.WSA);
            foreach (var featureSet in featureSets)
            {
                featureSet.isEnabled = false;
                featureSet.wasChanged = true;
            }
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.WSA);

            extInfo = FeatureHelpersInternal.GetAllFeatureInfo(BuildTargetGroup.WSA);
            foreach (var ext in extInfo.Features)
            {
                Assert.IsFalse(ext.Feature.enabled);
            }

        }


        [Test]
        public void DisableSharedFeaturesLeaveOthersFeaturesEnabled()
        {
            string[] allFeatureIds = new string[]{
                MicrosoftHandInteraction.featureId,
                EyeGazeInteraction.featureId,
                MicrosoftMotionControllerProfile.featureId,
            };

            string[] otherFeatureIds = new string[] {
                MicrosoftMotionControllerProfile.featureId,
            };

            OpenXRFeatureSetManager.InitializeFeatureSets(true);
            var extInfo = FeatureHelpersInternal.GetAllFeatureInfo(BuildTargetGroup.WSA);
            foreach (var ext in extInfo.Features)
            {
                Assert.IsFalse(ext.Feature.enabled);
                if (Array.IndexOf(otherFeatureIds, ext.Attribute.FeatureId) > -1)
                    ext.Feature.enabled = true;
            }

            var featureSetToEnable = OpenXRFeatureSetManager.GetFeatureSetInfoWithId(BuildTargetGroup.WSA, k_TestFeatureSetIdTwo);
            featureSetToEnable.isEnabled = true;
            featureSetToEnable.wasChanged = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.WSA);

            foreach (var ext in extInfo.Features)
            {
                if (Array.IndexOf(allFeatureIds, ext.Attribute.FeatureId) >= 0)
                {
                    Assert.IsTrue(ext.Feature.enabled);
                }
                else
                {
                    Assert.IsFalse(ext.Feature.enabled);
                }
            }

            var featureSetToDisable = OpenXRFeatureSetManager.GetFeatureSetInfoWithId(BuildTargetGroup.WSA, k_TestFeatureSetIdTwo);
            Assert.IsNotNull(featureSetToDisable);
            featureSetToDisable.isEnabled = false;
            featureSetToDisable.wasChanged = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.WSA);

            foreach (var ext in extInfo.Features)
            {
                if (Array.IndexOf(otherFeatureIds, ext.Attribute.FeatureId) >= 0)
                {
                    Assert.IsTrue(ext.Feature.enabled);
                }
                else
                {
                    Assert.IsFalse(ext.Feature.enabled);
                }
            }
        }

        [Test]
        public void EnablingFeatureSetEnabledDefaultFeatures()
        {
            OpenXRFeatureSetManager.InitializeFeatureSets(true);
            var extInfo = FeatureHelpersInternal.GetAllFeatureInfo(BuildTargetGroup.Standalone);
            foreach (var ext in extInfo.Features)
            {
                Assert.IsFalse(ext.Feature.enabled);
            }

            var foundFeatureSet = OpenXRFeatureSetManager.GetFeatureSetInfoWithId(BuildTargetGroup.Standalone, k_TestFeatureSetIdFour);
            Assert.IsNotNull(foundFeatureSet);
            foundFeatureSet.isEnabled = true;
            foundFeatureSet.wasChanged = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Standalone);

            foreach (var ext in extInfo.Features)
            {
                if (Array.IndexOf(foundFeatureSet.featureIds, ext.Attribute.FeatureId) > -1  && !FeatureIsOptional(foundFeatureSet, ext))
                {
                    Assert.IsTrue(ext.Feature.enabled);
                }
                else
                {
                    Assert.IsFalse(ext.Feature.enabled);
                }
            }
        }

        [Test]
        public void EnablingFeatureSetLeavesOptionFeaturesAlone()
        {
            OpenXRFeatureSetManager.InitializeFeatureSets(true);
            var extInfo = FeatureHelpersInternal.GetAllFeatureInfo(BuildTargetGroup.Standalone);
            foreach (var ext in extInfo.Features)
            {
                Assert.IsFalse(ext.Feature.enabled);
            }

            var foundFeatureSet = OpenXRFeatureSetManager.GetFeatureSetInfoWithId(BuildTargetGroup.Standalone, k_TestFeatureSetIdFour);
            Assert.IsNotNull(foundFeatureSet);
            foundFeatureSet.isEnabled = true;
            foundFeatureSet.wasChanged = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Standalone);

            foreach (var ext in extInfo.Features)
            {
                if (FeatureIsOptional(foundFeatureSet, ext))
                {
                    Assert.IsFalse(ext.Feature.enabled);
                }
            }

            foundFeatureSet.isEnabled = false;
            foundFeatureSet.wasChanged = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Standalone);

            foreach (var ext in extInfo.Features)
            {
                if (FeatureIsOptional(foundFeatureSet, ext))
                {
                    Assert.IsFalse(ext.Feature.enabled);
                }
            }

            foreach (var ext in extInfo.Features)
            {
                if (FeatureIsOptional(foundFeatureSet, ext))
                {
                    ext.Feature.enabled = true;
                }
            }

            foundFeatureSet.isEnabled = true;
            foundFeatureSet.wasChanged = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Standalone);

            foreach (var ext in extInfo.Features)
            {
                if (FeatureIsOptional(foundFeatureSet, ext))
                {
                    Assert.IsTrue(ext.Feature.enabled);
                }
            }

            foundFeatureSet.isEnabled = true;
            foundFeatureSet.wasChanged = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Standalone);

            foreach (var ext in extInfo.Features)
            {
                if (FeatureIsOptional(foundFeatureSet, ext))
                {
                    Assert.IsTrue(ext.Feature.enabled);
                }
            }
        }

        [Test]
        public void DisablingFeatureSetLeavesDefaultFeaturesEnabled()
        {
            OpenXRFeatureSetManager.InitializeFeatureSets(true);
            var extInfo = FeatureHelpersInternal.GetAllFeatureInfo(BuildTargetGroup.Standalone);
            foreach (var ext in extInfo.Features)
            {
                Assert.IsFalse(ext.Feature.enabled);
            }

            var foundFeatureSet = OpenXRFeatureSetManager.GetFeatureSetInfoWithId(BuildTargetGroup.Standalone, k_TestFeatureSetIdFour);
            Assert.IsNotNull(foundFeatureSet);
            foundFeatureSet.isEnabled = true;
            foundFeatureSet.wasChanged = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Standalone);

            foreach (var ext in extInfo.Features)
            {
                if (Array.IndexOf(foundFeatureSet.featureIds, ext.Attribute.FeatureId) > -1 && !FeatureIsOptional(foundFeatureSet, ext))
                {
                    Assert.IsTrue(ext.Feature.enabled);
                }
                else
                {
                    Assert.IsFalse(ext.Feature.enabled);
                }
            }

            foundFeatureSet.isEnabled = false;
            foundFeatureSet.wasChanged = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Standalone);


            foreach (var ext in extInfo.Features)
            {
                if (Array.IndexOf(foundFeatureSet.requiredFeatureIds, ext.Attribute.FeatureId) > -1)
                {
                    Assert.IsFalse(ext.Feature.enabled);
                }
                else if (Array.IndexOf(foundFeatureSet.defaultFeatureIds, ext.Attribute.FeatureId) > -1)
                {
                    Assert.IsTrue(ext.Feature.enabled);
                }
            }
        }

        [Test]
        public void DisablingFeatureSetLeavesDisabledDefaultFeaturesDisabled()
        {
            OpenXRFeatureSetManager.InitializeFeatureSets(true);
            var extInfo = FeatureHelpersInternal.GetAllFeatureInfo(BuildTargetGroup.Standalone);
            foreach (var ext in extInfo.Features)
            {
                Assert.IsFalse(ext.Feature.enabled);
            }

            var foundFeatureSet = OpenXRFeatureSetManager.GetFeatureSetInfoWithId(BuildTargetGroup.Standalone, k_TestFeatureSetIdFour);
            Assert.IsNotNull(foundFeatureSet);
            foundFeatureSet.isEnabled = true;
            foundFeatureSet.wasChanged = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Standalone);

            foreach (var ext in extInfo.Features)
            {
                if (Array.IndexOf(foundFeatureSet.featureIds, ext.Attribute.FeatureId) > -1 && !FeatureIsOptional(foundFeatureSet, ext))
                {
                    Assert.IsTrue(ext.Feature.enabled);
                }
                else
                {
                    Assert.IsFalse(ext.Feature.enabled);
                }
            }

            foreach (var ext in extInfo.Features)
            {
                if (Array.IndexOf(foundFeatureSet.defaultFeatureIds, ext.Attribute.FeatureId) > -1)
                {
                    ext.Feature.enabled = false;
                }
            }

            foreach (var ext in extInfo.Features)
            {
                if (Array.IndexOf(foundFeatureSet.requiredFeatureIds, ext.Attribute.FeatureId) > -1)
                {
                    Assert.IsTrue(ext.Feature.enabled);
                }
                else if (Array.IndexOf(foundFeatureSet.defaultFeatureIds, ext.Attribute.FeatureId) > -1)
                {
                    Assert.IsFalse(ext.Feature.enabled);
                }
            }
        }

        [Test]
        public void CanNotChangeEnabledStateOfRequiredFeature()
        {
            OpenXRFeatureSetManager.InitializeFeatureSets(true);
            OpenXRFeatureSetManager.activeBuildTarget = BuildTargetGroup.Standalone;
            var extInfo = FeatureHelpersInternal.GetAllFeatureInfo(BuildTargetGroup.Standalone);
            foreach (var ext in extInfo.Features)
            {
                Assert.IsFalse(ext.Feature.enabled);
            }

            var foundFeatureSet = OpenXRFeatureSetManager.GetFeatureSetInfoWithId(BuildTargetGroup.Standalone, k_TestFeatureSetIdFour);
            Assert.IsNotNull(foundFeatureSet);

            foreach (var ext in extInfo.Features)
            {
                if (Array.IndexOf(foundFeatureSet.requiredFeatureIds, ext.Attribute.FeatureId) > -1)
                {
                    Assert.IsFalse(ext.Feature.enabled);
                    ext.Feature.enabled = true;
                    Assert.IsTrue(ext.Feature.enabled);
                    ext.Feature.enabled = false;
                    Assert.IsFalse(ext.Feature.enabled);
                }
            }

            foundFeatureSet.isEnabled = true;
            foundFeatureSet.wasChanged = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Standalone);
            OpenXRFeature.canSetFeatureDisabled = OpenXRFeatureSetManager.CanFeatureBeDisabled;

            foreach (var ext in extInfo.Features)
            {
                if (Array.IndexOf(foundFeatureSet.requiredFeatureIds, ext.Attribute.FeatureId) > -1)
                {
                    Assert.IsTrue(ext.Feature.enabled);
                    ext.Feature.enabled = false;
                    Assert.IsTrue(ext.Feature.enabled);
                }
            }

            foundFeatureSet.isEnabled = false;
            foundFeatureSet.wasChanged = true;
            OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Standalone);

            foreach (var ext in extInfo.Features)
            {
                if (Array.IndexOf(foundFeatureSet.requiredFeatureIds, ext.Attribute.FeatureId) > -1)
                {
                    Assert.IsFalse(ext.Feature.enabled);
                    ext.Feature.enabled = true;
                    Assert.IsTrue(ext.Feature.enabled);
                    ext.Feature.enabled = false;
                    Assert.IsFalse(ext.Feature.enabled);
                }
            }
            OpenXRFeatureSetManager.activeBuildTarget = BuildTargetGroup.Unknown;

        }

    }
}