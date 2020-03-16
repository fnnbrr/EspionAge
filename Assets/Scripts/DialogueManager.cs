using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class SpeakerContainer
{
    public string id;
    public GameObject speakerObject;
    public string npcVoicePath;

    public SpeakerContainer(string _id, GameObject _speakerObject, string _npcVoicePath)
    {
        id = _id;
        speakerObject = _speakerObject;
        npcVoicePath = _npcVoicePath;
    }
}

public class DialogueManager : Singleton<DialogueManager>
{
    public float charTypeSpeed = 0.05f;
    public float waitLineTime = Constants.WAIT_TIME_CONVO_LINE;
    private int activeLineIndex = 0;
    private int startFrame;

    private bool isConversing;
    private bool autoPlaying = false;
    private bool isTyping = false;
    private bool skipRequest = false;
    private bool waitingForNext = false;

    public TextMeshProUGUI textMesh;
    private Conversation currentConvo;
    private SpeakerUI speakerUI;
    private SpeakerContainer currentSpeaker;

    private Coroutine coroutine;
    private Coroutine currentTypingCoroutine;

    public List<SpeakerContainer> allSpeakers;
    private Dictionary<string, SpeakerContainer> speakers;

    public delegate void FinishTypingEvent(string typedText);
    public event FinishTypingEvent OnFinishTyping;


    // Start is called before the first frame update
    void Start()
    {
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

        if (isTyping && Input.GetButtonDown(Constants.INPUT_INTERACTABLE_GETDOWN) && Time.frameCount != startFrame)
        {
            if (!skipRequest)
            {
                skipRequest = true;
            }
            if (waitingForNext)
            {
                waitingForNext = false;
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


    public void StartConversation(Conversation convo)
    {
        // Stop any autoplay conversation
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        currentConvo = convo;
        startFrame = Time.frameCount;

        if (convo.autoplayConversation)
        {
            TriggerAutoplay();
        }
 
        if (!autoPlaying)
        {
            StartConversing();
            GameManager.Instance.GetPlayerController().EnablePlayerInput = false;
            AdvanceConversation();
        }
        else
        {
            coroutine = StartCoroutine(AutoplayConversation());
        }
    }

    void SetDialogue(string text)
    {
        // Hide previous speaker if there was one
        if (speakerUI != null)
        {
            speakerUI.Hide();
        }

        speakerUI = Utils.GetRequiredComponent<SpeakerUI>(currentSpeaker.speakerObject);

        speakerUI.Show();
        textMesh = speakerUI.conversationText;
        PlayVoice(currentSpeaker.npcVoicePath);
        currentTypingCoroutine = SetText(text);
    }

    void DisplayLine()
    {
        Line line = currentConvo.lines[activeLineIndex];
        currentSpeaker = speakers[line.id];

        SetDialogue(line.text);
    }

    private bool ContinueConversation()
    {
        bool shouldShowLine = activeLineIndex < currentConvo.lines.Length;

        if (shouldShowLine)
        {
            DisplayLine();
            activeLineIndex += 1;
        }

        return shouldShowLine;
    }

    void AdvanceConversation()
    {
        if (!ContinueConversation())
        {
            //Once the conversation is over 
            //what happens 
            FinishConversation();

            // Unfreeze player when done
            GameManager.Instance.GetPlayerController().EnablePlayerInput = true;
        }
    }

    protected virtual void FinishConversation()
    {
        speakerUI.Hide();
        activeLineIndex = 0;
        isTyping = false;
        EndConversing();
    }

    IEnumerator AutoplayConversation()
    {
        while (ContinueConversation())
        {
            yield return new WaitForSeconds(waitLineTime);
        }

        FinishConversation();
        OnAutoplayComplete();
    }

    private void OnAutoplayComplete()
    {
        autoPlaying = false;
    }

    public Coroutine SetText(string text)
    {
        if (isTyping && currentTypingCoroutine != null)
        {
            StopCoroutine(currentTypingCoroutine);
        }
        currentTypingCoroutine = StartCoroutine(StartTypeText(text));

        return currentTypingCoroutine;
    }

    private IEnumerator StartTypeText(string text)
    {

        isTyping = true;

        int currentCharIndex = 0;
        while (currentCharIndex < text.Length)
        {
            if (skipRequest)
            {
                skipRequest = false;
                currentCharIndex = text.Length;
            }
            else
            {
                currentCharIndex += 1;
            }
            textMesh.text = text.Substring(0, currentCharIndex);
            yield return new WaitForSeconds(charTypeSpeed);
        }

        waitingForNext = true;
        while (waitingForNext)
        {
            yield return new WaitForFixedUpdate();
        }

        textMesh.text = string.Empty;

        skipRequest = false;
        isTyping = false;

        AdvanceConversation();
    }

    private void PlayVoice(string fmodPath)
    {
        if (!string.IsNullOrEmpty(fmodPath.Trim()))
        {
            FMODUnity.RuntimeManager.PlayOneShot(fmodPath, currentSpeaker.speakerObject.transform.position);
        }
    }

    private void TriggerAutoplay()
    {
        autoPlaying = true;
    }

    public bool CheckIsConversing()
    {
        return isConversing;
    }

    public void StartConversing()
    {
        isConversing = true;
    }

    public void EndConversing()
    {
        isConversing = false;
    }

    public bool CheckIsTyping()
    {
        return isTyping;
    }

    public void StartTyping()
    {
        isTyping = true;
    }

    public void EndTyping()
    {
        isTyping = false;
    }

    public bool CheckIsAutoPlaying()
    {
        return autoPlaying;
    }
}
