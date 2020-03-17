using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace NPCs.Components
{
    public class Waiter : MonoBehaviour
    {
        private float waitTimer = 0f;

        public bool WaitComplete(float waitDuration=1.0f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer < waitDuration)
            {
                // baseStateAi.ToggleAnimations(false); TODO replace with Event
                return false;
            }

            // baseStateAi.ToggleAnimations(true); TODO replace with Event
            waitTimer = 0f;
            return true;
        }
    }
}