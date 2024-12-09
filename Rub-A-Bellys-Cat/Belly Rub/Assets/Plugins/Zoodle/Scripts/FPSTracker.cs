using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zoodle
{
    /// <summary>
    /// Component that tracks average FPS.
    /// </summary>
    public class FPSTracker : MonoBehaviour
    {
        [ShowInInspector]
        protected bool ShowDebugWindow { get; private set; }

        [DisplayInDebugWindow]
        public float FPS { get; private set; } = 0.0f;

        public override string ToString() => FPS.ToString();

        private float deltaTime = 0.0f;
        private float fpsSum = 0.0f;
        private int frameCount = 0;

        private void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;

            fpsSum += fps;
            frameCount++;

            if (frameCount > 180)
            {
                FPS = fpsSum / frameCount;
                fpsSum = 0.0f;
                frameCount = 0;
            }
        }

        private void OnGUI()
        {
            if (ShowDebugWindow)
                DebugUtility.DisplayDebugWindow(this);
        }
    }
}
