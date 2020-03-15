﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Cinemachine;
using NPCs;

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
        BaseAi
    }
    [SerializeField] public EnemyType enemyType;

    [Header("Generic Enemy Settings")]
    public GameObject prefab;
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;
    public bool isInitiallyResponding = false;

    [Header("Specific Patroller Settings")]
    public List<EnemyWaypoint> waypoints;

    [Header("Specific Chaser Settings")]
    public Vector3 startResponsePoint;
    public ChaserWanderBounds wanderBounds;
}

public class MissionKitchen1 : AMission
{
    public Vector3 respawnPosition;
    public Vector3 denturesCheckpointRespawnPosition;

    [Header("Cutscenes")]
    public GameObject denturesCutsceneCamera;
    public GameObject collectedDenturesCutsceneCamera;
    public float collectedDenturesCutsceneWait;

    public List<MissionEnemy> startEnemies;

    [Header("All lists below must be the same size!")]
    public List<MissionCriticalInteractable> missionCriticalInteractables;
    public List<MissionObject> missionObjects;

    private List<GameObject> instantiatedMissionInteractables;
    private List<BaseAi> instantiatedEnemies;
    private int interactedCount = 0;

    private bool isRestarting = false;
    private bool startCutscenePlayed = false;
    private bool denturesCollected = false;

    private void Awake()
    {
        instantiatedMissionInteractables = new List<GameObject>();

        // TODO: this should probably be changed to a generic enemy type at some point
        instantiatedEnemies = new List<BaseAi>();

        if (missionCriticalInteractables.Count == 0)
        {
            Utils.LogErrorAndStopPlayMode("Need at least one missionCriticalInteractables for the Kitchen Mission!");
        }
    }

    protected override void Initialize()
    {
        interactedCount = 0;

        SpawnObjects(missionObjects);
        SpawnInteractables(missionCriticalInteractables);
        SpawnEnemies(startEnemies);

        RegionManager.Instance.OnPlayerEnterZone += StartDenturesCutscene;
    }

    protected override void Cleanup()
    {
        if (MissionManager.Instance)
        {
            MissionManager.Instance.DestroyMissionObjects(missionObjects);
        }
        DestroyGameObjects(instantiatedMissionInteractables);
        DestroyGameObjects(instantiatedEnemies.Where(e => e).Select(e => e.gameObject).ToList());

        instantiatedMissionInteractables.Clear();
        instantiatedEnemies.Clear();

        if (!startCutscenePlayed && RegionManager.Instance)
        {
            RegionManager.Instance.OnPlayerEnterZone -= StartDenturesCutscene;
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
                BasicNurse enemyComponent = Utils.GetRequiredComponent<BasicNurse>(spawnedEnemy, $"Enemy in MissionCafeteria1 does not have a Chaser component!");
                enemyComponent.chaser.targetTransform = GameManager.Instance.GetPlayerTransform();
                enemyComponent.chaser.OnCollideWithPlayer += OnCollideWithPlayer;
                
                enemyComponent.patroller.SetPoints(enemy.waypoints.Select(waypoint => waypoint.position).ToList());
                if (enemy.isInitiallyResponding)
                {
                    enemyComponent.responder.InitializeResponderParameters(enemy.startResponsePoint);
                }
                
                instantiatedEnemies.Add(enemyComponent);
            }
            else
            {
                Debug.LogError("Could not sample position to spawn enemy for MissionCafeteria1!");
            }
        });
    }

    private void StartDenturesCutscene(CameraZone zone)
    {
        if (zone != RegionManager.Instance.kitchen) return;

        // The current assumptions are gonna be that:
        //  - we will have a camera where we need to set the look at
        //  - and it will have an already created Animator with a single state that it will start on Awake
        //
        // So we can just destroy it after the animation time is over and it should switch back to whatever the current camera was
        RegionManager.Instance.OnPlayerEnterZone -= StartDenturesCutscene;

        if (startCutscenePlayed) return;
        startCutscenePlayed = true;

        if (instantiatedMissionInteractables.Count == 0) return;

        StartCoroutine(StartDenturesCutsceneCoroutine());
    }

    private IEnumerator StartDenturesCutsceneCoroutine()
    {
        GameManager.Instance.GetPlayerController().EnablePlayerInput = false;

        yield return StartCoroutine(
            MissionManager.Instance.PlayMovingCameraCutscene(
                denturesCutsceneCamera, 
                instantiatedMissionInteractables[0].transform, 
                extraEndWaitMultiplier: 1.5f, 
                fadeBack: true));

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
        if (!denturesCollected)  // if we already collected the dentures, we arent technically reseting the mission status
        {
            AlertMissionReset();
        }
    }

    private void OnFadingCompleteRestartMission()
    {
        UIManager.Instance.OnFadingComplete -= OnFadingCompleteRestartMission;

        bool alreadyPlayedCutscene = startCutscenePlayed;

        if (CameraManager.Instance.GetActiveVirtualCamera() != RegionManager.Instance.kitchen.mainCamera &&
            CameraManager.Instance.GetActiveVirtualCamera() != RegionManager.Instance.diningArea.mainCamera)
        {
            Destroy(CameraManager.Instance.GetActiveVirtualCamera().gameObject);
        }
        CameraManager.Instance.BlendTo(RegionManager.Instance.kitchen.mainCamera, doHardBlend: true);
        UIManager.Instance.staminaBar.ResetAwakeness();
        Cleanup();
        Initialize();
        if (denturesCollected)
        {
            GameManager.Instance.GetPlayerTransform().position = denturesCheckpointRespawnPosition;
            DestroyGameObjects(instantiatedMissionInteractables);
        }
        else
        {
            GameManager.Instance.GetPlayerTransform().position = respawnPosition;
        }
        GameManager.Instance.GetThrowController().ResetThrowables();
        startCutscenePlayed = alreadyPlayedCutscene;

        isRestarting = false;

        if (denturesCollected)
        {
            SpawnFinalEnemyWave(0);  // just spawn the enemies for the first enemy wave (which we use every time anyways)

            CameraManager.Instance.BlendToCameraPrefabForSeconds(
                collectedDenturesCutsceneCamera, 
                collectedDenturesCutsceneWait, 
                doHardBlend: true);
        }

        UIManager.Instance.FadeIn();
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
            denturesCollected = true;

            // Logic based on the interactable object for spawning
            SpawnFinalEnemyWave(instantiatedMissionInteractables.IndexOf(interactable.gameObject));

            StartCoroutine(HandleDenturesCollectedCutscene());

            AlertMissionComplete();
        }

        // Probably TEMP solution for after interacting
        // - will probably have to call something in Interactable to initiate some destroy sequence
        // - this way be something like a coroutine which will animate the fade away and then destroy the object
        // - nice animation for fading away the entire object would be cool as well (POLISH)
        Destroy(interactable.gameObject);
    }

    private void SpawnFinalEnemyWave(int missionCriticalInteractableIndex)
    {
        SpawnEnemies(missionCriticalInteractables[missionCriticalInteractableIndex].enemiesToSpawnIfLastCollected);
    }

    private IEnumerator HandleDenturesCollectedCutscene()
    {
        GameManager.Instance.GetPlayerController().EnablePlayerInput = false;
        yield return CameraManager.Instance.BlendToCameraPrefabForSeconds(collectedDenturesCutsceneCamera, collectedDenturesCutsceneWait);
        GameManager.Instance.GetPlayerController().EnablePlayerInput = true;
    }
}
