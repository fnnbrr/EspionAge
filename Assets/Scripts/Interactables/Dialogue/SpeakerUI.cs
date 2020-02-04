using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class SpeakerUI : MonoBehaviour
{
    public TMP_Text dialogue;
    public GameObject textBoxContainer;
    
    private Vector3 textPosition;

    void Start()
    {
        this.Hide();
    }

    public string Dialogue
    {
        set { dialogue.text = value;}
    }

    public void Update()
    {

        textPosition = Camera.main.WorldToScreenPoint(this.transform.position);
        textBoxContainer.transform.position = textPosition;

    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
