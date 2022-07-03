using StanleyVr.VrCamera;
using UnityEngine;

namespace StanleyVr.VrUi;

public class VrUiController : MonoBehaviour
{
	private Canvas canvas;
	private const string vrUiScalerName = "VrUiScaler";
	
	public static void Create(Canvas canvas)
	{
		var instance = canvas.gameObject.AddComponent<VrUiController>();
		instance.canvas = canvas;
	}

	private void Start()
	{
		canvas.planeDistance = 1f;
		canvas.scaleFactor = 1.3f;
		gameObject.layer = LayerMask.NameToLayer("UI");
		Debug.Log($"Canvas parent is {(canvas.transform.parent ? canvas.transform.parent.name : "NONE")}");
		
		SetUpScale();
	}

	private void Update()
	{
		if (!VrUiManager.Instance) return;
		canvas.worldCamera = VrUiManager.Instance.UiCamera;
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