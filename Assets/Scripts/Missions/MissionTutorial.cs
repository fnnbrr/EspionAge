using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;

public class MissionTutorial : AMission
{
    public Vector3 playerStartPosition;
    public Vector3 playerStartRotation;
    public Vector3 playerRespawnPosition;
    public Vector3 playerRespawnRotation;

    [Header("Cutscene")]
    public List<string> startCutsceneTexts;
    public GameObject awakenessPointerUIAnimation;
    public GameObject vaseFocusCameraPrefab;
    public GameObject vaseDropCutsceneText;
    public GameObject enemyFocusCameraPrefab;
    public GameObject enemyCutsceneText;
    public GameObject birdieRunawayCutsceneText;

    [Header("Vase - General")]
    public GameObject vasePrefab;
    public GameObject vaseStandPrefab;

    [Header("First Vase")]
    public Vector3 firstVasePosition;
    private SpawnedVase firstVase;

    [Header("Row of Vases")]
    public List<Vector3> vasePositions;
    private List<SpawnedVase> spawnedVases;
    private List<GameObject> spawnedBrokenVases;
    // stand position must then be, (x - 0.5, 1.5, z - 1) (empirically)

    [Header("Chaser Enemies")]
    public GameObject chaserPrefab;
    public float enemyCutsceneSpeed = 3;
    public List<TutorialChaserGroup> chaserGroups;

    //[Header("Note")]
    //public MissionObject note;

    [Header("Dining Room Blocking")]
    public GameObject blockObject;
    public List<Vector3> doorBlockingObjectPositions;
    private List<GameObject> spawnedBlockingObjects;

    private bool startCutscenePlayed = false;
    private bool respawning = false;

    private List<SpawnedEnemy> spawnedEnemies;
    private List<GameObject> camerasToCleanUp;

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
        camerasToCleanUp = new List<GameObject>();
        spawnedBlockingObjects = new List<GameObject>();
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
        SpawnBlockingObjects();

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

        // Fade in
        UIManager.Instance.FadeIn();

        // Start the note spawning and start the animation
        //note.spawnedInstance = MissionManager.Instance.SpawnMissionObject(note);

