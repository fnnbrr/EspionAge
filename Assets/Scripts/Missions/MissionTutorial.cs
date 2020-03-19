using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;
using NaughtyAttributes;
using NPCs;

public class MissionTutorial : AMission
{
    public Vector3 playerStartPosition;
    public Vector3 playerStartRotation;
    public Vector3 playerRespawnPosition;
    public Vector3 playerRespawnRotation;

    [BoxGroup("Nurse Room Sequence")] public MissionObject tutorialNurse;
    [BoxGroup("Nurse Room Sequence")] public string tutorialNurseSpeakerId;
    [FMODUnity.EventRef]
    [BoxGroup("Nurse Room Sequence")] public string tutorialNurseVoicePath;
    [ReorderableList]
    [BoxGroup("Nurse Room Sequence")]  public List<Conversation> nurseConversations;
    [ReorderableList]
    [BoxGroup("Nurse Room Sequence")] public List<Conversation> birdieCaughtConversations;
    [BoxGroup("Nurse Room Sequence")] public Conversation cantEscapeYetConversation;
    [ReorderableList]
    [BoxGroup("Nurse Room Sequence")] public List<NPCs.Components.PatrolWaypoint> lostBirdieWaypoints;
    [ReorderableList]
    [BoxGroup("Nurse Room Sequence")] public List<Conversation> lostBirdieConversations;
    [BoxGroup("Nurse Room Sequence")] public Conversation lostBirdieLeaveRoomSelfPrompt;

    // private nurse room variables
    private bool nurseIsFollowingPlayer = false;
    private bool canEscape = false;
    private bool isInCantEscapeConversation = false;
    private TutorialNurse tutorialNurseAI;
    private FieldOfVision tutorialNurseFOV;
    private int currentCaughtConversationIndex = 0;
    private Conversation currentNurseConversation;
    private Conversation currentLostBirdieConversation;

    [Header("Cutscenes")]
    public List<string> startCutsceneTexts;
    public GameObject awakenessPointerUIAnimation;
    public GameObject vaseFocusCameraPrefab;
    public GameObject vaseDropCutsceneText;
    public GameObject enemyFocusCameraPrefab;
    public GameObject enemyCutsceneText;
    public GameObject birdieRunawayCutsceneText;
    public GameObject specialAbilityPointerUIAnimation;

    [Header("Vase - General")]
    public GameObject vasePrefab;
    public GameObject vaseStandPrefab;

    [Header("First Vase")]
    public Vector3 firstVasePosition;
    private SpawnedVase firstVase;

    [Header("Row of Vases")]
    [ReorderableList]
    public List<Vector3> vasePositions;
    private List<SpawnedVase> spawnedVases;
    private List<GameObject> spawnedBrokenVases;
    // stand position must then be, (x - 0.5, 1.5, z - 1) (empirically)

    [Header("Chaser Enemies")]
    public GameObject chaserPrefab;
    public float enemyCutsceneAnimationSpeed = 0.2f;
    public List<TutorialChaserGroup> chaserGroups;

    [Header("Misc. Objects")]
    public List<MissionObject> extraObjects;

    [Header("FMOD Audio")]
    private FMODUnity.StudioEventEmitter musicEv;

    private bool startCutscenePlayed = false;
    private bool respawning = false;
    private bool missionCompleting = false;

    private List<SpawnedEnemy> spawnedEnemies;

    [System.Serializable]
    public class TutorialChaserGroup
    {
        public float startChaseRadius;
        public float chaseSpeed;
        public List<Vector3> enemyStartPositions;
    }

    public class SpawnedVase
    {
        public GameObject vaseObject;
        public GameObject vaseStand;
        public LoudObject loudObject;
        public BreakableObject breakableObject;

        public SpawnedVase(GameObject o, GameObject s)
        {
            vaseObject = o;
            vaseStand = s;

            loudObject = Utils.GetRequiredComponentInChildren<LoudObject>(vaseObject);
            breakableObject = Utils.GetRequiredComponentInChildren<BreakableObject>(vaseObject);
        }
    }

