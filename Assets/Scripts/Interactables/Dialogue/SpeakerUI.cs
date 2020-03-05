using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class SpeakerUI : MonoBehaviour
{
    public GameObject textBoxContainer;
    public GameObject canvas;
    private UITextOverlay textOverlay;
    
    private Vector3 textPosition;

    void Start()
    {
        textOverlay = GetComponent<UITextOverlay>();
        Hide();
    }

    public void Update()
    {
        textPosition = Camera.main.WorldToScreenPoint(transform.position);
        textBoxContainer.transform.position = textPosition;
    }

    public Coroutine setDialogue(string textToSet)
    {
        return textOverlay.SetText(textToSet);
    }

    public void Show()
    {
        canvas.SetActive(true);
    }

    public void Hide()
    {
        canvas.SetActive(false);
    }
}
