using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueInteractable : Interactable
{
    public Conversation conversation;
    public string npcVoicePath;

    private PlayerManager playerManager;

    private SpeakerUI speakerUIBirdie;
    private SpeakerUI speakerUINPC;
    private UITextOverlay textOverlay;

    protected bool isConversing = false;
    public bool isTyping = false;
    private int activeLineIndex = 0;

    private Coroutine coroutine;

    protected bool autoPlaying = false;

    public float waitLineTime = Constants.WAIT_TIME_CONVO_LINE;


    protected override void Start()
    {
        base.Start();
        speakerUINPC = Utils.GetRequiredComponentInChildren<SpeakerUI>(this);
        playerManager = GameManager.Instance.GetPlayerManager();
        playerManager.OnInteractBegin += HandleAutoplayConversation;
        textOverlay = GetComponent<UITextOverlay>();
    }

    protected override void Update()
    {
        if (!autoPlaying && !isTyping)
        {
            base.Update();
        }
    }

    public override void OnInteract()
    {
        speakerUIBirdie = Utils.GetRequiredComponentInChildren<SpeakerUI>(player);
        if(!autoPlaying)
        {
            playerManager.InteractPlayer(this);
            if (!isConversing)
            {
                isConversing = true;

                textOverlay.OnFinishTyping += HandleFinishTyping;
                isTyping = true;

                // Used this to resolve bug where player freezes but cannot interact because player out of range
                continueInteracting = true;

                // Freeze player when conversing
                GameManager.Instance.GetPlayerController().EnablePlayerInput = false;

                speakerUIBirdie = Utils.GetRequiredComponentInChildren<SpeakerUI>(player);
                speakerUINPC = Utils.GetRequiredComponentInChildren<SpeakerUI>(this);

                HideInteractUI();
                AdvanceConversation();

                base.OnInteract();
            }
            //else
            //{
            //    AdvanceConversation();
            //}
        }
        else
        {
            //textOverlay.OnFinishTyping += HandleFinishTyping;
            coroutine = StartCoroutine(AutoplayConversation());
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
        bool shouldShowLine = activeLineIndex < conversation.lines.Length;

        if(shouldShowLine)
        {
            DisplayLine();
            activeLineIndex += 1;
        }

        return shouldShowLine;
    }

    protected virtual void EndConversation()
    {
        speakerUIBirdie.Hide();
        speakerUINPC.Hide();
        activeLineIndex = 0;
        isTyping = false;
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
            textOverlay.OnFinishTyping -= HandleFinishTyping;

            isConversing = false;
            continueInteracting = false;
            ShowInteractUI();

            // Unfreeze player when done
            GameManager.Instance.GetPlayerController().EnablePlayerInput = true;
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
            PlayVoice(npcVoicePath);
        }
    }

    void SetDialogue(SpeakerUI activeSpeakerUI, SpeakerUI inactiveSpeakerUI, string text) {
        //who is speaking
        //activeSpeakerUI.Dialogue = text;
        activeSpeakerUI.setDialogue(text);
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

    // Temporary function that listens to event of an interaction that ends all other autoplay conversations
    void HandleAutoplayConversation(DialogueInteractable source)
    {
        if(source.gameObject != this)
        {
            if(autoPlaying)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                    EndConversation();
                    OnAutoplayComplete();
                }
            }
        }
    }

    void HandleFinishTyping(string text)
    {
        AdvanceConversation();
        Debug.Log("finished typing");
    }
}