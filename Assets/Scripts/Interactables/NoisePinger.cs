using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoisePinger : MonoBehaviour
{
    [Header("General")]
    public GameObject pingPrefab;
    public float pingRadius = 10.0f;
    
    [Header("Tweaks")]
    public float pingGrowthScale = 20.0f;
    public float pingFloorOffset = 0.5f;
    
    public void SpawnNoisePing(Collision other)
    {
        Vector3 hitPoint = other.GetContact(0).point;
        Vector3 hitNormal = other.GetContact(0).normal;
        
        GameObject pingInstance = Instantiate(pingPrefab, hitPoint + (pingFloorOffset * hitNormal), Quaternion.LookRotation(hitPoint));
        Utils.GetRequiredComponent<NoisePing>(pingInstance).Initialize(pingRadius, pingGrowthScale);
    }
    
    void OnDrawGizmos()
    {
        // pingRadius visualization
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, pingRadius);
    }
}
