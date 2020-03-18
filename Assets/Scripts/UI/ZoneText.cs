using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ZoneText : MonoBehaviour
{
    public float charTypeSpeedMin = 0.1f;
    public float charTypeSpeedMax = 0.2f;
    public float stayTime = 3f;

    [Header("Colors")]
    public string defaultColor = "white";
    public string restrictedZoneColor = "red";

    private TextMeshProUGUI textMesh;
    private string currentTextValue = string.Empty;  // without rich text formatting
    private bool currentIsRestricted = false;
    private bool isTyping = false;

    private void Awake()
    {
        textMesh = Utils.GetRequiredComponentInChildren<TextMeshProUGUI>(this);
    }

    public void ClearText()
    {
        StopAllCoroutines();
        SetEmptyText(false);
    }

    private void SetEmptyText(bool isRestricted)
    {
        SetText(string.Empty, isRestricted);
    }

    private void SetText(string text, bool isRestricted)
    {
        textMesh.text = $"<color=\"{(isRestricted ? restrictedZoneColor : defaultColor)}\">{text}";
        currentTextValue = text;
        currentIsRestricted = isRestricted;
    }

    public void DisplayText(string text, bool isRestricted = false)
    {
        if (!isTyping)
        {
            StartCoroutine(Display(text, isRestricted));
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(DeleteThenDisplay(text, isRestricted));
        }
    }

    private IEnumerator Display(string text, bool isRestricted)
    {
        isTyping = true;
        yield return TypeText(text, isRestricted);
        yield return new WaitForSeconds(stayTime);
        yield return UntypeText(text, isRestricted);
        isTyping = false;
    }

    private IEnumerator DeleteThenDisplay(string text, bool isRestricted)
    {
        yield return UntypeText(currentTextValue, currentIsRestricted);
        isTyping = false;
        DisplayText(text, isRestricted);
    }

    private IEnumerator TypeText(string text, bool isRestricted)
    {
        SetEmptyText(isRestricted);

        int currentCharIndex = 0;
        while (currentCharIndex < text.Length)
        {
            currentCharIndex += 1;
            SetText(text.Substring(0, currentCharIndex), isRestricted);
            yield return new WaitForSeconds(GetCharTypeSpeed());
        }
    }

    private IEnumerator UntypeText(string text, bool isRestricted)
    {
        SetText(text, isRestricted);

        int currentCharIndex = text.Length - 1;
        while (currentCharIndex > 0)
        {
            currentCharIndex -= 1;
            SetText(text.Substring(0, currentCharIndex), isRestricted);
            yield return new WaitForSeconds(GetCharTypeSpeed());
        }
    }

    private float GetCharTypeSpeed()
    {
        return Random.Range(charTypeSpeedMin, charTypeSpeedMax);
    }
}
