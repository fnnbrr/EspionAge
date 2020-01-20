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
                if (Input.GetKeyDown(KeyCode.P))
                {
                    // Temporary Controls for Minigame
                    MinigameManager.Instance.LoadRandomMinigame();
                }
            }
        }
        else
        {
            UIManager.Instance.UpdateRestingText(false);
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
