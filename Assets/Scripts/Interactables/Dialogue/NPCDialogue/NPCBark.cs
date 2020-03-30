using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;

public enum NPCBarkTriggerType
{
    TutorialChaseOver
}

public class NPCBark : MonoBehaviour
{
    [Header("Settings")]
    [EnumFlags] public BarkBehaviour behaviour;
    public BarkSelectionBehaviour selectionBehaviour;

    public float enabledRadius = 30f;  // barks will only have a chance to be played when within this range

    [ShowIf(EConditionOperator.Or, "IsOnRangeEnter", "IsOnRangeExit")]
    public float triggerRadius = 5f;

    [ShowIf("IsTimed")]
    [MinMaxSlider(0f, 600f)]
    public Vector2 secondsBetweenRange;

    [OnValueChanged("OnWeightedChanged")] [ShowIf("IsSelectionRandom")]
    public bool weighted = false;

    [OnValueChanged("OnBarkContainersChanged")] [ReorderableList]
    public List<NPCBarkContainer> barkContainers;

    [ReorderableList]
    public List<NPCTriggerableBarkContainer> triggerableBarks;
    private Dictionary<NPCBarkTriggerType, Bark> triggerableBarksMap;

    private const float WEIGHT_DEFAULT = 1f;

    private bool inRange = false;
    private bool isWaitingForNextBark = false;
    private float nextBarkTime;
    private int currentSequentialBarkIndex;
    private bool currentlyBarking = false;
    private Conversation currentBark;

    [System.Serializable]
    public class NPCBarkContainer
    {
        public Bark bark;
        [ShowIf("ShowIfWeighted")]
        [AllowNesting]
        public float weight;

        [HideInInspector] public bool weighted;
        public bool ShowIfWeighted => weighted;
    }

    public enum BarkBehaviour
    {
        None = 0,
        OnRangeEnter = 1 << 0,
        OnRangeExit = 1 << 1,
        Timed = 1 << 2
    }
    public bool IsOnRangeEnter => behaviour.HasFlag(BarkBehaviour.OnRangeEnter);
    public bool IsOnRangeExit => behaviour.HasFlag(BarkBehaviour.OnRangeExit);
    public bool IsTimed => behaviour.HasFlag(BarkBehaviour.Timed);

    public enum BarkSelectionBehaviour
    {
        Sequential,
        Random
    }
    public bool IsSelectionSequential => selectionBehaviour == BarkSelectionBehaviour.Sequential;
    public bool IsSelectionRandom => selectionBehaviour == BarkSelectionBehaviour.Random;

    [System.Serializable]
    public class NPCTriggerableBarkContainer
    {
        public NPCBarkTriggerType triggerType;
        public Bark bark;
    }

    private void Awake()
    {
        triggerableBarksMap = new Dictionary<NPCBarkTriggerType, Bark>();
        triggerableBarks.ForEach(t =>
        {
            triggerableBarksMap.Add(t.triggerType, t.bark);
        });
    }

    private void Bark()
    {
        if (currentlyBarking || barkContainers.Count == 0) return;

        Bark bark = null;
        switch (selectionBehaviour)
        {
            case BarkSelectionBehaviour.Sequential:
                bark = barkContainers[currentSequentialBarkIndex++ % barkContainers.Count].bark;
                break;
            case BarkSelectionBehaviour.Random:
                bark = barkContainers[GetRandomWeightedIndex(barkContainers.Select(c => c.weight).ToArray())].bark;
                break;
            default:
                Utils.LogErrorAndStopPlayMode($"Unexpected bark selection behaviour: {selectionBehaviour}!");
                break;
        }

        StartBark(bark);
    }

    private void StartBark(Bark bark)
    {
        string speakerId = DialogueManager.Instance.GetSpeakerId(gameObject);
        currentBark = DialogueManager.Instance.StartBark(speakerId, bark);

        DialogueManager.Instance.OnFinishConversation += HandleBarkFinish;
        currentlyBarking = true;
    }

    private void HandleBarkFinish(Conversation conversation)
    {
        if (conversation == currentBark)
        {
            DialogueManager.Instance.OnFinishConversation -= HandleBarkFinish;
            currentlyBarking = false;
        }
    }

    // https://forum.unity.com/threads/random-numbers-with-a-weighted-chance.442190/
    public int GetRandomWeightedIndex(float[] weights)
    {
        // Get the total sum of all the weights.
        float weightSum = 0f;
        for (int i = 0; i < weights.Length; ++i)
        {
            weightSum += weights[i];
        }

        // Step through all the possibilities, one by one, checking to see if each one is selected.
        int index = 0;
        int lastIndex = weights.Length - 1;
        while (index < lastIndex)
        {
            // Do a probability check with a likelihood of weights[index] / weightSum.
            if (Random.Range(0, weightSum) < weights[index])
            {
                return index;
            }

            // Remove the last item from the sum of total untested weights and try again.
            weightSum -= weights[index++];
        }

        // No other item was selected, so return very last index.
        return index;
    }

    public void TriggerBark(NPCBarkTriggerType type)
    {
        if (triggerableBarksMap.ContainsKey(type))
        {
            StartBark(triggerableBarksMap[type]);
        }
        else
        {
            Debug.LogWarning($"NPCBarkTriggerType={type} is not set for object {name}! Cannot trigger bark.");
        }
    }

    public void StopCurrentBark()
    {
        if (currentBark != null)
        {
            DialogueManager.Instance.ResolveConversation(currentBark);
        }
        if (currentlyBarking)
        {
            DialogueManager.Instance.OnFinishConversation -= HandleBarkFinish;
        }
    }

    private void Update()
    {
        float distanceFromPlayer = Vector3.Distance(transform.position, GameManager.Instance.GetPlayerTransform().position);
        if (distanceFromPlayer <= enabledRadius)
        {
            HandleRangeBehaviour(distanceFromPlayer);
            HandleTimedBehaviour();
        }
    }

    private void HandleRangeBehaviour(float distanceFromPlayer)
    {
        if (IsOnRangeEnter || IsOnRangeExit)
        {
            if (distanceFromPlayer <= triggerRadius)
            {
                if (!inRange)
                {
                    inRange = true;
                    if (IsOnRangeEnter)
                    {
                        Bark();
                    }
                }
            }
            else
            {
                if (inRange)
                {
                    inRange = false;
                    if (IsOnRangeExit)
                    {
                        Bark();
                    }
                }
            }
        }
    }

    private void HandleTimedBehaviour()
    {
        if (IsTimed)
        {
            if (!isWaitingForNextBark)
            {
                nextBarkTime = Time.time + Random.Range(secondsBetweenRange.x, secondsBetweenRange.y);
                isWaitingForNextBark = true;
            }
            else if (isWaitingForNextBark && Time.time >= nextBarkTime)
            {
                Bark();
                isWaitingForNextBark = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }

    //  --- INSPECTOR FUNCTIONS ---
    private void OnBarkContainersChanged(List<NPCBarkContainer> oldValue, List<NPCBarkContainer> newValue)
    {
        ReevaluateWeights();
    }

    private void OnWeightedChanged(bool oldValue, bool newValue)
    {
        ReevaluateWeights();
    }

    private void ReevaluateWeights()
    {
        barkContainers.ForEach(c =>
        {
            if (!weighted)
            {
                c.weight = WEIGHT_DEFAULT;
            }
            c.weighted = weighted;
        });
    }
}
