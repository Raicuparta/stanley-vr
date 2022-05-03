using UnityEngine;
using UnityEngine.SpatialTracking;

namespace StanleyVr;

public class VrHand: MonoBehaviour
{
    public static void Create(Camera camera)
    {
        var hand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hand.name = "VrRightHand";
        hand.transform.SetParent(camera.transform.parent, false);
        hand.transform.localScale = Vector3.one * 0.05f;
        hand.transform.localPosition = Vector3.down;
        hand.transform.localRotation = Quaternion.identity;

        var laser = VrLaser.Create(hand.transform);
        laser.SetUp(camera);


        var poseDriver = hand.AddComponent<TrackedPoseDriver>();
        poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController,
	        TrackedPoseDriver.TrackedPose.RightPose);
        poseDriver.UseRelativeTransform = true;
    }
}