using System;
using UnityEngine;

namespace StanleyVr;

public class VrLaser : MonoBehaviour
{
    private const float laserLength = 1f;
    private bool ignoreNextInput;
    public static VrLaser Instance; // TODO no public singletons.

    // private LaserInputModule inputModule;
    private LineRenderer lineRenderer;
    // private Vector3? target;

    public static VrLaser Create(Transform dominantHand)
    {
        Debug.Log("Creting laser...");
        
        var instance = new GameObject("VrHandLaser").AddComponent<VrLaser>();
        var instanceTransform = instance.transform;
        instanceTransform.SetParent(dominantHand, false);
        instanceTransform.localEulerAngles = new Vector3(39.132f, 356.9302f, 0.3666f);
        return instance;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void SetUp(Camera camera)
    {
        // inputModule = LaserInputModule.Create(this);
        // inputModule.EventCamera = camera;
        // target = null;
    }

    private void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.SetPositions(new[] {Vector3.zero, Vector3.forward * laserLength});
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.001f;
        lineRenderer.material.shader = Shader.Find("Stanley/Misc/ToolsTransparent");
        lineRenderer.sortingOrder = 10000;
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        UpdateLaserVisibility();
        UpdateLaserTarget();
    }

    public void SetTarget(Vector3? newTarget)
    {
        // target = newTarget;
    }

    private void UpdateLaserTarget()
    {
        // lineRenderer.SetPosition(1,
        //     target != null
        //         ? transform.InverseTransformPoint((Vector3) target)
        //         : Vector3.forward * laserLength);
    }

    // private bool HasCurrentTarget()
    // {
    //     return target != null;
    // }

    private void UpdateLaserVisibility()
    {
        lineRenderer.enabled = true; // todo
    }

    public bool ClickDown()
    {
        if (ignoreNextInput) return false;
        return Input.GetKeyDown(KeyCode.Space);
    }

    public bool ClickUp()
    {
        if (ignoreNextInput)
        {
            ignoreNextInput = false;
            return false;
        }

        return Input.GetKeyUp(KeyCode.Space);
    }

    public bool IsClicking()
    {
        return Input.GetKey(KeyCode.Space);;
    }
}