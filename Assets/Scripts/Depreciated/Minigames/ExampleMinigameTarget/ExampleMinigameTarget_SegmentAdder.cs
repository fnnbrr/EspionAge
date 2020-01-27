using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleMinigameTarget_SegmentAdder : MonoBehaviour
{
    public int segmentValue;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            MinigameStaminaManager.Instance.IncreaseStaminaBySegments(segmentValue);
            Destroy(gameObject);
        }
    }
}
