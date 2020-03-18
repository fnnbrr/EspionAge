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
    public Coroutine coroutine;
    public int activeLineIndex;
    public bool isTyping;
    public bool isAutoPlaying;
    public bool skipRequest;
    public bool waitingForNext;

    public ActiveConversation(Conversation _conversation)
    {
        conversation = _conversation;
        activeLineIndex = 0;
        isTyping = false;
        isAutoPlaying = false;
        skipRequest = false;
        waitingForNext = false;
}

    public void SetCoroutine(Coroutine _coroutine)
    {
        coroutine = _coroutine;
    }

    public void ResetIndex()
    {
        activeLineIndex = 0;
    }

    public void IncrementIndex()
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
    private bool convoAllowMovement = true;

    private Conversation advancingConversation;

    private Coroutine currentTypingCoroutine;

    public List<SpeakerContainer> allSpeakers;
    private Dictionary<string, SpeakerContainer> speakers;

    // Keep track of current characters that have convos based on their ids (string)
    private Dictionary<Conversation, ActiveConversation> activeConversations;

    public delegate void FinishTypingEvent(string typedText);
    public event FinishTypingEvent OnFinishTyping;


    // Start is called before the first frame update
    void Start()
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
        if (activeConversations[advancingConversation].isTyping && Input.GetButtonDown(Constants.INPUT_INTERACTABLE_GETDOWN) && Time.frameCount != startFrame)
        {
            activeConversations[advancingConversation].skipRequest = true;
            activeConversations[advancingConversation].waitingForNext = false;
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

        ActiveConversation startedConvo = new ActiveConversation(conversation);
        activeConversations.Add(conversation, startedConvo);

        // Get all speakers from the convo
        List<string> convoSpeakers = conversation.GetAllSpeakers();

        StopAllConversations(convoSpeakers);

        if (conversation.autoplayConversation)
        {
            Coroutine coroutine = StartCoroutine(AutoplayConversation(conversation));
            activeConversations[conversation].SetCoroutine(coroutine);
            activeConversations[conversation].isAutoPlaying = true;

        }
        // Conversation must be manually forwarded
        // There should only be one conversation being forwarded
        else
        {
            // Stop any currently advancing conversations
            advancingConversation = conversation;

            GameManager.Instance.GetPlayerController().EnablePlayerInput = false;
            convoAllowMovement = false;

            AdvanceConversation(conversation);
        }
    }

    void StopAllConversations(List<string> convoSpeakers)
    {
        // Stop any conversations that are currently happening (check current advancing convo and all autoplaying convos)
        foreach (string speaker in convoSpeakers)
        {
            foreach (KeyValuePair<Conversation, ActiveConversation> convo in activeConversations)
            {
                Conversation conversation = convo.Key;
                if (conversation.GetAllSpeakers().Contains(speaker))
                {
                    if (convo.Value.coroutine != null)
                    {
                        StopCoroutine(convo.Value.coroutine);
                    }
                    activeConversations.Remove(convo.Key);
                }
            }
        }
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

        if (activeConversations[conversation].isTyping && currentTypingCoroutine != null)
        {
            StopCoroutine(currentTypingCoroutine);
        }
        currentTypingCoroutine = StartCoroutine(StartTypeText(conversation, textMesh, line.text));
    }


    void AdvanceConversation(Conversation conversation)
    {
        if (!ContinueConversation(conversation))
        {
            FinishConversation(conversation);

            // Unfreeze player when done
            GameManager.Instance.GetPlayerController().EnablePlayerInput = true;
            convoAllowMovement = true;
        }
    }

    IEnumerator AutoplayConversation(Conversation conversation)
    {
        while (ContinueConversation(conversation))
        {
            yield return new WaitForSeconds(waitLineTime);
        }

        FinishConversation(conversation);
        activeConversations.Remove(conversation);
    }

    private bool ContinueConversation(Conversation conversation)
    {
        bool shouldShowLine = activeConversations[conversation].activeLineIndex < conversation.lines.Length;

        if (shouldShowLine)
        {
            SetDialogueLine(conversation);
            activeConversations[conversation].IncrementIndex();
        }

        return shouldShowLine;
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

    public bool RestrictMoveWhenConversing()
    {
        return !convoAllowMovement && CheckIsAdvancing();
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
    }
}
