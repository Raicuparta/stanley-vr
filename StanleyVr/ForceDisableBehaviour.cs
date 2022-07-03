using UnityEngine;

namespace StanleyVr;

public class ForceDisableBehaviour: MonoBehaviour
{
    public Behaviour behaviour;
    
    public void LateUpdate()
    {
        behaviour.enabled = false;
    }
}