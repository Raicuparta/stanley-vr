using UnityEngine;
using UnityEngine.SpatialTracking;
using Valve.VR;

namespace StanleyVr;

public class VrHand: MonoBehaviour
{
    
    public static void Create(Camera camera, SteamVR_Input_Sources inputSource)
    {
        var hand = new GameObject($"VrHand-{inputSource.ToString()}");
        hand.SetActive(false);
        var instance = hand.AddComponent<VrHand>();
        hand.transform.SetParent(camera.transform.parent, false);
        hand.transform.localPosition = Vector3.down;
        hand.transform.localRotation = Quaternion.identity;

        if (inputSource == SteamVR_Input_Sources.RightHand)
        {
            var laser = VrLaser.Create(hand.transform);
            laser.SetUp(camera);
        }


        var poseDriver = hand.AddComponent<TrackedPoseDriver>();
        poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController,
            inputSource == SteamVR_Input_Sources.RightHand ?
	        TrackedPoseDriver.TrackedPose.RightPose : TrackedPoseDriver.TrackedPose.LeftPose);
        poseDriver.UseRelativeTransform = true;

        // var pose = hand.AddComponent<SteamVR_Behaviour_Pose>();
        // pose.inputSource = inputSource;
        // pose.poseAction = SteamVR_Actions.default_Pose;

        var renderModel = new GameObject("RenderModel").AddComponent<SteamVR_RenderModel>();
        renderModel.transform.SetParent(hand.transform, false);
        renderModel.createComponents = true;
        renderModel.updateDynamically = true;
        renderModel.SetInputSource(inputSource);
        renderModel.index = inputSource == SteamVR_Input_Sources.RightHand
            ? SteamVR_TrackedObject.EIndex.Device1
            : SteamVR_TrackedObject.EIndex.Device2;
        
        hand.SetActive(true);
    }
}