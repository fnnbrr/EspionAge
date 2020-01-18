using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public const float STAMINA_MAX = 1f;
    public float speed = 0.001f;

    private Image staminaBarImage;

    private void Awake() 
    {
        staminaBarImage = Utils.GetRequiredComponent<Image>(this);
        staminaBarImage.fillAmount = 1f;

    }

    //void Update()
    //{
        // Testing stuff
        // if (Input.GetKeyDown("space")){
        //     print("pressed space");
        //     IEnumerator decreaseCoroutine = decreaseStaminaBy(.1f);
        //     StartCoroutine(decreaseCoroutine);
        // } if (Input.GetKeyDown("up")){
        //     print("pressed up");
        //     IEnumerator increaseCoroutine = increaseStaminaBy(.5f);
        //     StartCoroutine(increaseCoroutine);
        // }
    //}

    public IEnumerator DecreaseStaminaBy(float percent) 
    {
        float percentClamped = Mathf.Clamp(percent, 0f, STAMINA_MAX);
        float decreaseBy = percentClamped * STAMINA_MAX;

        float fillAmount = Mathf.Max(0f, staminaBarImage.fillAmount - decreaseBy);

        while (fillAmount <= staminaBarImage.fillAmount)
        {
            staminaBarImage.fillAmount -= speed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    public IEnumerator IncreaseStaminaBy(float percent) 
    {
        float percentClamped = Mathf.Clamp(percent, 0f, STAMINA_MAX);
        float increaseBy = percentClamped * STAMINA_MAX;

        float fillAmount = Mathf.Min(STAMINA_MAX, staminaBarImage.fillAmount + increaseBy);

        while (fillAmount >= staminaBarImage.fillAmount)
        {
            staminaBarImage.fillAmount += speed;
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }


    
}
