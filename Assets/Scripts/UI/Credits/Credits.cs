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
        if (toggle)
        {
            UIManager.Instance.FadeOut();
        }
        else
        {
            UIManager.Instance.FadeIn();
        }

        StopAllCoroutines();
        //text.maxVisibleCharacters = creditsText.text.Length;
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

    [Button("Force Show")]
    private void ForceShow()
    {
        Show(true);
    }

    [Button("Force Hide")]
    private void ForceHide()
    {
        Show(false);
    }
}
