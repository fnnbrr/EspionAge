using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;

public class MenuManager : MonoBehaviour
{
    public Light mainLight;
    
    [Header("Image Darkening")]
    public Color imageDarkenedColor;
    public List<Graphic> imagesToDarknen;
    private List<Color> imagesOriginalColor = new List<Color>();

    [Header("Camera Switching")]
    public List<CinemachineVirtualCamera> onPressPlayCameras;

    private bool isStarting;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void HandlePressStart()
    {
        isStarting = true;

        if (onPressPlayCameras.Count > 0)
        {
            CameraManager.Instance.OnBlendingComplete += HandlePressStartCameraBlendingComplete;
            CameraManager.Instance.BlendTo(onPressPlayCameras[0]);
            onPressPlayCameras.RemoveAt(0);
        } 
        else
        {
            StartMainScene();
        }
    }

    private void HandlePressStartCameraBlendingComplete(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera)
    {
        CameraManager.Instance.OnBlendingComplete -= HandlePressStartCameraBlendingComplete;
        HandlePressStart();
    }

    void StartMainScene()
    {
        SceneManager.LoadScene(Constants.SCENE_MAIN);
    }

    public void OnStartButtonEnter()
    {
        if (!isStarting)
        {
            mainLight.enabled = false;

            imagesOriginalColor.Clear();
            imagesToDarknen.ForEach(i =>
            {
                imagesOriginalColor.Add(i.color);
                i.color = imageDarkenedColor;
            });
        }
    }

    public void OnStartButtonExit()
    {
        if (!isStarting)
        {
            mainLight.enabled = true;

            imagesToDarknen.ForEach(i => {
                i.color = imagesOriginalColor[imagesToDarknen.IndexOf(i)];
            });
        }
    }
}