    public class SpawnedEnemy
    {
        public GameObject gameObject;
        public PureChaser pureChaser;
        public TutorialChaserGroup chaserGroup;

        public SpawnedEnemy(GameObject o, PureChaser c, TutorialChaserGroup g)
        {
            gameObject = o;
            pureChaser = c;
            chaserGroup = g;
        }
    }
    
    private void Awake()
    {
        spawnedVases = new List<SpawnedVase>();
        spawnedBrokenVases = new List<GameObject>();
        spawnedEnemies = new List<SpawnedEnemy>();
        musicEv = GetComponent<FMODUnity.StudioEventEmitter>();
    }

    protected override void Initialize()
    {
        Debug.Log("MissionTutorial Initialize()!");

        // Force fade out
        UIManager.Instance.InstantFadeOut();

        // Init some general vars
        UIManager.Instance.CanPause = false;

        // Move player to a set position
        GameManager.Instance.GetPlayerController().EnablePlayerInput = false;
        GameManager.Instance.GetPlayerRigidbody().isKinematic = true;  // to stop collisions with the bed for the wake up animation
        GameManager.Instance.GetPlayerTransform().position = playerStartPosition;
        GameManager.Instance.GetPlayerTransform().rotation = Quaternion.Euler(playerStartRotation);

        // Toggle the event in the EventManager
        GameEventManager.Instance.SetEventStatus(GameEventManager.GameEvent.TutorialActive, true);

        // spawn the specific first vase we will drop, and the remaining ones
        firstVase = SpawnVase(firstVasePosition);
        firstVase.loudObject.dropRadius = 0f;

        // Spawn all the other vases
        SpawnRegularVases();
        SpawnExtraObjects();

        // Listen for the player to pass through the final door to finish the mission
        RegionManager.Instance.finalHallwayDoor.OnPlayerPassThrough += CommenceCompleteMission;

        StartCoroutine(StartMissionLogic());
    }

    private void HandleCouldNotFindBirdie()
    {
        tutorialNurseAI.OnCouldNotFindBirdie -= HandleCouldNotFindBirdie;

        UnlockDoorAndDisableNurseComponents();

        tutorialNurseAI.SetLostBirdieWaypoints(lostBirdieWaypoints);
        PlaySequentialConversationsSequential();
        DialogueManager.Instance.StartConversation(lostBirdieLeaveRoomSelfPrompt);
    }

    private void PlaySequentialConversationsSequential()
    {
        if (lostBirdieConversations.Count > 0)
        {
            currentLostBirdieConversation = lostBirdieConversations[0];
            lostBirdieConversations.RemoveAt(0);

            DialogueManager.Instance.StartConversation(currentLostBirdieConversation);
            DialogueManager.Instance.OnFinishConversation += OnFinishLostBirdieConversation;
        }
    }

    private void OnFinishLostBirdieConversation(Conversation conversation)
    {
        if (conversation != currentLostBirdieConversation) return;
        DialogueManager.Instance.OnFinishConversation -= OnFinishLostBirdieConversation;
        PlaySequentialConversationsSequential();
    }

    private void HandleTutorialNurseSpottingPlayer()
    {
        // If they are returning, we want them to fully return before starting a conversation again
        if (nurseIsFollowingPlayer || tutorialNurseAI.currentState == TutorialNurseStates.Returning) return;

        if (RegionManager.Instance.PlayerIsInRegion(RegionManager.Instance.nurseRoomBirdiesBedArea))
        {
            if (nurseConversations.Count == 0) return;

            currentNurseConversation = nurseConversations.First();
            DialogueManager.Instance.StartConversation(currentNurseConversation);
            tutorialNurseAI.SetFoundBirdie();

            nurseConversations.RemoveAt(0);
            if (nurseConversations.Count == 0)
            {
                UnlockDoorAndDisableNurseComponents();
                return;
            }

            DialogueManager.Instance.OnFinishConversation += StopFollowingAfterConversationEnds;
            tutorialNurseAI.StopMovement();
        }
        else if (RegionManager.Instance.PlayerIsInRegion(RegionManager.Instance.nurseRoomDoorArea) || 
            RegionManager.Instance.PlayerIsInRegion(RegionManager.Instance.nurseRoomOtherBedArea))
        {
            if (birdieCaughtConversations.Count == 0)
            {
                Debug.LogError("Need at least one element in birdieCaughtConversations!");
                return;
            }
            currentNurseConversation = birdieCaughtConversations[currentCaughtConversationIndex++ % birdieCaughtConversations.Count];
            DialogueManager.Instance.StartConversation(currentNurseConversation);
            nurseIsFollowingPlayer = true;

            tutorialNurseAI.StartFollowingPlayer();
            tutorialNurseAI.SetFoundBirdie();
            WorldObjectivePointer.Instance.PointTo(playerStartPosition, RegionManager.Instance.nurseRoomBirdiesBedArea);

            RegionManager.Instance.OnPlayerEnterRegion += WaitForBirdieToGoBackToBed;
        }
        else
        {
            Debug.LogError("Player is not in any of the expected nurse room regions when tutorial nurse spotted her!");
        }
    }

