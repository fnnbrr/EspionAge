using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Stamina")]
    public float staminaIncrease = 0.1f;
    public float staminaDecrease = 0.001f;

    [SerializeField]  // this allows us to see the field update in the inspector (helps for debugging) 
    private bool _canRest;

    public bool CanRest
    {
        get { return _canRest; }
        set {
            UIManager.Instance.EnableCanRestUI(value);
            _canRest = value; 
        }
    }
    private List<Coroutine> spawnedCoroutines;

    private void Start()
    {
        spawnedCoroutines = new List<Coroutine>();
    }

    private void Update()
    {
        if (CanRest)
        {
            bool isResting = Input.GetKey(KeyCode.E);
            UIManager.Instance.UpdateRestingText(isResting);

            if (isResting)
            {
                HandleIncreaseStamina();
            } 
        }
        else
        {
            UIManager.Instance.UpdateRestingText(false);
        }
    }

    // Note: Might need to do more testing if this is actually doing anything considerable...
    //  but better safe than sorry to make sure coroutines we spawn are no longer running when we enter a minigame
    void StopAllSpawnedCoroutines()
    {
        // No loose coroutines in MY house!
        foreach (Coroutine c in spawnedCoroutines)
        {
            StopCoroutine(c);
        }
        spawnedCoroutines.Clear();
    }

    private void OnDisable()
    {
        StopAllSpawnedCoroutines();

    }

    void HandleIncreaseStamina()
    {
        spawnedCoroutines.Add(StartCoroutine(UIManager.Instance.staminaBar.IncreaseStaminaBy(staminaIncrease)));
    }

    public void HandleDecreaseStamina()
    {
        spawnedCoroutines.Add(StartCoroutine(UIManager.Instance.staminaBar.DecreaseStaminaBy(staminaDecrease)));
    }
}
