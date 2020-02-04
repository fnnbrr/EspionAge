using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueInteractable : Interactable
{
    public Conversation conversation;

    private GameObject NPCBubble;
    private GameObject BirdieBubble;
    private SpeakerUI speakerUILeft;
    private SpeakerUI speakerUIRight;

    private int activeLineIndex = 0;

    public override void OnInteract(GameObject birdie)
    {
        BirdieBubble = birdie.transform.GetChild(1).gameObject; 
        NPCBubble = this.transform.GetChild(1).gameObject;

        speakerUILeft = BirdieBubble.GetComponent<SpeakerUI>();
        speakerUIRight = NPCBubble.GetComponent<SpeakerUI>();

        HideInteractUI();
        AdvanceConversation();

        base.OnInteract(birdie);
    }

    void AdvanceConversation() {
        if (activeLineIndex < conversation.lines.Length)
        {
            //If there is still conversation left
            DisplayLine();
            activeLineIndex += 1;
        }
        else
        {
            //Once the conversation is over 
            //what happens 
            speakerUILeft.Hide();
            speakerUIRight.Hide();
            activeLineIndex = 0;
        }
    }

    void DisplayLine() {
        Line line = conversation.lines[activeLineIndex];
        bool isBirdie = line.isBirdie;

        if (isBirdie)
        {
            SetDialogue(speakerUILeft, speakerUIRight, line.text);
        } 
        else {
            SetDialogue(speakerUIRight, speakerUILeft, line.text);
        }
    }

    void SetDialogue(
        SpeakerUI activeSpeakerUI,
        SpeakerUI inactiveSpeakerUI,
        string text
    ) {
        //who is speaking
        activeSpeakerUI.Dialogue = text;
        activeSpeakerUI.Show();
        //who is not speaking
        inactiveSpeakerUI.Hide();
    }

    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
    
}