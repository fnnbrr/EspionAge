using UnityEngine;

public class LocationTrackable : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log(gameObject.name);
        if (RegionManager.Instance)
        {
            RegionManager.Instance.RegisterTrackedObject(gameObject);
            Debug.Log("added");
            Debug.Log(RegionManager.Instance.trackedObjectZones[gameObject][0]);
        }
    }

    private void OnDisable()
    {
        if (RegionManager.Instance) RegionManager.Instance.UnregisterTrackedObject(gameObject);
    }
}
