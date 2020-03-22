using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UITextOverlay : MonoBehaviour
{
    public float charTypeSpeed = 0.05f;

    public TextMeshProUGUI textMesh;
    public GameObject optionalPrompt;

    private bool isTyping = false;
    private bool skipRequest = false;
    private bool waitingForNext = false;

    public delegate void FinishTypingEvent(string typedText);
    public event FinishTypingEvent OnFinishTyping;

    private bool usedForTextCutscenes = false;

    private void Update()
    {
        if (optionalPrompt)
        {
            optionalPrompt.SetActive(isTyping);
        }

        HandleInput();
    }

    private void HandleInput()
    {
        if (UIManager.Instance.IsGamePaused()) return;

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

    public void RegisterForTextCutscenes()
    {
        usedForTextCutscenes = true;
    }

    public void ClearText()
    {
        StopAllCoroutines();
        textMesh.text = string.Empty;
    }

    public Coroutine SetText(string text)
    {
        if (isTyping)
        {
            StopAllCoroutines();
        }
        return StartCoroutine(StartTypeText(text));
    }

    private IEnumerator StartTypeText(string text)
    {
        if (usedForTextCutscenes && GameManager.Instance.skipSettings.allTextCutscenes) yield break;

        isTyping = true;

        textMesh.maxVisibleCharacters = 0;
        textMesh.text = text;

        int currentDisplayingCharacters = 0;
        while (currentDisplayingCharacters < text.Length)
        {
            if (skipRequest)
            {
                skipRequest = false;
                currentDisplayingCharacters = text.Length;
            }
            else
            {
                currentDisplayingCharacters += 1;
            }
            textMesh.maxVisibleCharacters = currentDisplayingCharacters;
            yield return new WaitForSeconds(charTypeSpeed);
        }

        waitingForNext = true;
        while (waitingForNext)
        {
            yield return new WaitForFixedUpdate();
        }

        textMesh.text = string.Empty;

        skipRequest = false;
        isTyping = false;

        OnFinishTyping?.Invoke(text);
    }
}
