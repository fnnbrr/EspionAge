using System.Collections;
using System.Collections.Generic;
using NPCs;
using UnityEngine;


public enum PlayerDetectionStatus
{
    Visible,
    ChasedNotVisible,
    Hidden
}

public class NPCReactiveBark : MonoBehaviour
{
    private BarkEvent idleBark;
    private BarkEvent spottedBark;
    private BarkEvent lostBark;
    private BarkEvent reactiveNoiseBark;
    public MissionsEnum missionsEnum;

    private float timeLostVision;
    private float timeLastHiddenBark;

    private Conversation currentBark;
    private float currentTime;
    public float randomNum; // Used to have randomized time for next idle bark
    private int numTimesSpotted;

    private BasicNurse basicNurseStates;
    private PlayerDetectionStatus playerStatus;

    private Coroutine coroutine;

    private void Awake()
    {
        basicNurseStates = Utils.GetRequiredComponent<BasicNurse>(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadBarks(missionsEnum);

        basicNurseStates.chaser.OnSeePlayer += TargetSpottedBark;
        basicNurseStates.chaser.OnLosePlayer += TargetLostBark;
        basicNurseStates.responder.OnStartResponding += ReactievBark;

        currentTime = 0;
        randomNum = Random.Range(-10f, 10.0f);
        SetPlayerStatusHidden();
    }

    private void Update()
    {
        currentTime += Time.deltaTime;

        // NPC barks when idle every 5-9s
        if (playerStatus == PlayerDetectionStatus.Hidden)
        {
            if (currentTime - timeLastHiddenBark >= (15f + randomNum))
            {
                timeLastHiddenBark = currentTime;
                randomNum = Random.Range(-5f, 15.0f);
                StartBark(idleBark);
            }
        }
    }

    private void TargetSpottedBark()
    {
        if (playerStatus != PlayerDetectionStatus.Visible)
        {
            if (playerStatus == PlayerDetectionStatus.Hidden)
            {
                StartBark(spottedBark);
                numTimesSpotted = 0;
            }
            else
            {
                if(numTimesSpotted >= 3)
                {
                    StartBark(spottedBark);
                    numTimesSpotted = 0;
                }
                else
                {
                    numTimesSpotted++;
                }
            }
        }
        playerStatus = PlayerDetectionStatus.Visible;
    }

    private void TargetLostBark()
    {
        timeLostVision = currentTime;
        playerStatus = PlayerDetectionStatus.ChasedNotVisible;

        if(coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        coroutine = StartCoroutine(TryLostBark(lostBark));
    }

    private void ReactievBark()
    {
        StartBark(reactiveNoiseBark);
    }

    private void StartBark(BarkEvent barkEvent)
    {
        // Dont start new bark if a current bark is in place
        if (currentBark != null) return;

        // Get a random bark
        List<Bark> barks = BarkManager.Instance.GetBarkLines(barkEvent);
        Bark bark = barks[Random.Range(0, barks.Count)];

        string speakerId = DialogueManager.Instance.GetSpeakerId(gameObject);

        currentBark = DialogueManager.Instance.StartBark(speakerId, bark);

        DialogueManager.Instance.OnFinishConversation += FinishBark;
    }

    private void FinishBark(Conversation conversation)
    {
        if (conversation == currentBark)
        {
            DialogueManager.Instance.OnFinishConversation -= FinishBark;
            currentBark = null;
        }
    }

    private IEnumerator TryLostBark(BarkEvent barkEvent)
    {
        yield return new WaitForSeconds(3.0f);

        if (currentTime - timeLostVision >= 3.0f && playerStatus != PlayerDetectionStatus.Visible)
        {
            StartBark(barkEvent);
            SetPlayerStatusHidden();
        }
    }

    private void SetPlayerStatusHidden()
    {
        playerStatus = PlayerDetectionStatus.Hidden;
        timeLastHiddenBark = currentTime;
    }


    private void LoadBarks(MissionsEnum missionEnum)
    {
        switch (missionEnum)
        {
            case MissionsEnum.MissionTutorial:
                break;
            case MissionsEnum.KitchenMission:
                idleBark = BarkEvent.KitchenIdleBark;
                spottedBark = BarkEvent.KitchenSpottedNurseReaction;
                lostBark = BarkEvent.KitchenLostNurseReaction;
                reactiveNoiseBark = BarkEvent.KitchenPlateDroppedNurseReaction;
                break;
        }
    }
}
