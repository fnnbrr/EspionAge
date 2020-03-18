using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class SpeakerContainer
{
    public string id;
    public GameObject speakerObject;
    [FMODUnity.EventRef]
    public string npcVoicePath;

    public SpeakerContainer(string _id, GameObject _speakerObject, string _npcVoicePath)
    {
        id = _id;
        speakerObject = _speakerObject;
        npcVoicePath = _npcVoicePath;
    }
}

[System.Serializable]
public class ActiveConversation
{
    public Conversation conversation;
    public Coroutine mainCoroutine;
    public Coroutine typingCoroutine;
    public int activeLineIndex;
    public bool isTyping;
    public bool isAutoPlaying;
    public bool skipRequest;
    public bool waitingForNext;

    public ActiveConversation(Conversation _conversation)
    {
        activeLineIndex = 0;
        isTyping = false;
        isAutoPlaying = false;
        skipRequest = false;
        waitingForNext = true;
}

    public void SetCoroutine(Coroutine _coroutine)
    {
        mainCoroutine = _coroutine;
    }

    public void SetTypingCoroutine(Coroutine _coroutine)
    {
        typingCoroutine = _coroutine;
    }

    public void ResetActiveLineIndex()
    {
        activeLineIndex = 0;
    }

    public void IncrementActiveLineIndex()
    {
        activeLineIndex++;
    }
}

public class DialogueManager : Singleton<DialogueManager>
{
    public float charTypeSpeed = 0.05f;
    public float waitLineTime = Constants.WAIT_TIME_CONVO_LINE;
    private int startFrame;

    private bool isAdvancing;
    private bool convoAllowInput = true;

    private Conversation advancingConversation;

    public List<SpeakerContainer> allSpeakers;
    private Dictionary<string, SpeakerContainer> speakers;

    // Keep track of current characters that have convos based on their ids (string)
    private Dictionary<Conversation, ActiveConversation> activeConversations;

    public delegate void FinishTypingEvent(string typedText);
    public event FinishTypingEvent OnFinishTyping;


