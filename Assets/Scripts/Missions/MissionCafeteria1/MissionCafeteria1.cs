using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Cinemachine;

[System.Serializable]
public class MissionCriticalInteractable : MissionObject
{
    public List<MissionEnemy> enemiesToSpawnIfLastCollected;
}

[System.Serializable]
public class MissionEnemy
{
    [System.Serializable]
    public class EnemyWaypoint
    {
        public Vector3 position;
        public float stayTime;
    }

    [System.Serializable]
    public class ChaserWanderBounds
    {
        public Vector3 position;
        public float radius;
    }

    public enum EnemyType
    {
        Patroller,
        Chaser
    }
    [SerializeField] public EnemyType enemyType;

    [Header("Generic Enemy Settings")]
    public GameObject prefab;
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;

    [Header("Specific Patroller Settings")]
    public List<EnemyWaypoint> waypoints;

    [Header("Specific Chaser Settings")]
    public Vector3 startResponsePoint;
    public ChaserWanderBounds wanderBounds;
}

public class MissionCafeteria1 : AMission
{
    public Vector3 respawnPosition;

    public GameObject startCutsceneCamera;

    public List<MissionEnemy> startEnemies;

    [Header("All lists below must be the same size!")]
    public List<MissionCriticalInteractable> missionCriticalInteractables;
    public List<MissionObject> missionObjects;

    private List<GameObject> instantiatedMissionInteractables;
    private List<Chaser> instantiatedEnemies;
    private int interactedCount = 0;

    private bool isRestarting = false;
    private bool startCutscenePlayed = false;

    private void Awake()
    {
        instantiatedMissionInteractables = new List<GameObject>();

        // TODO: this should probably be changed to a generic enemy type at some point
        instantiatedEnemies = new List<Chaser>();
    }

    protected override void Initialize()
    {
        interactedCount = 0;

        SpawnObjects(missionObjects);
        SpawnInteractables(missionCriticalInteractables);
        SpawnEnemies(startEnemies);

        RegionManager.Instance.kitchen.OnPlayerEnter += StartCutscene;
    }

    protected override void Cleanup()
    {
        DestroyMissionObjects(missionObjects);
        DestroyGameObjects(instantiatedMissionInteractables);
        DestroyGameObjects(instantiatedEnemies.Where(e => e).Select(e => e.gameObject).ToList());

        instantiatedMissionInteractables.Clear();
        instantiatedEnemies.Clear();

        if (!startCutscenePlayed && RegionManager.Instance)
        {
            RegionManager.Instance.kitchen.OnPlayerEnter -= StartCutscene;
        }
        startCutscenePlayed = false;
    }

    private void SpawnObjects(List<MissionObject> interactables)
    {
        // Instantiate all interactable objects
        interactables.ForEach(i =>
        {
            i.spawnedInstance = MissionManager.Instance.SpawnMissionObject(i);
        });
    }

    private void SpawnInteractables(List<MissionCriticalInteractable> interactables)
    {
        // Instantiate all interactable objects
        interactables.ForEach(i =>
        {
            GameObject interactableGameObject = Instantiate(i.prefab, i.position, Quaternion.Euler(i.rotation));
            Interactable interactable = Utils.GetRequiredComponent<Interactable>(interactableGameObject);

            interactable.OnInteractEnd += HandleInteractedWith;
            instantiatedMissionInteractables.Add(interactableGameObject);
        });
    }


    private void SpawnEnemies(List<MissionEnemy> enemies)
    {
        // Instantiate all enemies
        enemies.ForEach(enemy =>
        {
            // We can only spawn a NavMeshAgent on a position close enough to a NavMesh, so we must sample the inputted position first just in case.
            if (NavMesh.SamplePosition(enemy.spawnPosition, out NavMeshHit closestNavmeshHit, 10.0f, NavMesh.AllAreas))
            {
                GameObject spawnedEnemy = Instantiate(enemy.prefab, closestNavmeshHit.position, Quaternion.Euler(enemy.spawnRotation));
             
                // All enemies will be chasers, so we need to set the target transform for all.
                Chaser enemyComponent = Utils.GetRequiredComponent<Chaser>(spawnedEnemy, $"Enemy in MissionCafeteria1 does not have a Chaser component!");
                enemyComponent.targetTransform = GameManager.Instance.GetPlayerTransform();
                enemyComponent.OnCollideWithPlayer += OnCollideWithPlayer;

                switch (enemy.enemyType)
                {
                    case MissionEnemy.EnemyType.Patroller:
                        Patroller patrol = enemyComponent as Patroller;
                        patrol.SetPoints(enemy.waypoints.Select(waypoint => waypoint.position).ToList());
                        break;
                    case MissionEnemy.EnemyType.Chaser:
                        Chaser chaser = enemyComponent as Chaser;
                        chaser.InitializeResponderParameters(enemy.startResponsePoint, enemy.wanderBounds.position, enemy.wanderBounds.radius);
                        break;
                    default:
                        Debug.LogError($"Unknown enemy type: {enemy.enemyType}!");
                        break;
                }

                instantiatedEnemies.Add(enemyComponent);
            }
            else
            {
                Debug.LogError("Could not sample position to spawn enemy for MissionCafeteria1!");
            }
        });
    }