        // Cutscene for once they enter the hallway
        RegionManager.Instance.hallway.OnPlayerEnter += StartDropCutscene;
    }

    private void StartDropCutscene()
    {
        RegionManager.Instance.hallway.OnPlayerEnter -= StartDropCutscene;
        StartCoroutine(WaitForPlayerMovement());
    }

    private IEnumerator WaitForPlayerMovement()
    {
        while (!GameManager.Instance.GetMovementController().IsMoving)
        {
            yield return new WaitForFixedUpdate();
        }
        startCutscenePlayed = true;
        firstVase.loudObject.Drop();
        firstVase.breakableObject.OnBreak += StartVaseFocus;
    }

    private void StartVaseFocus(GameObject brokenInstance)
    {
        StartCoroutine(VaseCutsceneCoroutine(brokenInstance));
    }

    private IEnumerator VaseCutsceneCoroutine(GameObject focusObject)
    {
        GameManager.Instance.GetPlayerController().EnablePlayerInput = false;
        CinemachineVirtualCamera currentCamera = CameraManager.Instance.GetActiveVirtualCamera();

        // Cutscene Order: Vase Focus -> Outline Awakeness Meter -> Enemies Come -> Birdie Runs Away
        Time.timeScale = 0f;
        yield return StartCoroutine(PlayCutsceneText(awakenessPointerUIAnimation));
        Time.timeScale = 1f;
        yield return StartCoroutine(PlayCutscenePart(currentCamera, vaseFocusCameraPrefab, vaseDropCutsceneText, focusObject.transform));
        SpawnEnemies();
        SetEnemySpeed(enemyCutsceneSpeed);  // make all slower than usual for now
        yield return StartCoroutine(PlayCutscenePart(currentCamera, enemyFocusCameraPrefab, enemyCutsceneText, spawnedEnemies[0].gameObject.transform, doHardBlend: true));
        ResetEnemySpeed();  // reset to assigned speeds
        GameManager.Instance.GetPlayerController().EnablePlayerInput = true;

        yield return StartCoroutine(PlayCutsceneText(birdieRunawayCutsceneText));
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
        SpawnBlockingObjects();

        UIManager.Instance.FadeIn();
        yield return new WaitForSeconds(UIManager.Instance.fadeSpeed);

        respawning = false;
    }

    private IEnumerator PlayCutscenePart(CinemachineVirtualCamera startCamera, GameObject cameraPrefab, GameObject cutsceneTextPrefab, Transform focusTransform, bool doHardBlend = false)
    {
        if (GameManager.Instance.skipSettings.allRealtimeCutscenes) yield break;

        GameObject cutsceneCameraInstance = CameraManager.Instance.SpawnCameraFromPrefab(cameraPrefab);

        if (doHardBlend)
        {
            CinemachineBlenderSettings.CustomBlend hardBlend = new CinemachineBlenderSettings.CustomBlend
            {
                m_From = CinemachineBlenderSettings.kBlendFromAnyCameraLabel,
                m_To = cutsceneCameraInstance.name,
                m_Blend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f)
            };
            CameraManager.Instance.brain.m_CustomBlends.m_CustomBlends = 
                CameraManager.Instance.brain.m_CustomBlends.m_CustomBlends.Append(hardBlend).ToArray();
        }

        cutsceneCameraInstance.GetComponent<CinemachineVirtualCamera>().LookAt = focusTransform;
        camerasToCleanUp.Add(cutsceneCameraInstance);
        CameraManager.Instance.BlendTo(cutsceneCameraInstance.GetComponent<CinemachineVirtualCamera>(), alertGlobally: false);

        yield return StartCoroutine(PlayCutsceneText(cutsceneTextPrefab));

        CameraManager.Instance.BlendTo(startCamera, alertGlobally: false);
        CameraManager.Instance.OnBlendingComplete += CleanupCamerasAfterBlending;
    }

    private IEnumerator PlayCutsceneText(GameObject cutsceneTextPrefab)
    {
        if (GameManager.Instance.skipSettings.allRealtimeCutscenes) yield break;

        GameObject instantiatedCutsceneText = Instantiate(cutsceneTextPrefab, UIManager.Instance.mainUICanvas.transform);
        Animator textAnimator = Utils.GetRequiredComponentInChildren<Animator>(instantiatedCutsceneText);
        float textAnimationLength = textAnimator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSecondsRealtime(textAnimationLength);
        Destroy(instantiatedCutsceneText);
    }

    private void CleanupCamerasAfterBlending(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera)
    {
        CameraManager.Instance.OnBlendingComplete += CleanupCamerasAfterBlending;
        camerasToCleanUp.ForEach(c =>
        {
            Destroy(c);
        });
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
        }

        // Handle the cutscene event handlers
        if (!startCutscenePlayed && RegionManager.Instance)
        {
            RegionManager.Instance.hallway.OnPlayerEnter -= StartDropCutscene;
        }
        startCutscenePlayed = false;

        // Destroy all spawned objects
        DestroyAllObjects();
        DestroyFromList(camerasToCleanUp);
    }

    private void SpawnRegularVases()
    {
        vasePositions.ForEach(p =>
        {
            spawnedVases.Add(SpawnVase(p));
        });
    }

    private void SpawnBlockingObjects()
    {
        doorBlockingObjectPositions.ForEach(p =>
        {
            spawnedBlockingObjects.Add(Instantiate(blockObject, p, Quaternion.identity));
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
        DestroyFromList(spawnedEnemies.Select(e => e.gameObject).ToList());
        DestroyFromList(spawnedBlockingObjects);
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
