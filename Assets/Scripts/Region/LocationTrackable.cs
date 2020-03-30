using UnityEngine;

public class LocationTrackable : MonoBehaviour
{
    private void OnEnable()
    {
        if (RegionManager.Instance) RegionManager.Instance.RegisterTrackedObject(gameObject);
    }

    private void OnDisable()
    {
        if (RegionManager.Instance) RegionManager.Instance.UnregisterTrackedObject(gameObject);
    }
}