    private void StopFollowingAfterConversationEnds(Conversation conversation)
    {
        if (conversation != currentNurseConversation) return;
        DialogueManager.Instance.OnFinishConversation -= StopFollowingAfterConversationEnds;

        tutorialNurseAI.ReturnToOrigin();
        nurseIsFollowingPlayer = false;
    }

    private void WaitForBirdieToGoBackToBed(RegionTrigger region)
    {
        if (region != RegionManager.Instance.nurseRoomBirdiesBedArea) return;
        RegionManager.Instance.OnPlayerEnterRegion -= WaitForBirdieToGoBackToBed;

        tutorialNurseAI.ReturnToOrigin();
        nurseIsFollowingPlayer = false;
    }

    private void UnlockDoorAndDisableNurseComponents()
    {
        canEscape = true;

        // Disable events were we previously listening for
        tutorialNurseAI.OnCouldNotFindBirdie -= HandleCouldNotFindBirdie;
        tutorialNurseFOV.OnTargetSpotted -= HandleTutorialNurseSpottingPlayer;
        RegionManager.Instance.nurseRoomDoor.OnPlayerCollideWithDoor -= OnPlayerCollideWithNurseRoomDoor;

        // Hide the FOV now (if we disable the component it'll go wonky and freeze the fov mesh weirdly)
        tutorialNurseFOV.viewRadius = 0f;

        // Make the nurse go back to his chair + make him stay there (it's his originWaypoint)
        tutorialNurseAI.ReturnThenIdle();

        // Tell the user they found the right way out!
        ObjectiveList.Instance.CrossOutObjectiveText();

        // Unlock the door (conversations will be clear to the player that they can leave now)
        RegionManager.Instance.nurseRoomDoor.SetLocked(false);

        // Others...
        // TODO: door unlock sound
    }

    private void CommenceCompleteMission()
    {
        RegionManager.Instance.finalHallwayDoor.OnPlayerPassThrough -= CommenceCompleteMission;

        if (missionCompleting) return;
        missionCompleting = true;

        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("ChaseEnd", 1f);

        spawnedEnemies.ForEach(e =>
        {
            // Send back all enemies to around the area of their start (mostly to get them off camera)
            e.pureChaser.targetTransform = null;
            e.pureChaser.SetDestination(e.chaserGroup.enemyStartPositions[0]);
            e.pureChaser.OnReachDestination += HandleEnemyReachedStartPoint;
        });
    }
    private void HandleEnemyReachedStartPoint()
    {
        AlertMissionComplete();
        MissionManager.Instance.EndMission(MissionsEnum.MissionTutorial);
    }

