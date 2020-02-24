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

    protected bool autoPlaying = false;

    public float waitLineTime = Constants.WAIT_TIME_CONVO_LINE;


    protected override void Start()
    {
        base.Start();
        speakerUINPC = Utils.GetRequiredComponentInChildren<SpeakerUI>(this);
    }


    // Conversation to happen when interacted with
    public override void OnInteract()
    {
        speakerUIBirdie = Utils.GetRequiredComponentInChildren<SpeakerUI>(player);

        if(!autoPlaying)
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
        }
        else
        {
            StartCoroutine(AutoplayConversation()); ;
        }
    }


    protected void TriggerAutoplay()
    {
        autoPlaying = true;
    }


    protected virtual void OnAutoplayComplete()
    {
        autoPlaying = false;
    }


    bool ContinueConversation()
    {
        DisplayLine();
        activeLineIndex += 1;
        return activeLineIndex < conversation.lines.Length;
    }


    void EndConversation()
    {
        speakerUIBirdie.Hide();
        speakerUINPC.Hide();
        activeLineIndex = 0;
    }


    IEnumerator AutoplayConversation()
    {
        while (ContinueConversation())
        {
            yield return new WaitForSeconds(waitLineTime);
        }

        EndConversation();
        OnAutoplayComplete();
    }


    void AdvanceConversation() {
        if (!ContinueConversation())
        {
            //Once the conversation is over 
            //what happens 
            EndConversation();

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