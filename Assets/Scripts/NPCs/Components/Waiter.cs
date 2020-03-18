using System;
using System.Collections;
using UnityEngine;

namespace NPCs.Components
{
    public class Waiter : MonoBehaviour
    {
        public event Action OnWaitComplete;

        public IEnumerator StartWaiting(float waitDuration=1.0f)
        {
            yield return new WaitForSeconds(waitDuration);
            
            OnWaitComplete?.Invoke();
        }
    }
}