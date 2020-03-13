using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RegionText : MonoBehaviour
{
    public float charTypeSpeedMin = 0.1f;
    public float charTypeSpeedMax = 0.2f;
    public float stayTime = 3f;

    private TextMeshProUGUI textMesh;
    private bool isTyping = false;

    private void Awake()
    {
        textMesh = Utils.GetRequiredComponentInChildren<TextMeshProUGUI>(this);
    }

    public void DisplayText(string text)
    {
        if (!isTyping)
        {
            StartCoroutine(Display(text));
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(DeleteThenDisplay(text));
        }
    }

    private IEnumerator Display(string text)
    {
        isTyping = true;
        yield return TypeText(text);
        yield return new WaitForSeconds(stayTime);
        yield return UntypeText(text);
        isTyping = false;
    }

    private IEnumerator DeleteThenDisplay(string text)
    {
        yield return UntypeText(textMesh.text);
        isTyping = false;
        DisplayText(text);
    }

    private IEnumerator TypeText(string text)
    {
        textMesh.text = string.Empty;

        int currentCharIndex = 0;
        while (currentCharIndex < text.Length)
        {
            currentCharIndex += 1;
            textMesh.text = text.Substring(0, currentCharIndex);
            yield return new WaitForSeconds(GetCharTypeSpeed());
        }
    }

    private IEnumerator UntypeText(string text)
    {
        textMesh.text = text;

        int currentCharIndex = text.Length - 1;
        while (currentCharIndex > 0)
        {
            currentCharIndex -= 1;
            textMesh.text = text.Substring(0, currentCharIndex);
            yield return new WaitForSeconds(GetCharTypeSpeed());
        }
    }

    private float GetCharTypeSpeed()
    {
        return Random.Range(charTypeSpeedMin, charTypeSpeedMax);
    }
}
