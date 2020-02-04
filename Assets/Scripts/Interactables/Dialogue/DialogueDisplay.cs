using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueDisplay : MonoBehaviour
{
    // public Conversation conversation;

    // public GameObject BirdieBubble;
    // public GameObject NPCBubble;

    // private SpeakerUI speakerUILeft;
    // private SpeakerUI speakerUIRight;

    // private int activeLineIndex = 0;

    // void Start()
    // {
    //     speakerUILeft = BirdieBubble.GetComponent<SpeakerUI>();
    //     speakerUIRight = NPCBubble.GetComponent<SpeakerUI>();

    //     speakerUILeft.Speaker = conversation.Birdie;
    //     speakerUIRight.Speaker = conversation.NPC;

    //     speakerUILeft.Hide();
    //     speakerUIRight.Hide();
    //     this.Disable();
    // }

    // void Update()
    // {
    //     if (Input.GetKeyDown("z")){
    //         AdvanceConversation();
    //     }
    // }

    // void AdvanceConversation() {
    //     if (activeLineIndex < conversation.lines.Length)
    //     {
    //         //If there is still conversation left
    //         DisplayLine();
    //         activeLineIndex += 1;
    //     }
    //     else
    //     {
    //         //Once the conversation is over 
    //         //what happens 
    //         speakerUILeft.Hide();
    //         speakerUIRight.Hide();
    //         activeLineIndex = 0;
            
    //     }
    // }

    // void DisplayLine() {
    //     Line line = conversation.lines[activeLineIndex];
    //     Speaker npc = line.speaker;

    //     if (speakerUILeft.SpeakerIs(npc))
    //     {
    //         SetDialogue(speakerUILeft, speakerUIRight, line.text);
    //     } 
    //     else {
    //         SetDialogue(speakerUIRight, speakerUILeft, line.text);
            
    //     }
    // }

    // void SetDialogue(
    //     SpeakerUI activeSpeakerUI,
    //     SpeakerUI inactiveSpeakerUI,
    //     string text
    // ) {
    //     //who is speaking
    //     activeSpeakerUI.Dialogue = text;
    //     activeSpeakerUI.Show();
    //     //who is not speaking
    //     inactiveSpeakerUI.Hide();
    // }

    // public void Enable()
    // {
    //     gameObject.SetActive(true);
    // }

    // public void Disable()
    // {
    //     gameObject.SetActive(false);
    // }

}
