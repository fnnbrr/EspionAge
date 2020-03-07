using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaunchArcRenderer : MonoBehaviour
{
    [Range(1.0f, 10.0f)] public float throwSensitivity = 2.0f;
    
    public float velocity;
    public float angle;
    public int resolution = 10;

    private float g;  // force of gravity on the y-axis
    private float radianAngle;

    private LineRenderer lr;
    private Camera mainCamera;
    private Plane mouseHitPlane;
    private Vector3 mousePosition;

    private void Awake()
    {
        lr = Utils.GetRequiredComponent<LineRenderer>(this);
        g = Mathf.Abs(Physics.gravity.y);
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
        mainCamera = Camera.main;
        mouseHitPlane = new Plane(Vector3.up, Vector3.zero);
        mousePosition = transform.position;
    }

    // populating the line renderer withthe appropriate settings
    public void RenderArc()
    {
        lr.positionCount = resolution + 1;

        Vector3[] arcArray = CalculateArcArray();
        lr.SetPositions(arcArray);
    }
    
    public void RenderArc(float minAngle, float maxAngle)
    {
        angle = Mathf.Clamp(throwSensitivity * Vector3.Distance(transform.position, mousePosition), minAngle, maxAngle);
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

    private void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if(mouseHitPlane.Raycast(ray, out float enter));
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            
            mousePosition.x = hitPoint.x;
            mousePosition.z = hitPoint.z;
            
            transform.LookAt(mousePosition);
            transform.Rotate(0, 270, 0);
        }
    }
}
