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
    private GameObject firstVaseInstance;
    private LoudObject firstVaseLoudObject;
    private BreakableObject firstVaseBreakable;

    [Header("Row of Vases")]
    public List<Vector3> vasePositions;
    private List<GameObject> spawnedVases;
    private List<GameObject> spawnedVaseStands;
    // stand position must then be, (x - 0.5, 1.5, z - 1) (empirically)

    [Header("Chaser Enemies")]
    public GameObject chaserPrefab;
    public List<Vector3> enemyPositions;

    //[Header("Note")]
    //public MissionObject note;

    private bool startCutscenePlayed = false;
    private bool respawning = false;

    private List<GameObject> spawnedEnemies;
    private List<GameObject> camerasToCleanUp;

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
        spawnedVases = new List<GameObject>();
        spawnedVaseStands = new List<GameObject>();
        spawnedEnemies = new List<GameObject>();
        camerasToCleanUp = new List<GameObject>();
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
        firstVaseInstance = SpawnVase(firstVasePosition);
        firstVaseLoudObject = firstVaseInstance.GetComponentInChildren<LoudObject>();
        firstVaseLoudObject.dropRadius = 0f;
        firstVaseBreakable = firstVaseInstance.GetComponentInChildren<BreakableObject>();

        // Spawn all the other vases
        SpawnRegularVases();

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
        while (!GameManager.Instance.GetPlayerController().IsMoving)
        {
            yield return new WaitForFixedUpdate();
        }
        startCutscenePlayed = true;
        firstVaseLoudObject.Drop();
        firstVaseBreakable.OnBreak += StartVaseFocus;
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
        yield return StartCoroutine(PlayCutscenePart(currentCamera, vaseFocusCameraPrefab, vaseDropCutsceneText, focusObject.transform));
        yield return StartCoroutine(PlayCutsceneText(awakenessPointerUIAnimation));
        SpawnEnemies();
        yield return StartCoroutine(PlayCutscenePart(currentCamera, enemyFocusCameraPrefab, enemyCutsceneText, spawnedEnemies[0].transform, doHardBlend: true));

        GameManager.Instance.GetPlayerController().EnablePlayerInput = true;

        yield return StartCoroutine(PlayCutsceneText(birdieRunawayCutsceneText));
    }

    private void SpawnEnemies()
    {
        enemyPositions.ForEach(p =>
        {
            GameObject enemyInstance = Instantiate(chaserPrefab, p, Quaternion.identity);
            spawnedEnemies.Add(enemyInstance);

            PureChaser chaser = Utils.GetRequiredComponent<PureChaser>(enemyInstance);
            chaser.targetTransform = GameManager.Instance.GetPlayerTransform();
            chaser.OnCollideWithPlayer += RestartAfterCutscene;
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

        DestroyFromList(spawnedVases);
        DestroyFromList(spawnedVaseStands);
        DestroyFromList(spawnedEnemies);

        SpawnRegularVases();
        SpawnEnemies();

        UIManager.Instance.FadeIn();
        yield return new WaitForSeconds(UIManager.Instance.fadeSpeed);

        respawning = false;
    }

    private IEnumerator PlayCutscenePart(CinemachineVirtualCamera startCamera, GameObject cameraPrefab, GameObject cutsceneTextPrefab, Transform focusTransform, bool doHardBlend = false)
    {
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
        GameObject instantiatedCutsceneText = Instantiate(cutsceneTextPrefab, UIManager.Instance.mainUICanvas.transform);
        Animator textAnimator = Utils.GetRequiredComponentInChildren<Animator>(instantiatedCutsceneText);
        float textAnimationLength = textAnimator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(textAnimationLength);
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

    private GameObject SpawnVase(Vector3 position)
    {
        GameObject vaseInstance = Instantiate(vasePrefab, position, Quaternion.identity);
        spawnedVaseStands.Add(Instantiate(vaseStandPrefab, new Vector3(position.x - 1f, 1.5f, position.z - 1f), Quaternion.identity));

        BreakableObject breakableVase = Utils.GetRequiredComponentInChildren<BreakableObject>(vaseInstance);
        breakableVase.OnBreak += OnVaseBreak;

        return vaseInstance;
    }

    private void OnVaseBreak(GameObject brokenInstance)
    {
        spawnedVases.Add(brokenInstance);
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

    private void DestroyAllObjects()
    {
        if (firstVaseInstance)
        {
            Destroy(firstVaseInstance);
        }
        DestroyFromList(spawnedVases);
        DestroyFromList(spawnedVaseStands);
        DestroyFromList(spawnedEnemies);
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
