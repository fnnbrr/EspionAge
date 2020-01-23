using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Stamina")]
    public float staminaIncrease = 0.1f;
    public float staminaDecrease = 0.001f;
    public float dangerRadius = 100.0f;

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
        if (CanRest && Input.GetKeyDown(KeyCode.F))
        {
            // Temporary Controls for Minigame
            MinigameManager.Instance.LoadRandomMinigame();   
        }
    }

    private void FixedUpdate()
    {
        if (CanRest)
        {
            HandleIncreaseStamina(1.0f);
        }

        else
        {
            float minDistance = distToClosestEnemy();

            if (minDistance >= dangerRadius)
            {
                HandleDecreaseStamina(minDistance - dangerRadius);
            }
            else
            {
                HandleIncreaseStamina(dangerRadius - minDistance);
            }
        }
    }

    private float distToClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float minDistance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject enemy in enemies)
        {
            Vector3 diff = enemy.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < minDistance)
            {
                minDistance = curDistance;
            }
        }
        return minDistance;
    }

    void HandleIncreaseStamina(float multiplier)
    {
        StartCoroutine(UIManager.Instance.staminaBar.IncreaseStaminaBy(multiplier * staminaIncrease));
    }
    public void HandleDecreaseStamina(float multiplier)
    {
        StartCoroutine(UIManager.Instance.staminaBar.DecreaseStaminaBy(multiplier * staminaDecrease));
    }
}
