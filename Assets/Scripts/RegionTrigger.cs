using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionTrigger : MonoBehaviour
{
    public delegate void PlayerPassedThroughAction();
    public event PlayerPassedThroughAction OnPlayerPassThrough;
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            OnPlayerPassThrough?.Invoke();
        }
    }
}
