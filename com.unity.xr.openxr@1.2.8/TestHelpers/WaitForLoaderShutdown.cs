using System;


namespace UnityEngine.XR.OpenXR.TestHelpers
{

    internal sealed class WaitForLoaderShutdown : CustomYieldInstruction
    {
        OpenXRLoader loader;
        private bool didShutdown = false;

        private float timeout = 0;

        public WaitForLoaderShutdown(OpenXRLoader loader)
        {
            this.loader = loader;
            loader.onAutoShutdown += OnLoaderShutdown;
            timeout = Time.realtimeSinceStartup + 2.0f;
        }

        public void OnLoaderShutdown(object ldr)
        {
            OpenXRLoader oxrldr = ldr as OpenXRLoader;
            if (oxrldr == this.loader) didShutdown = true;
        }

        bool ShouldKeepWaiting()
        {
            if (Time.realtimeSinceStartup > timeout)
            {
                Debug.LogError($"Timeout waiting for loader shutdown.");
                loader.onAutoShutdown -= OnLoaderShutdown;
                throw new TimeoutException();
            }

            if (didShutdown)
            {
                loader.onAutoShutdown -= OnLoaderShutdown;
            }
            return !didShutdown;
        }

        public override bool keepWaiting
        {
            get { return ShouldKeepWaiting(); }
        }
    }
}