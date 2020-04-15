using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NaughtyAttributes;

public class Credits : MonoBehaviour
{
    public TextAsset creditsText;
    public float initialWaitSeconds = 3f;
    public float endWaitSeconds = 3f;
    public float charTypeSpeedMin = 0.1f;
    public float charTypeSpeedMax = 0.2f;

    private TextMeshProUGUI text;

    private void Awake()
    {
        text = Utils.GetRequiredComponentInChildren<TextMeshProUGUI>(this);
        text.text = creditsText.text;

        Show(false);
    }

    public Coroutine Show(bool toggle)
    {
        text.gameObject.SetActive(toggle);

        StopAllCoroutines();
        text.firstVisibleCharacter = 0;

        if (toggle)
        {
            return StartCoroutine(StartTyping());
        }
        return null;
    }

    private IEnumerator StartTyping()
    {
        yield return new WaitForSeconds(initialWaitSeconds);

        while (text.firstVisibleCharacter <= creditsText.text.Length)
        {
            yield return new WaitForSeconds(GetCharTypeSpeed());
            text.firstVisibleCharacter += 1;
        }

        yield return new WaitForSeconds(endWaitSeconds);
    }

    private float GetCharTypeSpeed()
    {
        return Random.Range(charTypeSpeedMin, charTypeSpeedMax);
    }


    // DEBUG BUTTONS
    [Button("Force Show")]
    private void ForceShow()
    {
        UIManager.Instance.FadeOut();
        Show(true);
    }

    [Button("Force Hide")]
    private void ForceHide()
    {
        UIManager.Instance.FadeIn();
        Show(false);
    }
}
