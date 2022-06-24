using UnityEngine;

namespace StanleyVr;

public class VrUi : MonoBehaviour
{
	private Canvas canvas;
	private const string vrUiScalerName = "VrUiScaler";
	
	public static void Create(Canvas canvas)
	{
		var instance = canvas.gameObject.AddComponent<VrUi>();
		instance.canvas = canvas;
	}

	private void Start()
	{
		canvas.planeDistance = 0.2f;
		Debug.Log($"Canvas parent is {(canvas.transform.parent ? canvas.transform.parent.name : "NONE")}");
		
		SetUpScale();
	}

	private void Update()
	{
		// Inneficient way of doing this.
		// I just want to make sure the camera-space canvas use the correct camera.
		// Should be easy if we use a reference from the VrCamera behaviour.
		canvas.worldCamera = Camera.main ? Camera.main : Camera.current;
		canvas.renderMode = RenderMode.ScreenSpaceCamera;
	}

	// Need to scale the canvas parents down to make them fit in the screen.
	private void SetUpScale()
	{
		if (!transform.parent || transform.parent.name == vrUiScalerName) return;
		var vrUiParent = new GameObject(vrUiScalerName).transform;
		vrUiParent.SetParent(transform.parent, false);
		transform.SetParent(vrUiParent, true);
		vrUiParent.localScale = Vector3.one * 0.5f;
	}
}