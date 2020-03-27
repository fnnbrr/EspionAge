using System.Collections;
using System.Collections.Generic;
using NPCs;
using NPCs.Components;
using UnityEngine;

public enum NPCDetectionStatus
{
    Visible,
    ChasingNotVisible,
    Hidden
}

public class NPCReactiveBark : MonoBehaviour
{
    private Conversation currentBark;
    private float timeLostVision;
    private float currentTime;
    private int numTimesSpotted;

    private BasicNurse basicNurseStates;
    private NPCDetectionStatus playerStatus;

    private Coroutine coroutine;

    private void Awake()
    {
        basicNurseStates = Utils.GetRequiredComponent<BasicNurse>(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        basicNurseStates.chaser.OnSeePlayer += TargetSpottedBark;
        basicNurseStates.chaser.OnLosePlayer += TargetLostBark;
        playerStatus = NPCDetectionStatus.Hidden;
        currentTime = 0;
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
    }

    private void TargetSpottedBark()
    {
        if (playerStatus != NPCDetectionStatus.Visible)
        {
            if (playerStatus == NPCDetectionStatus.Hidden)
            {
                StartBark(BarkEvent.KitchenSpottedNurseReaction);
                numTimesSpotted = 0;
            }
            else
            {
                if(numTimesSpotted >= 3)
                {
                    StartBark(BarkEvent.KitchenSpottedNurseReaction);
                    numTimesSpotted = 0;
                }
                else
                {
                    numTimesSpotted++;
                }
            }
        }
        playerStatus = NPCDetectionStatus.Visible;
    }

    private void TargetLostBark()
    {
        timeLostVision = currentTime;
        playerStatus = NPCDetectionStatus.ChasingNotVisible;

        if(coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        coroutine = StartCoroutine(TryLostBark(BarkEvent.KitchenLostNurseReaction));

        //StartBark(BarkEvent.KitchenLostNurseReaction);
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

        if (currentTime - timeLostVision >= 3.0f && playerStatus != NPCDetectionStatus.Visible)
        {
            playerStatus = NPCDetectionStatus.Hidden;
            StartBark(barkEvent);
        }
    }
}
