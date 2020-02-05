using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueInteractable : Interactable
{
    public Conversation conversation;

    private SpeakerUI speakerUIBirdie;
    private SpeakerUI speakerUINPC;

    private int activeLineIndex = 0;

    public override void OnInteract(GameObject birdie)
    {
        speakerUIBirdie = birdie.GetComponentInChildren<SpeakerUI>();
        speakerUINPC = GetComponentInChildren<SpeakerUI>();

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
            speakerUIBirdie.Hide();
            speakerUINPC.Hide();
            activeLineIndex = 0;
        }
    }

    void DisplayLine() {
        Line line = conversation.lines[activeLineIndex];
        bool isBirdie = line.isBirdie;

        if (isBirdie)
        {
            SetDialogue(speakerUIBirdie, speakerUINPC, line.text);
        } 
        else {
            SetDialogue(speakerUINPC, speakerUIBirdie, line.text);
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