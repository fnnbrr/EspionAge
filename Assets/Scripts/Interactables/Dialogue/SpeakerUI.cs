using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class SpeakerUI : MonoBehaviour
{
    public TMP_Text dialogue;
    public GameObject textBoxContainer;
    public GameObject canvas;
    
    private Vector3 textPosition;

    public string Dialogue
    {
        set { dialogue.text = value;}
    }

    void Start()
    {
        Hide();
    }

    public void Update()
    {
        textPosition = Camera.main.WorldToScreenPoint(transform.position);
        textBoxContainer.transform.position = textPosition;
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
