using UnityEngine;

namespace StanleyVr;

public class StereoPortalRenderer: MonoBehaviour
{
    public float stereoSeparation;

    private void OnPreRender()
    {
	    var camera = Camera.current;
	    var cameraRight = camera.transform.right;
	    // Not really sure why I have to divide by 4. Probably related to the camera scale?
	    var separation = stereoSeparation > 0 ? stereoSeparation : camera.stereoSeparation / 4f;
	    var offset = cameraRight.normalized * separation;

	    if (camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
	    {
		    offset *= -1;
	    }

	    var cameraPosition = camera.transform.position;
	    camera.transform.position = cameraPosition + offset;
	    
	    foreach (var portal in MainCamera.Portals)
	    {
		    if (!portal.disabled)
		    {
			    portal.Render();
		    }
	    }

	    foreach (var portal in MainCamera.Portals)
	    {
		    if (!portal.disabled)
		    {
			    portal.PostPortalRender();
		    }
	    }

	    camera.transform.position = cameraPosition;
    }
}