// using System;
// using UnityEngine;
//
// namespace StanleyVr;
//
// public class VrUi : MonoBehaviour
// {
// 	private Camera uiCamera;
// 	public static VrUi Instance; // TODO no public singletons.
//
// 	public static VrUi Create()
// 	{
// 		var instance = Instantiate(VrAssetManager.VrUi);
// 		DontDestroyOnLoad(instance);
// 		return instance.AddComponent<VrUi>();
// 	}
//
// 	private void Awake()
// 	{
// 		Instance = this;
// 		DontDestroyOnLoad(gameObject);
// 		uiCamera = GetComponentInChildren<Camera>();
// 	}
//
// 	private void OnDisable()
// 	{
// 		throw new Exception("Disabling VrUI");
// 	}
//
// 	private void Start()
// 	{
// 		SetUp();
// 		SetUpCamera(null);
// 	}
//
// 	private void Update()
// 	{
// 		if (Input.GetKeyDown(KeyCode.F2))
// 		{
// 			Debug.Log("## setting up vr ui");
// 			SetUp();
// 		}
// 	}
//
// 	private void SetUp()
// 	{
// 		var canvases = FindObjectsOfType<Canvas>();
// 		foreach (var canvas in canvases)
// 		{
// 			SetUpCanvas(canvas);
// 		}
// 	}
//
// 	public void SetUpCamera(Camera camera)
// 	{
// 		Debug.Log($"Setting up VrUi with camera {camera}");
//
// 		var mainCamera = camera;
// 		if (!mainCamera)
// 		{
// 			mainCamera = new GameObject("VrCamera").AddComponent<Camera>();
// 			mainCamera.tag = "MainCamera";
// 		}
//
// 		transform.SetParent(mainCamera.transform, false);
//
// 		// VrHand.Create(mainCamera, SteamVR_Input_Sources.RightHand);
// 		// VrHand.Create(mainCamera, SteamVR_Input_Sources.LeftHand);
// 	}
//
// 	public void SetUpCanvas(Canvas canvas)
// 	{
// 		canvas.worldCamera = uiCamera;
// 		canvas.renderMode = RenderMode.ScreenSpaceCamera;
//
// 		if (canvas.gameObject.layer != LayerMask.NameToLayer("Default") ||
// 		    canvas.gameObject.layer != LayerMask.NameToLayer("UI"))
// 		{
// 			Debug.LogWarning($"Warning: changing canvas layer from {LayerMask.LayerToName(canvas.gameObject.layer)} to UI");
// 		}
//
// 		canvas.gameObject.layer = LayerMask.NameToLayer("UI");
// 	}
// }