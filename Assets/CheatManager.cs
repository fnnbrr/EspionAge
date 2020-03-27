using System.Collections;
using UnityEngine;

public class CheatManager : MonoBehaviour
{
    public GameObject teleportMarker;

    private GameObject curTeleportMarker;
    private Camera mainCamera;
    private LayerMask terrainMask;

    private void Start()
    {
        mainCamera = CameraManager.Instance.brain.gameObject.GetComponent<Camera>();
        terrainMask = LayerMask.GetMask("Terrain");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            // start painting red X for teleport location
            curTeleportMarker = Instantiate(teleportMarker, GameManager.Instance.GetPlayerTransform());
            StartCoroutine(UpdateTeleportPosition());
        }
        
        else if (Input.GetMouseButtonUp(2))
        {
            StopCoroutine(UpdateTeleportPosition());
            GameManager.Instance.GetPlayerTransform().position = curTeleportMarker.transform.position;
            Destroy(curTeleportMarker);
        }
    }

    private IEnumerator UpdateTeleportPosition()
    {
        while (Input.GetMouseButton(2))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainMask))
            {
                curTeleportMarker.transform.position = hit.point;
            }
            
            yield return new WaitForEndOfFrame();
        }
    }
}
