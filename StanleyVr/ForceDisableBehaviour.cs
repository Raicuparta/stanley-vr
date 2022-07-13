using UnityEngine;

namespace StanleyVr;

public class ForceDisableBehaviour: MonoBehaviour
{
    public Behaviour behaviour;
    
    public void LateUpdate()
    {
        if (!behaviour)
        {
            Destroy(this);
        }
        behaviour.enabled = false;
    }
}