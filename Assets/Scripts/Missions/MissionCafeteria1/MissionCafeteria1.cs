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

    private void Awake()
    {
        instantiatedMissionInteractables = new List<GameObject>();
        instantiatedEnemies = new List<NursePatrol>();
    }

    public void OnEnable()
    {
        // Instantiate all interactable objects
        missionInteractables.ForEach(i =>
        {
            GameObject interactableGameObject = Instantiate(i.prefab, i.position, Quaternion.Euler(i.rotation));

            // Here we register for the event that 
            // interactableGameObject.GetComponent<INTERACTBLE_SCRIPT>().EVENT += HandleInteractedWith;

            instantiatedMissionInteractables.Add(interactableGameObject);
        });

        // Instantiate all enemies
        startEnemies.ForEach(enemy =>
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

    private void OnDisable()
    {
        instantiatedMissionInteractables.ForEach(i =>
        {
            Destroy(i);
        });

        instantiatedEnemies.ForEach(e =>
        {
            if (e && e.gameObject)
            {
                Destroy(e.gameObject);
            }
        });
    }

    /*
    // TODO: Connect with future Interactable system (Justine & Chadley)
    private void HandleInteractedWith(INTERACTABLE_SCRIPT interactable)
    {
        interactedCount++;

        if (interactedCount == missionInteractables.Count())
        {
            // Logic based on the interactable object for spawning
        }
    }
    */
}
