using System.Collections;
using System.Collections.Generic;
using NPCs;
using UnityEngine;
using NaughtyAttributes;


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

    public float waitTimeBeforeLost = 3f;
    [MinMaxSlider(0, 100f)]
    public Vector2 randomBarkTimeRange;

    private Conversation currentBark;
    private float randomBarkTime; // Used to have randomized time for next idle bark
    private int numTimesSpotted;

    private BasicNurse basicNurseStates;
    private PlayerDetectionStatus playerStatus;


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
        basicNurseStates.responder.OnStartResponding += ReactiveBark;

        randomBarkTime = Random.Range(randomBarkTimeRange.x, randomBarkTimeRange.y);
        SetPlayerStatusHidden();
    }

    private void Update()
    {
        // NPC barks at random times when idle 
        if (playerStatus == PlayerDetectionStatus.Hidden)
        {
            if (Time.time - timeLastHiddenBark >= randomBarkTime)
            {
                timeLastHiddenBark = Time.time;
                randomBarkTime = Random.Range(randomBarkTimeRange.x, randomBarkTimeRange.y);
                StartBark(idleBark);
            }
        }
    }

    private void TargetSpottedBark()
    {
        if (playerStatus == PlayerDetectionStatus.Hidden)
        {
            StartBark(spottedBark);
            numTimesSpotted = 0;
        }
        else if (playerStatus == PlayerDetectionStatus.ChasedNotVisible)
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
        playerStatus = PlayerDetectionStatus.Visible;
    }

    private void TargetLostBark()
    {
        timeLostVision = Time.time;
        playerStatus = PlayerDetectionStatus.ChasedNotVisible;

        StopAllCoroutines();

        StartCoroutine(TryLostBark(lostBark));
    }

    private void ReactiveBark()
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
        yield return new WaitForSeconds(waitTimeBeforeLost);

        if (Time.time - timeLostVision >= waitTimeBeforeLost && playerStatus != PlayerDetectionStatus.Visible)
        {
            StartBark(barkEvent);
            SetPlayerStatusHidden();
        }
    }

    private void SetPlayerStatusHidden()
    {
        playerStatus = PlayerDetectionStatus.Hidden;
        timeLastHiddenBark = Time.time;
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
