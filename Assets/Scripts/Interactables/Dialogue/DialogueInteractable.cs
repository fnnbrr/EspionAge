using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueInteractable : Interactable
{
    public Conversation conversation;

    private SpeakerUI speakerUIBirdie;
    private SpeakerUI speakerUINPC;

    protected bool isConversing = false;

    private int activeLineIndex = 0;

    public override void OnInteract()  
    {
        if (!isConversing)
        {
            isConversing = true;

            // Freeze player when conversing
            player.GetComponent<PlayerController>().CanMove = false;

            speakerUIBirdie = Utils.GetRequiredComponentInChildren<SpeakerUI>(player);
            speakerUINPC = Utils.GetRequiredComponentInChildren<SpeakerUI>(this);

            HideInteractUI();
            AdvanceConversation();

            base.OnInteract();
        }
        else
        {
            AdvanceConversation();
        }
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

            isConversing = false;
            ShowInteractUI();

            // Unfreeze player when done
            player.GetComponent<PlayerController>().CanMove = true;
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
            PlayVoice(conversation.npcVoice);
        }
    }

    void SetDialogue(SpeakerUI activeSpeakerUI, SpeakerUI inactiveSpeakerUI, string text) {
        //who is speaking
        activeSpeakerUI.Dialogue = text;
        activeSpeakerUI.Show();

        //who is not speaking
        inactiveSpeakerUI.Hide();
    }

    private void PlayVoice(string fmodPath)
    {
        if (!string.IsNullOrEmpty(fmodPath.Trim()))
        {
            FMODUnity.RuntimeManager.PlayOneShot(fmodPath, speakerUINPC.transform.position);
        }
    }
}