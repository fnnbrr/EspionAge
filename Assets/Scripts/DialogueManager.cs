using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class SpeakerContainer
{
    public int id;
    public GameObject speakerObject;
}

public class DialogueManager : Singleton<DialogueManager>
{
    public float charTypeSpeed = 0.05f;
    public float waitLineTime = Constants.WAIT_TIME_CONVO_LINE;
    private int activeLineIndex = 0;

    private bool isConversing;
    private bool autoPlaying = false;
    private bool isTyping = false;
    private bool skipRequest = false;
    private bool waitingForNext = false;

    public TextMeshProUGUI textMesh;
    private Conversation currentConvo;
    private SpeakerUI currentSpeaker;

    private Coroutine coroutine;
    private Coroutine currentTypingCoroutine;

    public List<SpeakerContainer> allSpeakers;
    private Dictionary<int, GameObject> speakers;

    public delegate void FinishTypingEvent(string typedText);
    public event FinishTypingEvent OnFinishTyping;


    // Start is called before the first frame update
    void Start()
    {
        speakers = new Dictionary<int, GameObject>();

        //Load all speakers into dictionary
        foreach(SpeakerContainer speaker in allSpeakers)
        {
            speakers.Add(speaker.id, speaker.speakerObject);
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

        if (isTyping && Input.GetButtonDown(Constants.INPUT_INTERACTABLE_GETDOWN))
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


    public void StartConversation(Conversation convo)
    {
        Debug.Log("Being called");
        // Stop any autoplay conversation
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        currentConvo = convo;

        if (convo.autoplayConversation)
        {
            TriggerAutoplay();
        }
 
        if (!autoPlaying)
        {
            StartConversing();
            StartTyping();
            GameManager.Instance.GetPlayerController().EnablePlayerInput = false;
            AdvanceConversation();
        }
        else
        {
            coroutine = StartCoroutine(AutoplayConversation());
        }
    }

    void SetDialogue(SpeakerUI activeSpeakerUI, string text)
    {
        if (currentSpeaker != null)
        {
            currentSpeaker.Hide();
        }
        //who is speaking
        currentSpeaker = activeSpeakerUI;

        currentSpeaker.Show();
        textMesh = currentSpeaker.textMesh;
        currentTypingCoroutine = SetText(text);
    }

    void DisplayLine()
    {
        Line line = currentConvo.lines[activeLineIndex];
        currentSpeaker = Utils.GetRequiredComponent<SpeakerUI>(speakers[line.id]);

        SetDialogue(currentSpeaker, line.text);
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

            EndConversing();
            //continueInteracting = false;

            // Unfreeze player when done
            GameManager.Instance.GetPlayerController().EnablePlayerInput = true;
        }
    }

    protected virtual void FinishConversation()
    {
        currentSpeaker.Hide();
        activeLineIndex = 0;
        isTyping = false;
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

    protected virtual void OnAutoplayComplete()
    {
        //if (currentConvo.shouldFollow)
        //{
        //    currentSpeaker.StopFollow();
        //    ReturnToOrigin();
        //}
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

        currentSpeaker.Hide();
        AdvanceConversation();
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
}
