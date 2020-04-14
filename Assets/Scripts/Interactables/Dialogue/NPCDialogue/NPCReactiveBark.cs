using System.Collections;
using System.Collections.Generic;
using NPCs;
using UnityEngine;
using NaughtyAttributes;
using NPCs.Components;

public enum PlayerDetectionStatus
{
    Visible,
    ChasedNotVisible,
    Hidden
}

public enum ReactiveBarkType
{
    IdleBark = 1,
    SpottedBark = 2,
    LostBark = 3,
    NoiseBark = 4
}

public class NPCReactiveBark : MonoBehaviour
{
    private BarkEvent idleBark;
    private BarkEvent spottedBark;
    private BarkEvent lostBark;
    private BarkEvent reactiveNoiseBark;
    public MissionsEnum missionsEnum;
    public bool isBrutus = false;
    [ValidateInput("isBrutusTrue", "isBrutus must also be checked is isBrutusResponder is checked")]
    public bool isBrutusResponder = false;
    private bool canBark = true;
    private float timeLostVision;
    private float timeLastHiddenBark;

    public float waitTimeBeforeLost = 3f;
    [MinMaxSlider(0, 100f)]
    public Vector2 randomBarkTimeRange;

    private Conversation currentBark;
    private float randomBarkTime; // Used to have randomized time for next idle bark
    private int numTimesSpotted;

    private BasicNurse basicNurseStates;
    private AIBrutusOffice brutusStates;
    private BrutusResponder brutusResponderStates;
    private PlayerDetectionStatus playerStatus;

    private void Awake()
    {
        LoadBarks(missionsEnum);
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadAIState(isBrutus);

        randomBarkTime = Random.Range(randomBarkTimeRange.x, randomBarkTimeRange.y);
        SetPlayerStatusHidden();


        MissionManager.Instance.OnMissionRestart += MissionRestart;
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
        timeLastHiddenBark = Time.time; // prevent of barks happening too often
        StartBark(reactiveNoiseBark);
    }

    private void StartBark(BarkEvent barkEvent)
    {
        if (!canBark) return;

        // Barks only play when player is in all of the same zones as the character
        List<CameraZone> characterZones = RegionManager.Instance.GetCharacterCurrentZones(gameObject);
        foreach(CameraZone cz in characterZones)
        {
            if (!RegionManager.Instance.PlayerIsInZone(cz)) return;
        }
    
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

    private void MissionRestart(MissionsEnum missionsEnumValue)
    {
        if (missionsEnumValue != missionsEnum) return;

        MissionManager.Instance.OnMissionRestart -= MissionRestart;

        StopAllCoroutines();
        timeLastHiddenBark = Time.time;
    }

    public void TurnOffBark()
    {
        StopAllCoroutines();
        canBark = false;
    }

    private void LoadAIState(bool isBrutus)
    {
        if (isBrutus)
        {
            if (isBrutusResponder)
            {
                brutusResponderStates = Utils.GetRequiredComponent<BrutusResponder>(this);
                brutusResponderStates.chaser.OnSeePlayer += TargetSpottedBark;
                brutusResponderStates.chaser.OnLosePlayer += TargetLostBark;
                brutusResponderStates.responder.OnStartResponding += ReactiveBark;
            }
            else
            {
                brutusStates = Utils.GetRequiredComponent<AIBrutusOffice>(this);
                brutusStates.chaser.OnSeePlayer += TargetSpottedBark;
                brutusStates.chaser.OnLosePlayer += TargetLostBark;
                brutusStates.responder.OnStartResponding += ReactiveBark;
            }
        }
        else
        {
            basicNurseStates = Utils.GetRequiredComponent<BasicNurse>(this);
            basicNurseStates.chaser.OnSeePlayer += TargetSpottedBark;
            basicNurseStates.chaser.OnLosePlayer += TargetLostBark;
            basicNurseStates.responder.OnStartResponding += ReactiveBark;
        }
        
    }

    public void LoadNewBark(ReactiveBarkType reactiveBarkType, BarkEvent barkEvent)
    {
        switch (reactiveBarkType)
        {
            case ReactiveBarkType.IdleBark:
                idleBark = barkEvent;
                break;
            case ReactiveBarkType.SpottedBark:
                spottedBark = barkEvent;
                break;
            case ReactiveBarkType.LostBark:
                lostBark = barkEvent;
                break;
            case ReactiveBarkType.NoiseBark:
                reactiveNoiseBark = barkEvent;
                break;
        }
    }

    public void LoadBarks(MissionsEnum missionEnum)
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
            case MissionsEnum.BrutusOfficeSneak:
                idleBark = BarkEvent.BrutusOfficeIdleReaction;
                spottedBark = BarkEvent.BrutusOfficeSpottedReaction;
                lostBark = BarkEvent.BrutusOfficeLostReation;
                reactiveNoiseBark = BarkEvent.BrutusOfficeNoiseReaction;
                break;
            case MissionsEnum.HedgeMaze:
                if (isBrutus)
                {
                    idleBark = BarkEvent.BrutusHedgeMazeIdleBark;
                    spottedBark = BarkEvent.BrutusHedgeMazeSpottedBark;
                    lostBark = BarkEvent.BrutusHedgeMazeLostBark;
                    reactiveNoiseBark = BarkEvent.BrutusHedgeMazeNoiseBark;
                }
                else
                {
                    idleBark = BarkEvent.HedgeMazeIdleBark;
                    spottedBark = BarkEvent.HedgeMazeSpottedBark;
                    lostBark = BarkEvent.HedgeMazeLostBark;
                    reactiveNoiseBark = BarkEvent.HedgeMazeNoiseBark;
                }
                break;
        }
    }

    private bool isBrutusTrue(bool value)
    {
        if (value)
        {
            return isBrutus;
        }
        return true;
    }
}
