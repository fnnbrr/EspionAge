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

    private void Start()
    {
        // We do not care about having an animator otherwise, for the PlayerManager
        animator = playerCameraBlending.active ? Utils.GetRequiredComponent<Animator>(this) : null;

        MinigameManager.Instance.OnMinigameComplete += HandleMinigameComplete;
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
        MinigameManager.Instance.LoadRandomMinigame();
    }

    void HandleMinigameComplete()
    {
        SetRestingAnimation(false);
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
        StartCoroutine(UIManager.Instance.staminaBar.IncreaseStaminaBy(staminaIncrease));
    }
    public void HandleDecreaseStamina()
    {
        StartCoroutine(UIManager.Instance.staminaBar.DecreaseStaminaBy(staminaDecrease));
    }
}
