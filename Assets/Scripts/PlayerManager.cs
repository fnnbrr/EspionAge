using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Stamina")]
    public StaminaBar staminaBar;
    public float staminaChange = 0.001f;

    [SerializeField]
    private bool _canRest;

    public bool CanRest
    {
        get { return _canRest; }
        set {
            GameManager.Instance.EnableCanRestUI(value);
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
            GameManager.Instance.UpdateRestingText(isResting);
        }
        else
        {
            GameManager.Instance.UpdateRestingText(false);
        }
    }

    void HandleIncreaseStamina()
    {
        StartCoroutine(staminaBar.IncreaseStaminaBy(staminaChange));
    }
    public void HandleDecreaseStamina()
    {
        StartCoroutine(staminaBar.DecreaseStaminaBy(staminaChange));
    }
}
