using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NaughtyAttributes;

[System.Serializable]
public class SpeakerContainer
{
    public string id;
    public GameObject speakerObject;
    [HideInInspector]
    public SpeakerUI speakerUI;
    [FMODUnity.EventRef]
    public string npcVoicePath;

    public SpeakerContainer(string _id, GameObject _speakerObject, string _npcVoicePath)
    {
        id = _id;
        speakerObject = _speakerObject;
        npcVoicePath = _npcVoicePath;
    }

    public void SetSpeakerUI()
    {
        speakerUI = Utils.GetRequiredComponent<SpeakerUI>(speakerObject);
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

    public ActiveConversation()
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
    private int startFrame;

    private bool isAdvancing;
    private bool convoAllowInput = true;

    private Conversation advancingConversation;

    [ReorderableList]
    public List<SpeakerContainer> allSpeakers;
    private Dictionary<string, SpeakerContainer> speakers;

    // Keep track of current characters that have convos based on their ids (string)
    private Dictionary<Conversation, ActiveConversation> activeConversations;

    public delegate void FinishTypingEvent(string typedText);
    public event FinishTypingEvent OnFinishTyping;

    public delegate void FinishConversationEvent (Conversation conversation);
    public event FinishConversationEvent OnFinishConversation;

    private void Awake()
    {
        activeConversations = new Dictionary<Conversation, ActiveConversation>();
        speakers = new Dictionary<string, SpeakerContainer>();

        //Load all speakers into dictionary
        foreach (SpeakerContainer speaker in allSpeakers)
        {
            AddSpeaker(speaker);
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
        speaker.SetSpeakerUI();
        speakers.Add(speaker.id, speaker);
    }

    public bool HasSpeaker(string id)
    {
        return speakers.ContainsKey(id);
    }

    public void RemoveSpeaker(string speakerId)
    {
        if (!speakers.ContainsKey(speakerId))
        {
            Debug.LogError("Trying to remove a speaker that doesn't exist");
            return;
        }

        speakers.Remove(speakerId);
    }


    public void StartConversation(Conversation conversation)
    {
        startFrame = Time.frameCount;

        StopAllConversations(conversation.GetAllSpeakers());
        activeConversations.Add(conversation, new ActiveConversation());

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
                ResolveConversation(advancingConversation);
            }

            StartAdvancing();
            advancingConversation = conversation;

            AllowPlayerInput(false);

            AdvanceConversation(conversation);
        }
    }

    public Conversation StartBark(string speakerId, Bark bark)
    {
        Conversation barkConversation = ScriptableObject.CreateInstance<Conversation>();
        barkConversation.lines = new Line[] { new Line(speakerId, bark.line) };
        barkConversation.autoplayConversation = true;
        StartConversation(barkConversation);

        return barkConversation;
    }

    public string GetSpeakerId(GameObject speakerObject)
    {
        foreach (KeyValuePair<string, SpeakerContainer> pair in speakers)
        {
            if (pair.Value.speakerObject == speakerObject)
            {
                return pair.Value.id;
            }
        }

        return string.Empty;
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
            ResolveConversation(convo);
        }
    }

    public bool IsConversationActive(Conversation conversation)
    {
        return activeConversations.ContainsKey(conversation);
    }

    void AdvanceConversation(Conversation conversation)
    {
        if (!ContinueConversation(conversation))
        {
            ResolveConversation(conversation);

            // Unfreeze player when done
            AllowPlayerInput(true);
        }
    }

    IEnumerator AutoplayConversation(Conversation conversation)
    {
        while (ContinueConversation(conversation))
        {
            while(!activeConversations[conversation].waitingForNext)
            {
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(Constants.WAIT_TIME_CONVO_LINE);
        }

        ResolveConversation(conversation);
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

        TextMeshProUGUI textMesh = currentSpeaker.speakerUI.conversationText;
        currentSpeaker.speakerUI.SetActiveAButton(!activeConversations[conversation].isAutoPlaying);
        currentSpeaker.speakerUI.Show();

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
        activeConversations[conversation].waitingForNext = false;

        textMesh.maxVisibleCharacters = 0;
        textMesh.text = text;

        int currentDisplayingCharacters = 0;
        while (currentDisplayingCharacters < text.Length)
        {
            if (activeConversations[conversation].skipRequest)
            {
                activeConversations[conversation].skipRequest = false;
                currentDisplayingCharacters = text.Length;
            }
            else
            {
                currentDisplayingCharacters += 1;
            }
            textMesh.maxVisibleCharacters = currentDisplayingCharacters;
            yield return new WaitForSeconds(Constants.CHAR_TYPE_SPEED);
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

    public void ResolveConversation(Conversation conversation)
    {
        if (!activeConversations.ContainsKey(conversation)) return;

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

        OnFinishConversation?.Invoke(conversation);
    }

    void HideAllSpeakers(Conversation conversation)
    {
        List<string> convoSpeakers = conversation.GetAllSpeakers();

        foreach(string speaker in convoSpeakers)
        {
            speakers[speaker].speakerUI.Hide();
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
