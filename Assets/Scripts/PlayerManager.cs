using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[System.Serializable]
public class PlayerCameraBlendingOptions
{
    public bool active = false;
    public CinemachineVirtualCamera camera;
}

public class PlayerManager : MonoBehaviour
{
    [Header("Stamina")]
    public float staminaIncrease = 0.1f;
    public float staminaDecrease = 0.001f;

    public PlayerCameraBlendingOptions playerCameraBlending;

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

    private bool isInMinigame = false;

    private Animator animator;
    private List<Coroutine> spawnedCoroutines;

    private void Start()
    {
        // We do not care about having an animator otherwise, for the PlayerManager
        animator = playerCameraBlending.active ? Utils.GetRequiredComponent<Animator>(this) : null;

        MinigameManager.Instance.OnMinigameComplete += HandleMinigameComplete;

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
            else
            {
                // Temporary Controls for Minigame
                if (Input.GetKeyDown(KeyCode.P))
                {
                    HandleTriggerStartMinigame();
                }
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

    void SetRestingAnimation(bool setTo)
    {
        if (playerCameraBlending.active)
        {
            animator.SetBool(Constants.ANIMATION_PLAYER_ISRESTING, setTo);
        }
    }

    IEnumerator WaitForCameraSwitch()
    {
        // https://forum.unity.com/threads/how-can-i-tell-when-ive-reached-my-active-virtual-cam-blend-has-completed.498544/
        // Because we blend between two cameras, the player cam is live only until the blending has completed
        if (playerCameraBlending.active)
        {
            while (CinemachineCore.Instance.IsLive(playerCameraBlending.camera))
            {
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
        StopAllSpawnedCoroutines();
        MinigameManager.Instance.LoadRandomMinigame();
    }

    void HandleMinigameComplete(float gainedStamina)
    {
        SetRestingAnimation(false);
        HandleIncreaseStaminaBy(gainedStamina, 0.5f);
        isInMinigame = false;
    }

    void HandleTriggerStartMinigame()
    {
        if (!isInMinigame)
        {
            isInMinigame = true;

            // The player being in a resting state will also trigger the state-driven camera to also zoom into the player
            SetRestingAnimation(true);
            StartCoroutine(WaitForCameraSwitch());
        }
    }

    void HandleIncreaseStamina()
    {
        spawnedCoroutines.Add(StartCoroutine(UIManager.Instance.staminaBar.IncreaseStaminaBy(staminaIncrease)));
    }

    void HandleIncreaseStaminaBy(float value, float speed)
    {
        spawnedCoroutines.Add(StartCoroutine(UIManager.Instance.staminaBar.IncreaseStaminaBy(value, speed)));
    }

    public void HandleDecreaseStamina()
    {
        spawnedCoroutines.Add(StartCoroutine(UIManager.Instance.staminaBar.DecreaseStaminaBy(staminaDecrease)));
    }
}
