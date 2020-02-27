using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoisePing : MonoBehaviour
{
    private float pingGrowthScale;
    private float lifetime;

    private bool initialized = false;

    public void Initialize(float radius = 10f, float growthScale = 20f)
    {
        pingGrowthScale = growthScale;

        lifetime = 2 * radius / pingGrowthScale;
        Destroy(gameObject, lifetime);

        initialized = true;
    }

    void FixedUpdate()
    {
        if (!initialized) return;

        transform.localScale += new Vector3(Time.fixedDeltaTime * pingGrowthScale, 0f, Time.fixedDeltaTime * pingGrowthScale);
    }
}
