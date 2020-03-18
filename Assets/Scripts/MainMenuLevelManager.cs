using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuLevelManager : MonoBehaviour
{
    public List<GameObject> objectsToTurnAround;

    private void OnTriggerExit(Collider other)
    {
        print(other.gameObject.name);
        print(objectsToTurnAround.Contains(other.gameObject));
        if (objectsToTurnAround.Contains(other.gameObject))
        {
            other.transform.Rotate(Vector3.up, 180f);
        }
    }
}
