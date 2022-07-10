using UnityEngine;
using UnityEngine.SceneManagement;

namespace StanleyVr.VrUi;

public class MainMenuSceneFix: MonoBehaviour
{
    public static void Create()
    {
        new GameObject("VrMainMenuSceneFix").AddComponent<MainMenuSceneFix>();
    }
    
    private void Awake()
    {
	    EnableMenuScene();
        
		SceneManager.activeSceneChanged += OnSceneChanged;
    }

	private void OnSceneChanged(Scene arg0, Scene arg1)
	{
		EnableMenuScene();
	}

	// TODO: make sure to not enable this scene if other menu scenes are active.
	private void EnableMenuScene()
	{
		var backgroundScenes = GameObject.Find("Background Scenes");
        if (!backgroundScenes) return;

        var cameras = backgroundScenes.GetComponentsInChildren<Camera>(true);
        foreach (var camera in cameras)
        {
	        // Depth was 100 for some reason in these cameras,
	        // so I'm lowering it to make sure these cameras draw below the UI camera.
	        // The UI camera depth can't be increased due to some weird issue.
	        camera.depth = 1;
        }

        var menuBackground = backgroundScenes.transform.Find("MenuBackground-PC");
        
        menuBackground.Find("PC Background Camera").gameObject.SetActive(true);
        menuBackground.Find("Menu_StanleysOffice_DYNAMIC").gameObject.SetActive(true);
        menuBackground.Find("Menu_StanleysOffice_STATIC").gameObject.SetActive(true);
	}
}