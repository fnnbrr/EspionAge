using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameManager : Singleton<MinigameManager>
{
    public SceneField[] minigameScenes;

    [Tooltip("When setting all game objects active or inactive, these root objects will be ignored.")]
    public GameObject[] persistentGameObjectRoots;

    private bool isInMinigame = false;
    private Scene currentMinigameScene;
    private float lastGainedStamina;

    // Public events that can be subscribed to
    public delegate void MinigameCompleteAction(float gainedStamina);
    public event MinigameCompleteAction OnMinigameComplete;

    public bool IsInMinigame()
    {
        return isInMinigame && currentMinigameScene.IsValid();
    }

    private SceneField GetRandomMinigame()
    {
        return minigameScenes[Random.Range(0, minigameScenes.Length)];
    }
    
    public void LoadRandomMinigame()
    {
        if (minigameScenes.Length == 0)
        {
            Debug.LogError("No scene names found, cannot load a minigame.");
            return;
        }
        if (IsInMinigame())
        {
            Debug.LogError("Already in a minigame! Cannot load another minigame.");
            return;
        }

        SceneManager.sceneLoaded += OnMinigameSceneLoaded;
        SceneManager.LoadSceneAsync(GetRandomMinigame(), LoadSceneMode.Additive);
    }

    private void OnMinigameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isInMinigame = true;
        currentMinigameScene = scene;

        SceneManager.sceneLoaded -= OnMinigameSceneLoaded;
        // Some logic to do on-load
        // Current active scene (non-minigame) can be accessed using: SceneManager.GetActiveScene()
        // Just loaded minigame scene can be accessed using: scene

        // POSSIBLE ISSUE:
        //  - if we have a root-level object we want to start off as "inactive", later "reactivating" may be an issue...
        //    - possible fix... keep track of objects that were already inactive and just make sure they are not reactivated later...?
        //    - we can also just add those specific objects in the ignore list and then they will not be an issue at all
        SetActiveAllObjectsInScene(SceneManager.GetActiveScene(), false);
    }

    public void UnloadCurrentMinigame(float gainedStamina)
    {
        if (!IsInMinigame())
        {
            Debug.LogError($"Not in minigame, cannot unload: isInMinigame={isInMinigame}, currentMinigameScene.isValid()={currentMinigameScene.IsValid()}");
            return;
        }

        lastGainedStamina = gainedStamina;

        SceneManager.sceneUnloaded += OnMinigameSceneUnloaded;
        SceneManager.UnloadSceneAsync(currentMinigameScene);
    }

    private void OnMinigameSceneUnloaded(Scene currentScene)
    {
        isInMinigame = false;

        SceneManager.sceneUnloaded -= OnMinigameSceneUnloaded;
        // Some logic to do on-load
        // Current active scene (non-minigame) can be accessed using: SceneManager.GetActiveScene()
        // Just loaded minigame scene can be accessed using: scene

        // POSSIBLE ISSUE:
        //  - if we have a root-level object we want to start off as "inactive", later "reactivating" may be an issue...
        //    - possible fix... keep track of objects that were already inactive and just make sure they are not reactivated later...?
        //    - we can also just add those specific objects in the ignore list and then they will not be an issue at all
        SetActiveAllObjectsInScene(SceneManager.GetActiveScene(), true);

        // Alert anyone waiting for the minigame to be complete
        OnMinigameComplete?.Invoke(lastGainedStamina);
    }

    private void SetActiveAllObjectsInScene(Scene scene, bool active)
    {
        scene.GetRootGameObjects().ToList().ForEach(g => {
            if (!persistentGameObjectRoots.Any(p => p == g))
            {
                g.SetActive(active);
            }
        });
    }
}
