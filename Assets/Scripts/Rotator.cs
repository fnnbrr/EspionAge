using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Range(0.0f, 360.0f)]
    public float rotationSpeed = 90.0f;

    private void FixedUpdate()
    {
        transform.RotateAround(transform.position, transform.up, (Time.deltaTime * rotationSpeed));
    }
}
