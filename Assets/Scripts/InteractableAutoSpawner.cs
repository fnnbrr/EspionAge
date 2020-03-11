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

    private List<GameObject> currentInteractables;

    void Start()
    {
        Initialize();

        if (interactableType == InteractableType.Throwable)
        {
            GameManager.Instance.GetThrowController().OnThrow += OnInteractEnd;
            GameManager.Instance.GetThrowController().OnThrowableReset += Initialize;
        }
    }

    private void Initialize()
    {
        if (currentInteractables != null)
        {
            currentInteractables.ForEach(Destroy);
        }

        currentInteractables = new List<GameObject>();
        for (int i = 0; i < spawnCount; i++)
        {
            currentInteractables.Add(SpawnInteractable());
        }
    }

    private GameObject SpawnInteractable()
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

        return spawnedInteractable;
    }

    private void OnInteractEnd(Interactable source)
    {
        if (currentInteractables.Remove(source.gameObject))
        {
            currentInteractables.Add(SpawnInteractable());
        }
    }
}
