using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueAutoplay : MonoBehaviour
{
    public Conversation conversation;

    public GameObject player;

    private SpeakerUI speakerUIBirdie;
    private SpeakerUI speakerUINPC;

    private bool autoPlaying = false;

    public float waitLineTime = 2.0f;
    private int activeLineIndex = 0;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        speakerUIBirdie = Utils.GetRequiredComponentInChildren<SpeakerUI>(player);
        speakerUINPC = Utils.GetRequiredComponentInChildren<SpeakerUI>(this);
    }


    // Triggering should be handled by another/ separate function
    // Follow Player should be called in a child class of this class

    // Temporarily using on trigger enter to trigger autoplay
    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer(Constants.LAYER_PLAYER))
        {
            TriggerAutoplay();
        }
    }

    // Autoplay dialogue when triggered
    public void TriggerAutoplay()
    {
        if (!autoPlaying)
        {
            autoPlaying = true;
            StartCoroutine(DisplayConversations()); ;
        }
    }

    protected virtual void InactivateDialogue()
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

        InactivateDialogue();
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
