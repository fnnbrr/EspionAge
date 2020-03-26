using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCReactiveBark : MonoBehaviour
{
    private FieldOfVision npcVision;
    private Conversation currentBark;

    // Start is called before the first frame update
    void Start()
    {
        npcVision = Utils.GetRequiredComponent<FieldOfVision>(this);
        npcVision.OnTargetSpotted += TargetSpottedBark;
        npcVision.OnTargetLost += TargetLostBark;
    }

    private void TargetSpottedBark()
    {
        StartBark(BarkEvent.KitchenSpottedNurseReaction);
    }

    private void TargetLostBark()
    {
        StartBark(BarkEvent.KitchenLostNurseReaction);
    }

    private void StartBark(BarkEvent barkEvent)
    {
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
}
