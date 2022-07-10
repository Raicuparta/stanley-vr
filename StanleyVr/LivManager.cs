using System.Linq;
using StanleyVr.VrStage;
using UnityEngine;

namespace StanleyVr;

public class LivManager: MonoBehaviour
{
	private LIV.SDK.Unity.LIV liv;
	
    public static void Create(VrStageController stage)
    {
	    var instance = new GameObject("LivManager").AddComponent<LivManager>();
		instance.transform.SetParent(stage.transform, false);
    }

    private void Update()
    {
	    if (Input.GetKeyDown(KeyCode.F4))
	    {
		    SetUpLiv();
	    }
    }

    private void SetUpLiv()
    {
	    if (liv)
		{
			Debug.Log("#### LIV already exists, destroying");
			Destroy(liv);
		}
		Debug.Log($"Attempting to create LIV with camera {Camera.main.name}");

		var cameraObject = new GameObject("LIVCamera");
		cameraObject.SetActive(false);
		cameraObject.AddComponent<Camera>();

		gameObject.SetActive(false);
		
		liv = gameObject.AddComponent<LIV.SDK.Unity.LIV>();
		liv.stage = transform;
		liv.HMDCamera = Camera.main;
		liv.fixPostEffectsAlpha = true;
		liv.spectatorLayerMask = -1;
		liv.excludeBehaviours = liv.excludeBehaviours.Concat(new[]
		{
			"MainCamera",
			"AudioListener",
			"FlareLayer",
			"CanvasRenderer",
			"EnableDepthOnHighQuality",
			"TrackedPoseDriver"
		}).ToArray();

		gameObject.SetActive(true);

		Debug.Log("Successfully created LIV");
    }
}