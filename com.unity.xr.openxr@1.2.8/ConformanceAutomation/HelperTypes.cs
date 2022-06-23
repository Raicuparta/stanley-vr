/// <summary>
/// OpenXR types as C# structs to pass to an OpenXR runtime
/// </summary>
///

namespace UnityEngine.XR.OpenXR.Features.ConformanceAutomation
{
    struct XrVector2f
    {
        float x;
        float y;

        public XrVector2f(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public XrVector2f(Vector2 value)
        {
            x = value.x;
            y = value.y;
        }
    };

    struct XrVector3f
    {
        float x;
        float y;
        float z;

        public XrVector3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = -z;
        }

        public XrVector3f(Vector3 value)
        {
            x = value.x;
            y = value.y;
            z = -value.z;
        }
    };

    struct XrQuaternionf
    {
        float x;
        float y;
        float z;
        float w;

        public XrQuaternionf(float x, float y, float z, float w)
        {
            this.x = -x;
            this.y = -y;
            this.z = z;
            this.w = w;
        }

        public XrQuaternionf(Quaternion quaternion)
        {
            this.x = -quaternion.x;
            this.y = -quaternion.y;
            this.z = quaternion.z;
            this.w = quaternion.w;
        }
    };

    struct XrPosef
    {
        XrQuaternionf orientation;
        XrVector3f position;

        public XrPosef(Vector3 vec3, Quaternion quaternion)
        {
            this.position = new XrVector3f(vec3);
            this.orientation = new XrQuaternionf(quaternion);
        }
    };
}