    void Awake()
    {
        activeConversations = new Dictionary<Conversation, ActiveConversation>();
        speakers = new Dictionary<string, SpeakerContainer>();

        //Load all speakers into dictionary
        foreach(SpeakerContainer speaker in allSpeakers)
        {
            speakers.Add(speaker.id, speaker);
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (UIManager.Instance.IsGamePaused()) return;

        // Using Time.frameCount != startFrame because of an issue of skipping the first convo when beginning interaction
        // This happened because it the getdown of interacting and skipping happened in the same frame
        // Should be only able to advance the advancing conversation
        if (CheckIsAdvancing())
        {
            if (activeConversations[advancingConversation].isTyping && Input.GetButtonDown(Constants.INPUT_INTERACTABLE_GETDOWN) && Time.frameCount != startFrame)
            {
                activeConversations[advancingConversation].skipRequest = true;
                activeConversations[advancingConversation].waitingForNext = false;
            }
        }
    }

    public void AddSpeaker(SpeakerContainer speaker)
    {
        if (speakers.ContainsKey(speaker.id))
        {
            Debug.LogError("Trying to add a speaker that already exists");
            return;
        }

        speakers.Add(speaker.id, speaker);
    }


    public void StartConversation(Conversation conversation)
    {
        startFrame = Time.frameCount;

        StopAllConversations(conversation.GetAllSpeakers());
        activeConversations.Add(conversation, new ActiveConversation(conversation));

        if (conversation.autoplayConversation)
        {
            activeConversations[conversation].isAutoPlaying = true;
            activeConversations[conversation].SetCoroutine(StartCoroutine(AutoplayConversation(conversation)));
        }
        // Conversation must be manually forwarded
        // There should only be one conversation being forwarded
        else
        {
            // Stop currently advancing conversation
            if(advancingConversation != null)
            {
                FinishConversation(advancingConversation);
            }

            StartAdvancing();
            advancingConversation = conversation;

            AllowPlayerInput(false);

            AdvanceConversation(conversation);
        }
    }

    void StopAllConversations(List<string> convoSpeakers)
    {
        List<Conversation> conversationsToStop = new List<Conversation>();

        // Stop any conversations that are currently happening (check current advancing convo and all autoplaying convos)
        foreach (string speaker in convoSpeakers)
        {
            foreach (KeyValuePair<Conversation, ActiveConversation> convo in activeConversations)
            {
                if (convo.Key.GetAllSpeakers().Contains(speaker) && !conversationsToStop.Contains(convo.Key))
                {
                    conversationsToStop.Add(convo.Key);
                }
            }
        }

        foreach (Conversation convo in conversationsToStop)
        {
            FinishConversation(convo);
        }
    }

    void AdvanceConversation(Conversation conversation)
    {
        if (!ContinueConversation(conversation))
        {
            FinishConversation(conversation);

            // Unfreeze player when done
            AllowPlayerInput(true);
        }
    }

    IEnumerator AutoplayConversation(Conversation conversation)
    {
        while (ContinueConversation(conversation))
        {
            yield return new WaitForSeconds(waitLineTime);
        }

        FinishConversation(conversation);
    }

    private bool ContinueConversation(Conversation conversation)
    {
        bool shouldShowLine = activeConversations[conversation].activeLineIndex < conversation.lines.Length;

        if (shouldShowLine)
        {
            SetDialogueLine(conversation);
            activeConversations[conversation].IncrementActiveLineIndex();
        }

        return shouldShowLine;
    }

    void SetDialogueLine(Conversation conversation)
    {
        //Display Line
        Line line = conversation.lines[activeConversations[conversation].activeLineIndex];
        SpeakerContainer currentSpeaker = speakers[line.id];

        // Hide all speakers from that conversation
        HideAllSpeakers(conversation);

        SpeakerUI speakerUI = Utils.GetRequiredComponent<SpeakerUI>(currentSpeaker.speakerObject);
        TextMeshProUGUI textMesh = speakerUI.conversationText;
        speakerUI.Show();

        PlayVoice(currentSpeaker.npcVoicePath, currentSpeaker);

        if (activeConversations[conversation].isTyping && activeConversations[conversation].typingCoroutine != null)
        {
            StopCoroutine(activeConversations[conversation].typingCoroutine);
        }
        activeConversations[conversation].SetTypingCoroutine(StartCoroutine(StartTypeText(conversation, textMesh, line.text)));
    }

    private IEnumerator StartTypeText(Conversation conversation, TextMeshProUGUI textMesh, string text)
    {
        activeConversations[conversation].isTyping = true;

        int currentCharIndex = 0;
        while (currentCharIndex < text.Length)
        {
            if (activeConversations[conversation].skipRequest)
            {
                activeConversations[conversation].skipRequest = false;
                currentCharIndex = text.Length;
            }
            else
            {
                currentCharIndex += 1;
            }
            textMesh.text = text.Substring(0, currentCharIndex);
            yield return new WaitForSeconds(charTypeSpeed);
        }
        activeConversations[conversation].waitingForNext = true;
        while (activeConversations[conversation].waitingForNext)
        {
            yield return new WaitForFixedUpdate();
        }

        textMesh.text = string.Empty;

        activeConversations[conversation].skipRequest = false;
        activeConversations[conversation].isTyping = false;

        AdvanceConversation(conversation);
    }

    private void FinishConversation(Conversation conversation)
    {
        HideAllSpeakers(conversation);

        // Stop any coroutines that are part of the conversation
        // Null checks are done to be super safe just incase coroutines have not been set
        if (activeConversations[conversation].mainCoroutine != null)
        {
            StopCoroutine(activeConversations[conversation].mainCoroutine);
        }
        if (activeConversations[conversation].typingCoroutine != null)
        {
            StopCoroutine(activeConversations[conversation].typingCoroutine);
        }

        activeConversations.Remove(conversation);

        if (advancingConversation == conversation)
        {
            EndAdvancing();
        }
    }

    void HideAllSpeakers(Conversation conversation)
    {
        List<string> convoSpeakers = conversation.GetAllSpeakers();

        foreach(string speaker in convoSpeakers)
        {
            SpeakerUI speakerUI = Utils.GetRequiredComponent<SpeakerUI>(speakers[speaker].speakerObject);
            speakerUI.Hide();
        }
    }

    private void PlayVoice(string fmodPath, SpeakerContainer currentSpeaker)
    {
        if (!string.IsNullOrEmpty(fmodPath.Trim()))
        {
            FMODUnity.RuntimeManager.PlayOneShot(fmodPath, currentSpeaker.speakerObject.transform.position);
        }
    }

    public bool IsActiveConversation(Conversation conversation)
    {
        return conversation != null && activeConversations.ContainsKey(conversation);
    }

    private void AllowPlayerInput(bool allowInput)
    {
        GameManager.Instance.GetPlayerController().EnablePlayerInput = allowInput;
        convoAllowInput = allowInput;
    }

    public bool RestrictMoveWhenConversing()
    {
        return !convoAllowInput && CheckIsAdvancing();
    }

    public bool CheckIsAdvancing()
    {
        return isAdvancing;
    }

    public void StartAdvancing()
    {
        isAdvancing = true;
    }

    public void EndAdvancing()
    {
        isAdvancing = false;
        advancingConversation = null;
    }
}
