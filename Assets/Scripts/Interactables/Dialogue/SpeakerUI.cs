using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class SpeakerUI : MonoBehaviour
{
    public Text npcName;
    public TMP_Text dialogue;
    public GameObject textBoxContainer;
    
    
    private Vector3 textPosition;
    private Speaker speaker;

    public Speaker Speaker
    {
        get {return speaker;}
        set {
            speaker = value;
        }
    }

    public void Start()
    {
        this.Hide();
    }
    public string Dialogue
    {
        set { dialogue.text = value;}
    }
    
    public bool HasSpeaker()
    {
        return speaker != null;   
    }

    public bool SpeakerIs(Speaker character)
    {
        return speaker == character;
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
