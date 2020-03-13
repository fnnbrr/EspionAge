using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;

[System.Serializable]
public class MissionObject
{
    public GameObject prefab;
    public Vector3 position;
    public Vector3 rotation;

    [HideInInspector]
    public GameObject spawnedInstance;
}

public class InProgressMissionContainer
{
    public AMission mission;
    public GameObject gameObject;

    public InProgressMissionContainer(AMission _mission, GameObject _gameObject)
    {
        mission = _mission;
        gameObject = _gameObject;
    }
}

[System.Serializable]
public class MissionMapping
{
    [Header("Make sure this mission is unique among the list!")]
    public MissionsEnum mission;
    public GameObject prefab;
    public Objective objective;
    public StampCollectible collectible;
    [HideInInspector]
    public AMission instantiatedMission;
}

public class MissionManager : Singleton<MissionManager>
{
    [Header("This will get converted to a Dictionary at runtime.")]
    public List<MissionMapping> missionMappingList;
    private Dictionary<MissionsEnum, MissionMapping> missionMapping;

    private List<InProgressMissionContainer> activeMissions;
    private List<GameObject> camerasToCleanUp;

    private void Awake()
    {
        InitializeMissionMapping();

        activeMissions = new List<InProgressMissionContainer>();
        camerasToCleanUp = new List<GameObject>();
    }

    private void InitializeMissionMapping()
    {
        missionMapping = new Dictionary<MissionsEnum, MissionMapping>();
        missionMappingList.ForEach(m =>
        {
            missionMapping.Add(m.mission, m);
        });
        if (missionMapping.Count != System.Enum.GetNames(typeof(MissionsEnum)).Length)
        {
            Utils.LogErrorAndStopPlayMode("Expected the number of missions in MissionManager to be equal to the number in MissionsEnum!");
        }
    }

    public GameObject GetMissionPrefabFromEnum(MissionsEnum missionEnumValue)
    {
        if (missionMapping.TryGetValue(missionEnumValue, out MissionMapping mission))
        {
            return mission.prefab;
        }
        else
        {
            Debug.LogWarning($"Unmapped MissionsEnum value: {missionEnumValue} passed into GetMissionFromEnum!");
            return null;
        }
    }


    public AMission GetInstantiatedMissionFromEnum(MissionsEnum missionEnumValue)
    {
        if (missionMapping.TryGetValue(missionEnumValue, out MissionMapping mission))
        {
            return mission.instantiatedMission;
        }
        else
        {
            Debug.LogWarning($"Unmapped MissionsEnum value: {missionEnumValue} passed into GetMissionFromEnum!");
            return null;
        }
    }

    public void SetInstantiatedMissionForEnum(MissionsEnum missionEnumValue, AMission mission)
    {
        missionMapping[missionEnumValue].instantiatedMission = mission;
    }

    public void SetObjectiveTextForList(MissionsEnum missionEnumValue)
    {
        if (missionMapping[missionEnumValue].objective) 
        {
            ObjectiveList.Instance.DisplayObjectiveList();
            ObjectiveList.Instance.DisplayObjectiveText(missionMapping[missionEnumValue].objective.line);
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
        GameObject missionPrefab = GetMissionPrefabFromEnum(missionEnumValue);
        GameObject createdMission = Instantiate(missionPrefab, Vector3.zero, Quaternion.identity, transform);
        Component missionComponent = createdMission.GetComponent(typeof(AMission));

        if (missionComponent is AMission)
        {
            AMission mission = missionComponent as AMission;
            SetObjectiveTextForList(missionEnumValue);

            SetInstantiatedMissionForEnum(missionEnumValue, mission);
            ProgressManager.Instance.AddMission(mission);
            activeMissions.Add(new InProgressMissionContainer(mission, createdMission));
            return mission;
        } 
        else
        {
            Utils.LogErrorAndStopPlayMode($"Expected to find an AMission component on {missionPrefab.name}!");
            return null;
        }
    }

    public InProgressMissionContainer GetActiveMission<T>()
    {
        return activeMissions.Find(m => m.mission is T);
    }

    public void RestartMission(MissionsEnum missionEnumValue)
    {
        ProgressManager.Instance.UpdateMissionStatus(GetInstantiatedMissionFromEnum(missionEnumValue), MissionStatusCode.Started);
    }

    public void CompleteMissionObjective(MissionsEnum missionEnumValue)
    {
        ProgressManager.Instance.UpdateMissionStatus(GetInstantiatedMissionFromEnum(missionEnumValue), MissionStatusCode.Completed);
        ObjectiveList.Instance.CrossOutObjectiveText();
        Debug.Log("Objective Complete");
    }

    public void EndMission(MissionsEnum missionEnumValue)
    {
        AMission mission = GetInstantiatedMissionFromEnum(missionEnumValue);
        InProgressMissionContainer container = activeMissions.Find(m => m.mission == mission);
        ObjectiveList.Instance.HideObjectiveList();

        if (container != null)
        {
            Destroy(container.gameObject);
            activeMissions.Remove(container);
            ProgressManager.Instance.UpdateMissionStatus(mission, MissionStatusCode.Closed);

            StampCollectible collectible = GetStampCollectibleFromEnum(missionEnumValue);
            if (collectible != null)
            {
                ProgressManager.Instance.UnlockStampCollectible(collectible);
            }
        }
        else
        {
            Debug.LogError("Tried to end an invalid mission!");
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
        Animator textAnimator = Utils.GetRequiredComponentInChildren<Animator>(instantiatedCutsceneText);
        float textAnimationLength = textAnimator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSecondsRealtime(textAnimationLength);
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
}
