using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using NPCs;
using Cinemachine;
using NaughtyAttributes;

[System.Serializable]
public class MissionObject
{
    public GameObject prefab;
    public Vector3 position;
    public Vector3 rotation;

    [HideInInspector]
    public GameObject spawnedInstance;
}

[System.Serializable]
public class MissionMapping
{
    [Header("Make sure this mission is unique among the list!")]
    public MissionsEnum missionType;
    public AMission mission;
    public Objective objective;
    public StampCollectible collectible;
}

public class MissionManager : Singleton<MissionManager>
{
    [Header("This will get converted to a Dictionary at runtime.")] [ReorderableList]
    public List<MissionMapping> missionMappingList;
    private Dictionary<MissionsEnum, MissionMapping> missionMapping;

    private HashSet<MissionsEnum> activeMissions;
    private List<GameObject> camerasToCleanUp;

    private void Awake()
    {
        InitializeMissionMapping();

        activeMissions = new HashSet<MissionsEnum>();
        camerasToCleanUp = new List<GameObject>();
    }

    private void InitializeMissionMapping()
    {
        missionMapping = new Dictionary<MissionsEnum, MissionMapping>();
        missionMappingList.ForEach(m =>
        {
            missionMapping.Add(m.missionType, m);
        });
        if (missionMapping.Count != System.Enum.GetNames(typeof(MissionsEnum)).Length)
        {
            Utils.LogErrorAndStopPlayMode("Expected the number of missions in MissionManager to be equal to the number in MissionsEnum!");
        }
    }

    public AMission GetMissionLogic(MissionsEnum missionEnumValue)
    {
        if (missionMapping.TryGetValue(missionEnumValue, out MissionMapping mapping))
        {
            return mapping.mission;
        }
        else
        {
            Debug.LogWarning($"Unmapped MissionsEnum value: {missionEnumValue} passed into GetMissionLogic!");
            return null;
        }
    }

    public GameObject GetMissionFromEnum(MissionsEnum missionEnumValue)
    {
        if (missionMapping.TryGetValue(missionEnumValue, out MissionMapping mapping))
        {
            return mapping.mission.gameObject;
        }
        else
        {
            Debug.LogWarning($"Unmapped MissionsEnum value: {missionEnumValue} passed into GetMissionFromEnum!");
            return null;
        }
    }

    public void SetObjectiveTextForList(MissionsEnum missionEnumValue)
    {
        if (missionMapping[missionEnumValue].objective) 
        {
            ObjectiveList.Instance.DisplayObjectiveText(missionMapping[missionEnumValue].objective.line);
            ObjectiveList.Instance.SlideOutObjectTextForSeconds(5f);
        }
    }

    public StampCollectible GetStampCollectibleFromEnum(MissionsEnum missionEnumValue)
    {
        if (missionMapping.TryGetValue(missionEnumValue, out MissionMapping mission))
        {
            return mission.collectible;
        }
        else
        {
            Debug.LogWarning($"Unmapped MissionsEnum value: {missionEnumValue} passed into GetMissionFromEnum!");
            return null;
        }
    }

    public AMission StartMission(MissionsEnum missionEnumValue)
    {
        AMission missionLogic = GetMissionLogic(missionEnumValue);
        missionLogic.gameObject.SetActive(true);

        ProgressManager.Instance.AddMission(missionLogic);
        activeMissions.Add(missionEnumValue);

        SetObjectiveTextForList(missionEnumValue);
        
        return missionLogic;
    }

    public bool IsMissionActive(MissionsEnum missionEnumValue)
    {
        return activeMissions.Contains(missionEnumValue);
    }

    public void RestartMission(MissionsEnum missionEnumValue)
    {
        ProgressManager.Instance.UpdateMissionStatus(GetMissionLogic(missionEnumValue), MissionStatusCode.Started);

        SetObjectiveTextForList(missionEnumValue);
    }

    public void CompleteMissionObjective(MissionsEnum missionEnumValue)
    {
        ProgressManager.Instance.UpdateMissionStatus(GetMissionLogic(missionEnumValue), MissionStatusCode.Completed);
        ObjectiveList.Instance.CrossOutObjectiveText();
        Debug.Log("Objective Complete");
    }

    public void EndMission(MissionsEnum missionEnumValue)
    {
        AMission missionLogic = GetMissionLogic(missionEnumValue);

        ObjectiveList.Instance.HideObjectiveList();

        if (activeMissions.Contains(missionEnumValue))
        {
            activeMissions.Remove(missionEnumValue);

            missionLogic.gameObject.SetActive(false);
            ProgressManager.Instance.UpdateMissionStatus(missionLogic, MissionStatusCode.Closed);

            StampCollectible collectible = GetStampCollectibleFromEnum(missionEnumValue);
            if (collectible != null)
            {
                ProgressManager.Instance.UnlockStampCollectible(collectible);
            }
        }
        else
        {
            Debug.LogError($"Tried to end an invalid mission: {missionEnumValue}");
        }
    }

    public GameObject SpawnMissionObject(MissionObject missionObject)
    {
        return Instantiate(missionObject.prefab, missionObject.position, Quaternion.Euler(missionObject.rotation));
    }

    public void DestroyMissionObject(MissionObject missionObject)
    {
        if (missionObject.spawnedInstance)
        {
            Destroy(missionObject.spawnedInstance);
        }
    }

