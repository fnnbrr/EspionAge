using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;
using NaughtyAttributes;

public class MissionTutorial : AMission
{
    public Vector3 playerStartPosition;
    public Vector3 playerStartRotation;
    public Vector3 playerRespawnPosition;
    public Vector3 playerRespawnRotation;

    [BoxGroup("Nurse Room Sequence")]
    public MissionObject tutorialNurse;
    [BoxGroup("Nurse Room Sequence")]
    public string tutorialNurseSpeakerId;
    [BoxGroup("Nurse Room Sequence")]
    [FMODUnity.EventRef]
    public string tutorialNurseVoicePath;
    [BoxGroup("Nurse Room Sequence")]
    [ReorderableList]
    public List<Conversation> nurseConversations;

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

    //[Header("Note")]
    //public MissionObject note;

    [Header("Misc. Objects")]
    public List<MissionObject> extraObjects;

    private bool startCutscenePlayed = false;
    private bool respawning = false;

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
        public PureChaser chaser;
        public TutorialChaserGroup chaserGroup;

        public SpawnedEnemy(GameObject o, PureChaser c, TutorialChaserGroup g)
        {
            gameObject = o;
            chaser = c;
            chaserGroup = g;
        }
    }

    // General Logic Overview:
    // * start out faded out
    // * move player to the spawn point (in her room)
    // * spawn all the vases
    // * mission giver at the end of the hallway
    //   * somehow make the mission giver have a certain conversation
    //   * somehow wait for the player to talk to the the mission giver to end the mission
    // * fade in
    // - start animation of note going under the door
    //   - this note should be an interactable which will have some text of birdie speaking to herself (reading outloud i guess)
    // * for for birdie to exit to hallway before starting cutscene
    //   * vase drops, birdie gets excited
    //   * enemies come to yell at her
    //   * birdie runs away
    // * finish mission once we finishing interacting with the mission giver (which will also give us the next mission)

    private void Awake()
    {
        spawnedVases = new List<SpawnedVase>();
        spawnedBrokenVases = new List<GameObject>();
        spawnedEnemies = new List<SpawnedEnemy>();
    }

    protected override void Initialize()
    {
        Debug.Log("MissionTutorial Initialize()!");

        // Force fade out
        UIManager.Instance.InstantFadeOut();

        // Move player to a set position
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

        // Tutorial Nurse Section Init
        tutorialNurse.spawnedInstance = MissionManager.Instance.SpawnMissionObject(tutorialNurse);
        DialogueManager.Instance.AddSpeaker(
            new SpeakerContainer(
                tutorialNurseSpeakerId, 
                tutorialNurse.spawnedInstance, 
                tutorialNurseVoicePath));
        RegionManager.Instance.nurseRoomDoor.SetLocked(true);

        // Listen for the player to pass through the final door
        RegionManager.Instance.finalHallwayDoor.OnPlayerPassThrough += CommenceCompleteMission;

        StartCoroutine(StartMissionLogic());
    }

    private void CommenceCompleteMission()
    {
        spawnedEnemies.ForEach(e =>
        {
            // Send back all enemies to around the area of their start (mostly to get them off camera)
            e.chaser.targetTransform = null;
            e.chaser.SetDestination(e.chaserGroup.enemyStartPositions[0]);
            e.chaser.OnReachDestination += HandleEnemyReachedStartPoint;
        });
    }
    private void HandleEnemyReachedStartPoint()
    {
        AlertMissionComplete();
        MissionManager.Instance.EndMission(MissionsEnum.MissionTutorial);
    }

    private IEnumerator StartMissionLogic()
    {
        foreach (string text in startCutsceneTexts)
        {
            yield return UIManager.Instance.textOverlay.SetText(text);
        }

        // Fade in, and start typing the correct zone name from this point
        CameraZone currentZone = RegionManager.Instance.GetCurrentZone();
        UIManager.Instance.zoneText.SetEmptyText(currentZone.isRestricted);
        UIManager.Instance.FadeIn();
        UIManager.Instance.zoneText.DisplayText(currentZone.regionName, currentZone.isRestricted);

        // Start the note spawning and start the animation
        //note.spawnedInstance = MissionManager.Instance.SpawnMissionObject(note);

        // Cutscene for once they enter the hallway
        RegionManager.Instance.nurseRoomDoor.OnDoorClose += OnNurseRoomDoorClose;
    }

    private void OnNurseRoomDoorClose()
    {
        if (!RegionManager.Instance.PlayerIsInZone(RegionManager.Instance.nursesRoom))
        {
            RegionManager.Instance.nurseRoomDoor.OnDoorClose -= OnNurseRoomDoorClose;

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
        GameManager.Instance.GetPlayerController().EnablePlayerInput = false;
        yield return StartCoroutine(MissionManager.Instance.PlayCutsceneText(specialAbilityPointerUIAnimation));
        GameManager.Instance.GetPlayerController().EnablePlayerInput = true;
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
                chaser.OnCollideWithPlayer += RestartAfterCutscene;

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

    private void SetEnemySpeed(float speed)
    {
        spawnedEnemies.ForEach(enemy =>
        {
            PureChaser chaser = Utils.GetRequiredComponent<PureChaser>(enemy.gameObject);
            chaser.SetSpeed(speed);
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
        if (!respawning)
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
