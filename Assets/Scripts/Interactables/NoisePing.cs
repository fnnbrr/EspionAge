using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoisePing : MonoBehaviour
{
    [Header("General")]
    public GameObject pingPrefab;
    public float pingRadius = 10.0f;
    
    [Header("Tweaks")]
    public float pingGrowthScale = 20.0f;
    public float pingFloorOffset = 0.5f;
    
    private GameObject pingInstance;
    
    public void SpawnNoisePing(Collision other)
    {
        Vector3 hitPoint = other.GetContact(0).point;
        hitPoint.y = pingFloorOffset;  // so we always spawn right above the floor
        
        pingInstance = Instantiate(pingPrefab, hitPoint, Quaternion.LookRotation(Vector3.forward));
        
        float pingDuration = 2 * pingRadius / pingGrowthScale;
        Destroy(pingInstance, pingDuration);
    }

    private void FixedUpdate()
    {
        if (!pingInstance) return;

        pingInstance.transform.localScale += new Vector3(Time.deltaTime * pingGrowthScale, 0f, 
                                                         Time.deltaTime * pingGrowthScale);
    }
    
    void OnDrawGizmos()
    {
        // pingRadius visualization
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pingRadius);
    }
}
