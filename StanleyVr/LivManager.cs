using System.Linq;
using StanleyVr.VrStage;
using UnityEngine;

namespace StanleyVr;

public class LivManager: MonoBehaviour
{
	private LIV.SDK.Unity.LIV liv;
	private MainCamera mainCamera;
	
    public static void Create(MainCamera mainCamera)
    {
	    var instance = new GameObject("LivManager").AddComponent<LivManager>();
		instance.transform.SetParent(VrStageController.Instance.transform, false);
		instance.mainCamera = mainCamera;
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

	    gameObject.SetActive(false);
		
		liv = gameObject.AddComponent<LIV.SDK.Unity.LIV>();
		var camera = mainCamera.GetComponent<Camera>();
		liv.stage = transform;
		liv.HMDCamera = camera;
		liv.fixPostEffectsAlpha = true;
		liv.spectatorLayerMask = camera.cullingMask | 1 << LayerMask.NameToLayer("Bucket");
		liv.excludeBehaviours = liv.excludeBehaviours.Concat(new[]
		{
			"MainCamera",
			"AudioListener",
			"FlareLayer",
			"CanvasRenderer",
			"EnableDepthOnHighQuality",
			"TrackedPoseDriver",
			"VrCameraController"
		}).ToArray();

		gameObject.SetActive(true);

		Debug.Log("Successfully created LIV");
    }
}