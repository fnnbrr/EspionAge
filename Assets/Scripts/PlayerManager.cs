using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerManager : MonoBehaviour
{
    [Header("Stamina")]
    public float staminaIncrease = 0.1f;
    public float staminaDecrease = 0.001f;

    [Header("Player Camera")]
    public CinemachineVirtualCamera playerCamera;

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
        animator = Utils.GetRequiredComponent<Animator>(this);

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
                if (Input.GetKeyDown(KeyCode.P) && !isInMinigame)
                {
                    isInMinigame = true;
                    // Temporary Controls for Minigame
                    // The player being in a resting state will also trigger the state-driven camera to also zoom into the player
                    animator.SetBool(Constants.ANIMATION_PLAYER_ISRESTING, true);
                    StartCoroutine(WaitForCameraSwitch());
                }
            }
        }
        else
        {
            UIManager.Instance.UpdateRestingText(false);
        }
    }

    IEnumerator WaitForCameraSwitch()
    {
        // https://forum.unity.com/threads/how-can-i-tell-when-ive-reached-my-active-virtual-cam-blend-has-completed.498544/
        // Because we blend between two cameras, the player cam is live only until the blending has completed
        while (CinemachineCore.Instance.IsLive(playerCamera))
        {
            yield return new WaitForSeconds(Time.deltaTime);
        }
        MinigameManager.Instance.LoadRandomMinigame();
    }

    void HandleMinigameComplete()
    {
        animator.SetBool(Constants.ANIMATION_PLAYER_ISRESTING, false);
        isInMinigame = false;
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
