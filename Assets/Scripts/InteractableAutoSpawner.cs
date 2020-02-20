using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractableType
{
    Interactable,
    Throwable
}

public class InteractableAutoSpawner : MonoBehaviour
{
    public GameObject prefab;
    public int spawnCount = 5;
    [SerializeField] public InteractableType interactableType;

    void Start()
    {
        for(int i = 0; i < spawnCount; i++)
        {
            SpawnInteractable();
        }

        if (interactableType == InteractableType.Throwable)
        {
            GameManager.Instance.GetPlayerManager().OnThrow += OnInteractEnd;
        }
    }

    private void SpawnInteractable()
    {
        GameObject spawnedInteractable = Instantiate(prefab, transform);
        spawnedInteractable.transform.localPosition = Vector3.zero;

        switch(interactableType)
        {
            case InteractableType.Interactable:
                {
                    Interactable interactable = Utils.GetRequiredComponent<Interactable>(spawnedInteractable);
                    interactable.OnInteractEnd += OnInteractEnd;
                }
                break;
            case InteractableType.Throwable:
                break;
            default:
                Debug.LogError($"Unknown InteractableType: {interactableType}");
                break;
        }
    }

    private void OnInteractEnd(Interactable source)
    {
        SpawnInteractable();
    }
}
