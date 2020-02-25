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

    protected override void Update()
    {
        if (!autoPlaying)
        {
            base.Update();
        }
    }

    public override void OnInteract()
    {
        speakerUIBirdie = Utils.GetRequiredComponentInChildren<SpeakerUI>(player);
        if(!autoPlaying)
        {
            if (!isConversing)
            {
                isConversing = true;

                // Used this to resolve bug where player freezes but cannot interact because player out of range
                continueInteracting = true;

                // Freeze player when conversing
                GameManager.Instance.GetPlayerController().CanMove = false;

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

    private bool ContinueConversation()
    {
        //DisplayLine();
        //activeLineIndex += 1;
        return activeLineIndex < conversation.lines.Length;
    }

    protected virtual void EndConversation()
    {
        speakerUIBirdie.Hide();
        speakerUINPC.Hide();
        activeLineIndex = 0;
    }

    IEnumerator AutoplayConversation()
    {
        while (ContinueConversation())
        {
            //ISSUE: not displaying last line because it ends it right after it is displayed
            //Temporary solution to this issue
            DisplayLine();
            activeLineIndex += 1;
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
            continueInteracting = false;
            ShowInteractUI();

            // Unfreeze player when done
            GameManager.Instance.GetPlayerController().CanMove = true;
        }
        else
        {
            //ISSUE: not displaying last line because it ends it right after it is displayed
            //Temporary solution to this issue
            DisplayLine();
            activeLineIndex += 1;
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