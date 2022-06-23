using System;
using System.Collections;

using UnityEngine;
using UnityEngine.XR.Management;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace UnityEngine.XR.OpenXR
{
    internal class OpenXRRestarter : MonoBehaviour
    {

        internal OpenXRLoader loader = null;
        internal bool shouldRestart = true;

        internal Func<bool> ShouldCancelQuit = null;

        internal void ShutdownLoader(OpenXRLoaderBase loader, Action shutdownCallback)
        {
            StartCoroutine(Restart(loader, false, shutdownCallback, () => {}));
        }

        internal void ShutdownAndRestartLoader(OpenXRLoaderBase loader, Action shutdownCallback, Action restartCallback)
        {
            StartCoroutine(Restart(loader, true, shutdownCallback, restartCallback));
        }

        IEnumerator Restart(OpenXRLoaderBase loader, bool shouldRestart, Action shutdownCallback, Action restartCallback)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            yield return null;

            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            yield return null;

            shutdownCallback();

            if (shouldRestart)
            {
                yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
                yield return null;

                XRGeneralSettings.Instance.Manager.StartSubsystems();
                yield return null;

                if (XRGeneralSettings.Instance.Manager.activeLoader != null)
                {
                    Debug.Log("Successful restart.");
                }
                else
                {
                    Debug.LogError("Failure to restart OpenXRLoader after shutdown.");
                }

                restartCallback();
            }
            else
            {

                if (ShouldCancelQuit == null || ShouldCancelQuit.Invoke())
                {
#if UNITY_EDITOR
                    if (EditorApplication.isPlaying || EditorApplication.isPaused)
                    {
                        EditorApplication.ExitPlaymode();
                    }
#else
                    Application.Quit();
#endif
                }
            }
        }
    }
}
