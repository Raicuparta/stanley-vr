using System;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Unity.XR.OpenXR.Tests")]
namespace UnityEngine.XR.OpenXR.TestHelpers
{

    internal sealed class WaitForLoaderRestart : CustomYieldInstruction
    {
        OpenXRLoader loader;
        bool didShutdown = false;
        bool didRestart = false;

        private float timeout = 0;

        public WaitForLoaderRestart(OpenXRLoader loader)
        {
            this.loader = loader;
            loader.onAutoShutdown += OnLoaderShutdown;
            loader.onAutoRestart += OnLoaderRestart;
            loader.onAutoShutdown += OnLoaderShutdown;

            timeout = Time.realtimeSinceStartup + 5.0f;
        }

        public void OnLoaderShutdown(object ldr)
        {
            OpenXRLoader oxrldr = ldr as OpenXRLoader;
            if (oxrldr == this.loader) didShutdown = true;
        }

        public void OnLoaderRestart(object ldr)
        {
            OpenXRLoader oxrldr = ldr as OpenXRLoader;
            if (oxrldr == this.loader) didRestart = true;
        }

        bool ShouldKeepWaiting()
        {
            if (Time.realtimeSinceStartup > timeout)
            {
                Debug.LogError($"Timeout waiting for loader shutdown.");
                loader.onAutoShutdown -= OnLoaderShutdown;
                loader.onAutoRestart -= OnLoaderRestart;
                throw new TimeoutException();
            }

            if (didShutdown)
            {
                loader.onAutoShutdown -= OnLoaderShutdown;
            }

            if (didRestart)
            {
                loader.onAutoRestart -= OnLoaderRestart;
            }

            return !(didShutdown && didRestart);
        }

        public override bool keepWaiting
        {
            get { return ShouldKeepWaiting(); }
        }
    }
}