    public void DestroyMissionObjects(List<MissionObject> missionObjects)
    {
        missionObjects.ForEach(o =>
        {
            DestroyMissionObject(o);
        });
    }

    public List<BasicNurse> SpawnEnemyNurses(List<MissionEnemy> enemies, System.Action onCollideAction)
    {
        List<BasicNurse> instantiatedEnemies = new List<BasicNurse>();

        // Instantiate all enemies
        enemies.ForEach(enemy =>
        {
            // We can only spawn a NavMeshAgent on a position close enough to a NavMesh, so we must sample the inputted position first just in case.
            if (NavMesh.SamplePosition(enemy.spawnPosition, out NavMeshHit closestNavmeshHit, 10.0f, NavMesh.AllAreas))
            {
                GameObject spawnedEnemy = Instantiate(enemy.prefab, closestNavmeshHit.position, Quaternion.Euler(enemy.spawnRotation));

                // All enemies will be chasers, so we need to set the target transform for all.
                BasicNurse enemyComponent = Utils.GetRequiredComponent<BasicNurse>(spawnedEnemy, $"Enemy does not have a BasicNurse component!");
                enemyComponent.enemy.OnCollideWithPlayer += onCollideAction;

                enemyComponent.patroller.SetPoints(enemy.waypoints);
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

        return instantiatedEnemies;
    }

    public void DestroyEnemyNurses(List<BasicNurse> nurses)
    {
        nurses.Where(e => e).Select(e => e.gameObject).ToList().ForEach(o =>
        {
            if (o)
            {
                Destroy(o);
            }
        });
    }

    public IEnumerator PlayCutscenePart(CinemachineVirtualCamera startCamera, GameObject cameraPrefab, GameObject cutsceneTextPrefab, Transform focusTransform, bool doHardBlend = false)
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

    public IEnumerator PlayMovingCameraCutscene(GameObject cameraPrefab, Transform focusTransform, float extraEndWaitMultiplier = 1f, bool fadeBack = false)
    {
        if (GameManager.Instance.skipSettings.allRealtimeCutscenes) yield break;

        CinemachineVirtualCamera currentCamera = CameraManager.Instance.GetActiveVirtualCamera();

        GameObject cutsceneCameraInstance = CameraManager.Instance.SpawnCameraFromPrefab(cameraPrefab);
        CinemachineVirtualCamera virtualCamera = cutsceneCameraInstance.GetComponentInChildren<CinemachineVirtualCamera>();
        Animator virtualCameraAnim = cutsceneCameraInstance.GetComponentInChildren<Animator>();

        if (!virtualCamera || !virtualCameraAnim)
        {
            Destroy(cutsceneCameraInstance);
            yield break;
        }

        virtualCamera.LookAt = focusTransform;
        CameraManager.Instance.BlendTo(virtualCamera, alertGlobally: false);

        yield return new WaitForSeconds(virtualCameraAnim.GetCurrentAnimatorStateInfo(0).length * extraEndWaitMultiplier);

        if (fadeBack)
        {
            UIManager.Instance.FadeOut();
            yield return new WaitForSeconds(UIManager.Instance.fadeSpeed);
            Destroy(cutsceneCameraInstance);
            UIManager.Instance.FadeIn();
        }
        else
        {
            Destroy(cutsceneCameraInstance);
        }

        CameraManager.Instance.BlendTo(currentCamera, alertGlobally: false);
    }

    public IEnumerator PlayCutsceneText(GameObject cutsceneTextPrefab)
    {
        if (GameManager.Instance.skipSettings.allRealtimeCutscenes) yield break;

        GameObject instantiatedCutsceneText = Instantiate(cutsceneTextPrefab, UIManager.Instance.mainUICanvas.transform);
        instantiatedCutsceneText.transform.SetAsFirstSibling();  // this way, it will be covered by all other UI elements, like the pause menu
        Animator textAnimator = Utils.GetRequiredComponentInChildren<Animator>(instantiatedCutsceneText);
        float textAnimationLength = textAnimator.GetCurrentAnimatorStateInfo(0).length;

        // wait the appropriate amount of scaled seconds according to the update type of the animation we have
        if (textAnimator.updateMode == AnimatorUpdateMode.UnscaledTime)
        {
            yield return new WaitForSecondsRealtime(textAnimationLength);
        }
        else
        {
            yield return new WaitForSeconds(textAnimationLength);
        }
        Destroy(instantiatedCutsceneText);
    }

    private void CleanupCamerasAfterBlending(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera)
    {
        CameraManager.Instance.OnBlendingComplete -= CleanupCamerasAfterBlending;
        camerasToCleanUp.ForEach(c =>
        {
            Destroy(c);
        });
    }

    public IEnumerator DisablePlayerMovementDuringCutscene(PlayableDirector cutsceneDirector)
    {
        if (GameManager.Instance.skipSettings.allRealtimeCutscenes) yield break;

        cutsceneDirector.Play();
        GameManager.Instance.GetPlayerController().EnablePlayerInput = false;
        while (cutsceneDirector.state == PlayState.Playing)
        {
            yield return null;
        }
        GameManager.Instance.GetPlayerController().EnablePlayerInput = true;
    }
}
