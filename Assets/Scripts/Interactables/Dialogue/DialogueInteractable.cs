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


    private bool autoPlaying = false;

    public float waitLineTime = 2.0f;

    protected virtual void Start()
    {
        speakerUINPC = Utils.GetRequiredComponentInChildren<SpeakerUI>(this);
    }

    // Conversation to happen when interacted with
    public override void OnInteract()
    {
        speakerUIBirdie = Utils.GetRequiredComponentInChildren<SpeakerUI>(player);

        if (!autoPlaying)
        {
            if (!isConversing)
            {
                isConversing = true;

                // Freeze player when conversing
                player.GetComponent<PlayerController>().CanMove = false;

                HideInteractUI();
                AdvanceConversation();

                base.OnInteract();
            }
            else
            {
                AdvanceConversation();
            }
        }
        else
        {
            StartCoroutine(AutoplayConversations()); ;
        }
    }

    // Functionality for Autoplay Conversation to be triggered
    protected void TriggerAutoplay()
    {
        autoPlaying = true;
    }

    protected virtual void OnAutoplayComplete()
    {
        autoPlaying = false;
    }


    IEnumerator AutoplayConversations()
    {
        while (activeLineIndex < conversation.lines.Length)
        {
            //If there is still conversation left
            DisplayLine();
            activeLineIndex += 1;
            yield return new WaitForSeconds(waitLineTime);
        }

        speakerUIBirdie.Hide();
        speakerUINPC.Hide();
        activeLineIndex = 0;

        OnAutoplayComplete();
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