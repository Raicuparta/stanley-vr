using System.Diagnostics;
using UnityEngine.XR.OpenXR.Features.Mock;
using NUnit.Framework;

namespace UnityEngine.XR.OpenXR.Tests
{
    internal class WaitForXrFrame  : CustomYieldInstruction
    {
        private int m_Frames = 0;
        private long m_Timeout;
        private Stopwatch m_Timer;

        public override bool keepWaiting
        {
            get
            {
                if (m_Frames <= 0)
                    return false;

                if (m_Timer.ElapsedMilliseconds < m_Timeout)
                    return true;

                MockDriver.onScriptEvent -= OnScriptEvent;
                Assert.Fail("WaitForXrFrame: Timeout");
                return false;
            }
        }

        public WaitForXrFrame(int frames, float timeout = 10.0f)
        {
            m_Frames = frames;
            m_Timeout = (long)(timeout * 1000.0);
            if (frames == 0)
                return;

            // Start waiting for a new frame count
            var driver = OpenXRSettings.Instance.GetFeature<MockDriver>();
            Assert.IsNotNull(driver, "MockDriver feature not found");
            Assert.IsTrue(driver.enabled, "MockDriver feature must be enabled to use WaitForXrFrame");

            MockDriver.onScriptEvent += OnScriptEvent;

            m_Timer = new Stopwatch();
            m_Timer.Restart();
        }

        private void OnScriptEvent(MockDriver.ScriptEvent evt, ulong param)
        {
            m_Frames--;
            if (m_Frames > 0)
                return;

            m_Frames = 0;
            MockDriver.onScriptEvent -= OnScriptEvent;
        }
    }
}