    private IEnumerator StartMissionLogic()
    {
        // The starting rolling text... maybe one day we'll remove this
        foreach (string text in startCutsceneTexts)
        {
            yield return UIManager.Instance.textOverlay.SetText(text);
        }
        UIManager.Instance.zoneText.ClearText(); // instant clear it to type it out when we fade in later

        // Tutorial Nurse Section Init: add as speaker, init with ai components to respond to certain events
        tutorialNurse.spawnedInstance = MissionManager.Instance.SpawnMissionObject(tutorialNurse);
        DialogueManager.Instance.AddSpeaker(
            new SpeakerContainer(
                tutorialNurseSpeakerId,
                tutorialNurse.spawnedInstance,
                tutorialNurseVoicePath));
        tutorialNurseAI = Utils.GetRequiredComponent<TutorialNurse>(tutorialNurse.spawnedInstance);
        tutorialNurseAI.OnCouldNotFindBirdie += HandleCouldNotFindBirdie;  // make the ai freak out when they cant find birdie
        tutorialNurseFOV = Utils.GetRequiredComponent<FieldOfVision>(tutorialNurse.spawnedInstance);
        tutorialNurseFOV.OnTargetSpotted += HandleTutorialNurseSpottingPlayer;  // we can handle convos that occur when nurse spots birdie

        // Fade in and allow to pause, and lock the door to start
        UIManager.Instance.FadeIn();
        UIManager.Instance.CanPause = true;
        RegionManager.Instance.nurseRoomDoor.SetLocked(true);
        RegionManager.Instance.nurseRoomDoor.OnPlayerCollideWithDoor += OnPlayerCollideWithNurseRoomDoor;

        // Wake up cutscene + enabling movement afterwards
        if (!GameManager.Instance.skipSettings.allRealtimeCutscenes)
        {
            GameManager.Instance.GetPlayerAnimator().SetTrigger(Constants.ANIMATION_BIRDIE_WAKEUP);
            float animationLength = GameManager.Instance.GetPlayerAnimator().GetCurrentAnimatorClipInfo(0)[0].clip.length; // should always be there
            yield return new WaitForSeconds(animationLength * 0.8f);  // fine-tuned for best visuals
        }
        GameManager.Instance.GetPlayerRigidbody().isKinematic = false;
        GameManager.Instance.GetPlayerController().EnablePlayerInput = true;

        // we type it here because the user finally gains control, so it looks cool to then type the location they're in
        UIManager.Instance.zoneText.DisplayText(RegionManager.Instance.GetCurrentZone().regionName, RegionManager.Instance.GetCurrentZone().isRestricted);

        // Cutscene for once they enter the hallway
        RegionManager.Instance.nurseRoomDoor.OnDoorClose += OnNurseRoomDoorClose;
    }

    private void OnPlayerCollideWithNurseRoomDoor()
    {
        if (canEscape || isInCantEscapeConversation) return;

        isInCantEscapeConversation = true;
        DialogueManager.Instance.StartConversation(cantEscapeYetConversation);
        DialogueManager.Instance.OnFinishConversation += WaitForCantFinishConversationToFinish;
    }

    private void WaitForCantFinishConversationToFinish(Conversation conversation)
    {
        if (conversation == cantEscapeYetConversation)
        {
            DialogueManager.Instance.OnFinishConversation -= WaitForCantFinishConversationToFinish;
            isInCantEscapeConversation = false;
        }
    }

    private void OnNurseRoomDoorClose()
    {
        // we only care if the door closes and the player is completely out of the nurse room
        if (!RegionManager.Instance.PlayerIsInZone(RegionManager.Instance.nursesRoom))
        {
            RegionManager.Instance.nurseRoomDoor.OnDoorClose -= OnNurseRoomDoorClose;

            musicEv.Play();
            tutorialNurse.spawnedInstance.GetComponent<SpeakerUI>().Hide();  // if he's still talking, no more seeing the talking

            startCutscenePlayed = true;
            firstVase.loudObject.Drop();
            firstVase.breakableObject.OnBreak += StartVaseFocus;
        }
    }

    private void StartVaseFocus(GameObject brokenInstance)
    {
        StartCoroutine(VaseCutsceneCoroutine(brokenInstance));
    }

