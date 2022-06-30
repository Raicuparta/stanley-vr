using System.Collections.Generic;
using InControl;
using UnityEngine;
using UnityEngine.XR;
using InputDevice = InControl.InputDevice;

namespace StanleyVr
{
	public class VrInputDevice : InputDevice
	{
		public const int MaxDevices = 10;

		public const int MaxButtons = 20;

		public const int MaxAnalogs = 20;

		// private readonly InputDeviceProfile profile;

		public override bool IsSupportedOnThisPlatform => true;

		public override bool IsKnown => true;
		
		public override int NumUnknownButtons
		{
			get
			{
				return 20;
			}
		}

		public override int NumUnknownAnalogs
		{
			get
			{
				return 20;
			}
		}

		public VrInputDevice()
		{
			AnalogSnapshot = null;
			Name = "Unknown Device";
			Meta = "\"" + "VR Controller" + "\"";
			for (int k = 0; k < NumUnknownButtons; k++)
			{
				AddControl((InputControlType)(500 + k), "Button " + k);
			}
			for (int l = 0; l < NumUnknownAnalogs; l++)
			{
				AddControl((InputControlType)(400 + l), "Analog " + l, 0.2f, 0.9f);
			}
		}

		public override void Update(ulong updateTick, float deltaTime)
		{
			for (int k = 0; k < NumUnknownButtons; k++)
			{
				UpdateWithState((InputControlType)(500 + k), ReadRawButtonState(k), updateTick, deltaTime);
			}
			for (int l = 0; l < NumUnknownAnalogs; l++)
			{
				UpdateWithValue((InputControlType)(400 + l), ReadRawAnalogValue(l), updateTick, deltaTime);
			}
		}

		public override bool ReadRawButtonState(int index)
		{
			if (index < 20)
			{
				var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
				InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);

				foreach (var device in rightHandDevices)
				{
					device.TryGetFeatureValue(CommonUsages.triggerButton, out var isPressingTrigger);
					if (isPressingTrigger)
					{
						Debug.Log($"ReadRawButtonState {index}");
						return true;
					}
				}
			}
			return false;
		}

		private enum AxisId
		{
			RotateHorizontal = 3,
			RotateVertical = 4,
			MoveVertical = 1,
			MoveHorizontal = 0
		}

		public override float ReadRawAnalogValue(int index)
		{
			if (index < 20)
			{
				var devices = new List<UnityEngine.XR.InputDevice>();

				XRNode hand;
				if (index is (int) AxisId.RotateHorizontal or (int) AxisId.RotateVertical)
				{
					hand = XRNode.RightHand;
				}
				else
				{
					hand = XRNode.LeftHand;
				}
				
				InputDevices.GetDevicesAtXRNode(hand, devices);
			
				if (devices.Count == 0) return 0;
			
				var device = devices[0];
			
				device.TryGetFeatureValue(CommonUsages.primary2DAxis, out var axis);

				if (index is (int) AxisId.RotateHorizontal or (int) AxisId.MoveHorizontal)
				{
					return axis.x;
				}
				else
				{
					return axis.y;
				}
			}
			return 0f;
		}
	}
}
