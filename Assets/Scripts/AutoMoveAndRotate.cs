using System;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class AutoMoveAndRotate : MonoBehaviour
    {
        public Vector3andSpace moveUnitsPerSecond;
        public Vector3andSpace rotateDegreesPerSecond;
        public float timeBeforeSwapDirection = -1;
        public float bufferTime = -1;
        private float timeSinceSwap = 0;
        private bool directionIsForward = true;
        private bool waitingForBuffer = false;

        // Update is called once per frame
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            timeSinceSwap += deltaTime;

            if (waitingForBuffer)
            {
                if (timeSinceSwap >= bufferTime)
                {
                    waitingForBuffer = false;
                    timeSinceSwap = 0;
                }
                return;
            }

            if (timeBeforeSwapDirection != -1 && timeSinceSwap >= timeBeforeSwapDirection)
            {
                timeSinceSwap = 0;
                directionIsForward = !directionIsForward;
                if (bufferTime != -1) waitingForBuffer = true;
                return;
            }
            if (!directionIsForward) deltaTime *= -1;
            if (bufferTime != -1)
            transform.Translate(moveUnitsPerSecond.value*deltaTime, moveUnitsPerSecond.space);
            transform.Rotate(rotateDegreesPerSecond.value*deltaTime, moveUnitsPerSecond.space);
        }


        [Serializable]
        public class Vector3andSpace
        {
            public Vector3 value;
            public Space space = Space.Self;
        }
    }
}