    private IEnumerator VaseCutsceneCoroutine(GameObject focusObject)
    {
        GameManager.Instance.GetPlayerController().EnablePlayerInput = false;
        UIManager.Instance.CanPause = false;
        UIManager.Instance.staminaBar.overrideValue = true;
        UIManager.Instance.staminaBar.overrideTo = 0f;
        CinemachineVirtualCamera currentCamera = CameraManager.Instance.GetActiveVirtualCamera();

        // Cutscene Order: Vase Focus -> Outline Awakeness Meter -> Enemies Come -> Birdie Runs Away
        Time.timeScale = 0f;
        yield return StartCoroutine(MissionManager.Instance.PlayCutsceneText(awakenessPointerUIAnimation));
        Time.timeScale = 1f;
        yield return StartCoroutine(MissionManager.Instance.PlayCutscenePart(currentCamera, vaseFocusCameraPrefab, vaseDropCutsceneText, focusObject.transform));
        SpawnEnemies();
        SetEnemyAnimationSpeed(enemyCutsceneAnimationSpeed);  // make all slower than usual for now
        yield return StartCoroutine(MissionManager.Instance.PlayCutscenePart(currentCamera, enemyFocusCameraPrefab, enemyCutsceneText, spawnedEnemies[0].gameObject.transform, doHardBlend: true));
        ResetEnemySpeed();  // reset to assigned speeds
        GameManager.Instance.GetPlayerController().EnablePlayerInput = true;
        UIManager.Instance.CanPause = true;
        UIManager.Instance.staminaBar.OnLightningEnabled += StartSpecialAbilityTutorial;
        UIManager.Instance.staminaBar.overrideValue = false;

        yield return StartCoroutine(MissionManager.Instance.PlayCutsceneText(birdieRunawayCutsceneText));
    }

    private void StartSpecialAbilityTutorial(bool enabled)
    {
        if (!enabled) return;

        // this is the moment we've all been waiting for
        UIManager.Instance.staminaBar.OnLightningEnabled -= StartSpecialAbilityTutorial;

        StartCoroutine(DisplaySpecialAbilityTutorial());
    }

    private IEnumerator DisplaySpecialAbilityTutorial()
    {
        Time.timeScale = 0f;
        UIManager.Instance.CanPause = false;
        GameManager.Instance.GetPlayerController().EnablePlayerInput = false;
        yield return StartCoroutine(MissionManager.Instance.PlayCutsceneText(specialAbilityPointerUIAnimation));
        GameManager.Instance.GetPlayerController().EnablePlayerInput = true;
        UIManager.Instance.CanPause = true;
        Time.timeScale = 1f;
    }

    private void SpawnEnemies()
    {
        chaserGroups.ForEach(group =>
        {
            group.enemyStartPositions.ForEach(position =>
            {
                GameObject enemyInstance = Instantiate(chaserPrefab, position, Quaternion.identity);

                PureChaser chaser = Utils.GetRequiredComponent<PureChaser>(enemyInstance);
                chaser.targetTransform = GameManager.Instance.GetPlayerTransform();
                chaser.SetSpeed(group.chaseSpeed);
                chaser.startChaseRadius = group.startChaseRadius;
                chaser.chaser.OnCollideWithPlayer += RestartAfterCutscene;

                spawnedEnemies.Add(new SpawnedEnemy(enemyInstance, chaser, group));
            });
        });
    }

    private void SetEnemyAnimationSpeed(float speed)
    {
        spawnedEnemies.ForEach(enemy =>
        {
            PureChaser chaser = Utils.GetRequiredComponent<PureChaser>(enemy.gameObject);
            chaser.SetAnimationSpeed(speed);
        });
    }

    private void ResetEnemySpeed()
    {
        spawnedEnemies.ForEach(enemy =>
        {
            PureChaser chaser = Utils.GetRequiredComponent<PureChaser>(enemy.gameObject);
            chaser.SetAnimationSpeed(1f);
            chaser.SetSpeed(enemy.chaserGroup.chaseSpeed);
        });
    }

    private void RestartAfterCutscene()
    {
        if (!respawning && !missionCompleting)
        {
            respawning = true;
            StartCoroutine(RestartAfterCutsceneCoroutine());
        }
    }

