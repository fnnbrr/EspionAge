using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueAutoplay : MonoBehaviour
{
    public Conversation conversation;

    private SpeakerUI speakerUIBirdie;
    private SpeakerUI speakerUINPC;

    private bool autoPlaying = false;

    public float waitLineTime = 2.0f;
    private int activeLineIndex = 0;


    public void TriggerInteraction(GameObject target)
    {
        // Possible issue here when target is not Birdie
        speakerUIBirdie = Utils.GetRequiredComponentInChildren<SpeakerUI>(target);
        speakerUINPC = Utils.GetRequiredComponentInChildren<SpeakerUI>(this);

        TriggerAutoplay();
    }


    protected void TriggerAutoplay()
    {
        if (!autoPlaying)
        {
            autoPlaying = true;
            StartCoroutine(DisplayConversations()); ;
        }
    }

    protected virtual void OnDialogueComplete()
    {
        autoPlaying = false;
    }


    IEnumerator DisplayConversations()
    {
        while (activeLineIndex < conversation.lines.Length)
        {
            //If there is still conversation left
            Debug.Log("Displaying Next Line");
            DisplayLine();
            activeLineIndex += 1;
            yield return new WaitForSeconds(waitLineTime);
        }

        speakerUIBirdie.Hide();
        speakerUINPC.Hide();
        activeLineIndex = 0;

        OnDialogueComplete();
    }


    void DisplayLine()
    {
        Line line = conversation.lines[activeLineIndex];
        bool isBirdie = line.isBirdie;

        if (isBirdie)
        {
            SetDialogue(speakerUIBirdie, speakerUINPC, line.text);
        }
        else
        {
            SetDialogue(speakerUINPC, speakerUIBirdie, line.text);
            PlayVoice(conversation.npcVoice);
        }
    }

    void SetDialogue(SpeakerUI activeSpeakerUI, SpeakerUI inactiveSpeakerUI, string text)
    {
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
