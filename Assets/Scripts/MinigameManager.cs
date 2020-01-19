using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

public class MinigameManager : Singleton<MinigameManager>
{
    [Header("KEEP SCENE NAMES UPDATED")]
    public SceneField[] minigameScenes;

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
        else
        {
            SceneManager.sceneLoaded += OnMinigameSceneLoaded;
            SceneManager.LoadSceneAsync(GetRandomMinigame(), LoadSceneMode.Additive);
        }
    }

    private void OnMinigameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnMinigameSceneLoaded;
        // Some logic to do on-load
    }
}