    private IEnumerator RestartAfterCutsceneCoroutine()
    {
        UIManager.Instance.FadeOut();
        yield return new WaitForSeconds(UIManager.Instance.fadeSpeed);
        GameManager.Instance.GetPlayerTransform().position = playerRespawnPosition;
        GameManager.Instance.GetPlayerTransform().rotation = Quaternion.Euler(playerRespawnRotation);
        GameManager.Instance.GetMovementController().ResetVelocity();
        UIManager.Instance.staminaBar.ResetAwakeness();

        // would be weird if it disappeared, but the first vase should still be destroyed at this point
        DestroyAllObjects(exceptFirstVaseStand: true);

        SpawnRegularVases();
        SpawnEnemies();
        SpawnExtraObjects();

        UIManager.Instance.FadeIn();
        yield return new WaitForSeconds(UIManager.Instance.fadeSpeed);

        respawning = false;
    }

    private SpawnedVase SpawnVase(Vector3 position)
    {
        GameObject vaseInstance = Instantiate(vasePrefab, position, Quaternion.identity);
        GameObject vaseStandInstance = Instantiate(vaseStandPrefab, new Vector3(position.x - 1f, 1.5f, position.z - 1f), Quaternion.identity);

        BreakableObject breakableVase = Utils.GetRequiredComponentInChildren<BreakableObject>(vaseInstance);
        breakableVase.OnBreak += OnVaseBreak;

        return new SpawnedVase(vaseInstance, vaseStandInstance);
    }

    private void OnVaseBreak(GameObject brokenInstance)
    {
        spawnedBrokenVases.Add(brokenInstance);
    }

    protected override void Cleanup()
    {
        // Here we have checks for all the instances specifically because this can be called on App shutdown
        //  this means its possible for some Singletons to have already been garbage collected by the time we get here
        Debug.Log("MissionTutorial Cleanup()!");

        if (GameManager.Instance)
        {
            GameManager.Instance.GetPlayerController().EnablePlayerInput = true;
            GameManager.Instance.GetPlayerRigidbody().isKinematic = false;
        }

        // Update GameEventManager
        if (GameEventManager.Instance)
        {
            GameEventManager.Instance.SetEventStatus(GameEventManager.GameEvent.TutorialActive, false);
        }

        // Destroying the note
        if (MissionManager.Instance)
        {
            // Delete the note if it still exists
            //MissionManager.Instance.DestroyMissionObject(note);
            MissionManager.Instance.DestroyMissionObject(tutorialNurse);
        }

        if (RegionManager.Instance)
        {
            RegionManager.Instance.nurseRoomDoor.SetLocked(false);
        }

        // Handle the cutscene event handlers
        if (!startCutscenePlayed && RegionManager.Instance)
        {
            RegionManager.Instance.nurseRoomDoor.OnDoorClose -= OnNurseRoomDoorClose;
        }
        startCutscenePlayed = false;

        // Destroy all spawned objects
        DestroyAllObjects();
    }

    private void SpawnRegularVases()
    {
        vasePositions.ForEach(p =>
        {
            spawnedVases.Add(SpawnVase(p));
        });
    }

    private void SpawnExtraObjects()
    {
        extraObjects.ForEach(i =>
        {
            i.spawnedInstance = MissionManager.Instance.SpawnMissionObject(i);
        });
    }

    private void DestroyAllObjects(bool exceptFirstVaseStand = false)
    {
        if (firstVase != null)
        {
            if (firstVase.vaseObject)
            {
                Destroy(firstVase.vaseObject);
            }
            if (!exceptFirstVaseStand && firstVase.vaseStand)
            {
                Destroy(firstVase.vaseStand);
            }
        }
        DestroyFromList(spawnedBrokenVases);
        DestroyFromList(spawnedVases.Select(v => v.vaseObject).ToList());
        DestroyFromList(spawnedVases.Select(v => v.vaseStand).ToList());
        spawnedVases.Clear();
        DestroyFromList(spawnedEnemies.Select(e => e.gameObject).ToList());
        spawnedEnemies.Clear();
        if (MissionManager.Instance)
        {
            MissionManager.Instance.DestroyMissionObjects(extraObjects);
        }
    }

    private void DestroyFromList(List<GameObject> gameObjects)
    {
        if (gameObjects != null && gameObjects.Count > 0)
        {
            gameObjects.ForEach(o =>
            {
                if (o)
                {
                    Destroy(o);
                }
            });
        }
    }
}
