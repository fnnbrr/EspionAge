using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaunchArcRenderer : MonoBehaviour
{
    public float velocity;
    public float angle;
    public int resolution = 10;

    public GameObject endTargetDisplayPrefab;
    private GameObject endTargetDisplay;

    private float g;  // force of gravity on the y-axis
    private float radianAngle;

    private LineRenderer lr;

    private void Awake()
    {
        lr = Utils.GetRequiredComponent<LineRenderer>(this);
        g = Mathf.Abs(Physics.gravity.y);

        if (endTargetDisplayPrefab)
        {
            endTargetDisplay = Instantiate(endTargetDisplayPrefab, Vector3.zero, endTargetDisplayPrefab.transform.rotation, transform.parent);
        }
    }

    private void OnValidate()
    {
        if (lr != null && Application.isPlaying)
        {
            RenderArc();
        }
    }

    void Start()
    {
        RenderArc();
    }

    // populating the line renderer withthe appropriate settings
    public void RenderArc()
    {
        lr.positionCount = resolution + 1;

        Vector3[] arcArray = CalculateArcArray();
        lr.SetPositions(arcArray);

        if (endTargetDisplay)
        {
            // Set the z position of the endTargetDisplay to the x of the last point (because of darn parent-relative rotations), or zero
            endTargetDisplay.transform.localPosition = new Vector3(0f, 0f, (arcArray?[arcArray.Length - 1] ?? Vector3.zero).x);
        
            // Only have the display active if the velocity or angle are both not 0
            endTargetDisplay.SetActive(!Mathf.Approximately(velocity, 0f) && !Mathf.Approximately(angle, 0f));
        }
    }
    
    public void RenderArc(float newAngle)
    {
        angle = newAngle;
        RenderArc();
    }

    Vector3[] CalculateArcArray()
    {
        Vector3[] arcArray = new Vector3[resolution + 1];

        radianAngle = Mathf.Deg2Rad * angle;
        float maxDistance = (velocity * velocity * Mathf.Sin(2 * radianAngle)) / g;
        
        for (int i = 0; i <= resolution; i++)
        {
            float t = (float)i / resolution;
            arcArray[i] = CalculateArcPoint(t, maxDistance);
        }

        return arcArray;
    }

    Vector3 CalculateArcPoint(float t, float maxDistance)
    {
        float x = t * maxDistance;
        float y = x * Mathf.Tan(radianAngle) - ((g * x * x) / (2 * velocity * velocity * Mathf.Cos(radianAngle) * Mathf.Cos(radianAngle)));
        return new Vector3(x, y);
    }
}
