using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Stamina")]
    public float staminaIncrease = 0.1f;
    public float staminaDecrease = 0.001f;

    [SerializeField]
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
        if (CanRest && Input.GetKey(KeyCode.E))
        {
            bool isResting = Input.GetKey(KeyCode.E);
            if (isResting)
            {
                HandleIncreaseStamina();
            }
            UIManager.Instance.UpdateRestingText(isResting);
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
