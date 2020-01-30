using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class MissionInteractable
{
    public GameObject prefab;
    public Vector3 position;
    public Vector3 rotation;
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

    public GameObject prefab;
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;
    public List<EnemyWaypoint> waypoints; 
}

public class MissionCafeteria1 : MonoBehaviour, IMission
{
    public List<MissionEnemy> startEnemies;

    [Header("All lists below must be the same size!")]
    public List<MissionInteractable> missionInteractables;

    private List<GameObject> instantiatedMissionInteractables;
    private List<NursePatrol> instantiatedEnemies;
    private int interactedCount = 0;

    // TODO: Remove all properties below when interactables are implemented
    [Header("Remove below once we have interactables")]
    public bool testOnCollectSpawning = false;
    public int interactableIndexToTest;

    private void Awake()
    {
        instantiatedMissionInteractables = new List<GameObject>();
        instantiatedEnemies = new List<NursePatrol>();
    }

    ////////////////////////////////////////////////////
    // TOOD: REMOVE ONCE INTERACTABLES ARE IMPLEMENTED
    private void Start()
    {
        if (testOnCollectSpawning && interactableIndexToTest >= 0 && interactableIndexToTest < missionInteractables.Count)
        {
            SpawnEnemies(missionInteractables[interactableIndexToTest].enemiesToSpawnIfLastCollected);
        }
    }
    ////////////////////////////////////////////////////

    public void OnEnable()
    {
        SpawnInteractables(missionInteractables);
        SpawnEnemies(startEnemies);
    }

    private void Cleanup()
    {
        DestroyGameObjects(instantiatedMissionInteractables);
        DestroyGameObjects(instantiatedEnemies.Where(e => e).Select(e => e.gameObject).ToList());
    }

    private void OnDisable()
    {
        Cleanup();
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void SpawnInteractables(List<MissionInteractable> interactables)
    {
        // Instantiate all interactable objects
        interactables.ForEach(i =>
        {
            GameObject interactableGameObject = Instantiate(i.prefab, i.position, Quaternion.Euler(i.rotation));
            Interactable interactable = Utils.GetRequiredComponent<Interactable>(interactableGameObject);

            // Here we register for the event that 
            interactable.OnInteractEnd += HandleInteractedWith;

            instantiatedMissionInteractables.Add(interactableGameObject);  // TODO: make this a list of type INTERACTBLE_SCRIPT instead
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
                NursePatrol patrol = Utils.GetRequiredComponent<NursePatrol>(spawnedEnemy, $"Enemy in MissionCafeteria1 does not have a NursePatrol component!");

                patrol.targetTransform = GameManager.Instance.GetPlayerTransform();
                patrol.SetPoints(enemy.waypoints.Select(waypoint => waypoint.position).ToList());

                instantiatedEnemies.Add(patrol);
            }
            else
            {
                Debug.LogError("Could not sample position to spawn enemy for MissionCafeteria1!");
            }
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

        if (interactedCount == missionInteractables.Count())
        {
            // Logic based on the interactable object for spawning
            int interactableIndex = instantiatedMissionInteractables.IndexOf(interactable.gameObject);

            SpawnEnemies(missionInteractables[interactableIndex].enemiesToSpawnIfLastCollected);
        }

        // Probably TEMP solution for after interacting
        // - will probably have to call something in Interactable to initiate some destroy sequence
        // - this way be something like a coroutine which will animate the fade away and then destroy the object
        // - nice animation for fading away the entire object would be cool as well (POLISH)
        Destroy(interactable.gameObject);
    }
}
