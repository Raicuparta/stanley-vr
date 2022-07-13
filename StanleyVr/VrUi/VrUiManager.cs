using UnityEngine;
using UnityEngine.UI;

namespace StanleyVr.VrUi;

public class VrUiManager: MonoBehaviour
{
    public Camera UiCamera { get; private set; }
    public static VrUiManager Instance { get; private set; }
    public const float UiCameraDepth = 4;
    private GameMaster gameMaster;
    
    public static void Create(GameMaster gameMaster)
    {
        if (!gameMaster) return;
        if (Instance) Destroy(Instance.gameObject);
        Instance = new GameObject("VrUiManager").AddComponent<VrUiManager>();
        Instance.transform.SetParent(gameMaster.transform, false);
        Instance.gameMaster = gameMaster;
    }

    private void Awake()
    {
        UiCamera = gameObject.AddComponent<Camera>();
        UiCamera.cullingMask = LayerMask.GetMask("UI");
        UiCamera.clearFlags = CameraClearFlags.Nothing;
        
        // High depth to make this camera draw on top of the others.
        // Making depth higher than 5 flips the VR camera?? What.
        UiCamera.depth = UiCameraDepth;
    }

    private void Start()
    {
        // Fade ins/outs would show as a scare in the middle of the screen due to how VR UI is done.
        // Scaling it up a ton is a quick fix to make it cover the whole screen.
        gameMaster.fadeImage.transform.localScale = Vector3.one * 100;
    }
}