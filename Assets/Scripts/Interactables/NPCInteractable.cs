using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractable : Interactable
{
    // Handle the dialogue for this interactable
    // In the future I want to incorporate Dialogue Display into this class 
    public DialogueDisplay dialogue;

    public Conversation conversation;

    public GameObject BirdieBubble;
    public GameObject NPCBubble;

    private SpeakerUI speakerUILeft;
    private SpeakerUI speakerUIRight;

    private int activeLineIndex = 0;

    public override void OnInteract(GameObject birdie)
    {
        BirdieBubble = birdie.transform.GetChild(1).gameObject;

        speakerUILeft = BirdieBubble.GetComponent<SpeakerUI>();
        speakerUIRight = NPCBubble.GetComponent<SpeakerUI>();

        print("dialogue enabled");
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
        // Speaker npc = line.speaker;
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