    private void StartCutscene()
    {
        // The current assumptions are gonna be that:
        //  - we will have a camera where we need to set the look at
        //  - and it will have an already created Animator with a single state that it will start on Awake
        //
        // So we can just destroy it after the animation time is over and it should switch back to whatever the current camera was
        RegionManager.Instance.kitchen.OnPlayerEnter -= StartCutscene;

        if (startCutscenePlayed) return;

        startCutscenePlayed = true;

        if (!startCutsceneCamera || instantiatedMissionInteractables.Count == 0) return;

        GameManager.Instance.GetPlayerController().EnablePlayerInput = false;

        GameObject instantiatedCutscenePrefab = Instantiate(startCutsceneCamera);
        CinemachineVirtualCamera virtualCamera = instantiatedCutscenePrefab.GetComponentInChildren<CinemachineVirtualCamera>();
        Animator virtualCameraAnim = instantiatedCutscenePrefab.GetComponentInChildren<Animator>();
        if (!virtualCamera || !virtualCameraAnim) return;

        virtualCamera.LookAt = instantiatedMissionInteractables[0].transform;

        StartCoroutine(EndCutsceneOperations(instantiatedCutscenePrefab, virtualCameraAnim.GetCurrentAnimatorStateInfo(0).length * 1.5f));
    }

    private IEnumerator EndCutsceneOperations(GameObject cutsceneObject, float cutsceneLength)
    {
        yield return new WaitForSeconds(cutsceneLength);

        UIManager.Instance.FadeOut();

        yield return new WaitForSeconds(UIManager.Instance.fadeSpeed);

        Destroy(cutsceneObject);

        UIManager.Instance.FadeIn();

        GameManager.Instance.GetPlayerController().EnablePlayerInput = true;
    }

    private void OnCollideWithPlayer()
    {
        if (isRestarting) return;

        isRestarting = true;

        // Possible things we can look into the future:
        // - maybe some UI like "Oops! You got caught!"
        // - or even start some simple "caught" dialog with the person who caught you

        UIManager.Instance.OnFadingComplete += OnFadingCompleteRestartMission;
        UIManager.Instance.FadeOut();

        // Tell any listeners (looking at you ProgressManager) that we need to reset whatever mission status
        AlertMissionReset();
    }

    private void OnFadingCompleteRestartMission()
    {
        UIManager.Instance.OnFadingComplete -= OnFadingCompleteRestartMission;

        bool alreadyPlayedCutscene = startCutscenePlayed;

        Cleanup();
        Initialize();
        GameManager.Instance.GetPlayerTransform().position = respawnPosition;
        startCutscenePlayed = alreadyPlayedCutscene;

        isRestarting = false;

        UIManager.Instance.FadeIn();
    }

    private void DestroyMissionObjects(List<MissionObject> missionObjects)
    {
        missionObjects.ForEach(o =>
        {
            MissionManager.Instance.DestroyMissionObject(o);
        });
    }

    private void DestroyGameObjects(List<GameObject> gameObjects)
    {
        gameObjects.ForEach(o =>
        {
            if (o)
            {
                Destroy(o);
            }
        });
    }

    private void HandleInteractedWith(Interactable interactable)
    {
        interactedCount++;

        if (interactedCount == missionCriticalInteractables.Count())
        {
            // Logic based on the interactable object for spawning
            int interactableIndex = instantiatedMissionInteractables.IndexOf(interactable.gameObject);

            SpawnEnemies(missionCriticalInteractables[interactableIndex].enemiesToSpawnIfLastCollected);

            CameraManager.Instance.BlendToFarCameraForSeconds(2);

            AlertMissionComplete();
        }

        // Probably TEMP solution for after interacting
        // - will probably have to call something in Interactable to initiate some destroy sequence
        // - this way be something like a coroutine which will animate the fade away and then destroy the object
        // - nice animation for fading away the entire object would be cool as well (POLISH)
        Destroy(interactable.gameObject);
    }
}
