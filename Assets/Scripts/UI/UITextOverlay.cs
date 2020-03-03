using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UITextOverlay : MonoBehaviour
{
    public float charTypeSpeed = 0.1f;

    private TextMeshProUGUI textMesh;

    private bool isTyping = false;
    private bool skipRequest = false;
    private bool waitingForNext = false;
    private Coroutine currentTypingCoroutine;

    public delegate void FinishTypingEvent(string typedText);
    public event FinishTypingEvent OnFinishTyping;

    private void Awake()
    {
        textMesh = Utils.GetRequiredComponent<TextMeshProUGUI>(this);
    }

    private void Update()
    {
        UIManager.Instance.pressAPrompt.SetActive(isTyping);

        if (isTyping && Input.GetButtonDown(Constants.INPUT_INTERACTABLE_GETDOWN))
        {
            if (!skipRequest)
            {
                skipRequest = true;
            }
            if (waitingForNext)
            {
                waitingForNext = false;
            }
        }
    }

    public Coroutine SetText(string text)
    {
        if (isTyping && currentTypingCoroutine != null)
        {
            StopCoroutine(currentTypingCoroutine);
        }
        currentTypingCoroutine = StartCoroutine(StartTypeText(text));

        return currentTypingCoroutine;
    }

    private IEnumerator StartTypeText(string text)
    {
        isTyping = true;

        int currentCharIndex = 0;
        while (currentCharIndex < text.Length)
        {
            if (skipRequest)
            {
                skipRequest = false;
                currentCharIndex = text.Length;
            }
            else
            {
                currentCharIndex += 1;
            }
            textMesh.text = text.Substring(0, currentCharIndex);
            yield return new WaitForSeconds(charTypeSpeed);
        }

        waitingForNext = true;
        while (waitingForNext)
        {
            yield return new WaitForFixedUpdate();
        }

        textMesh.text = string.Empty;

        isTyping = false;
        skipRequest = false;

        OnFinishTyping?.Invoke(text);
    }
}
