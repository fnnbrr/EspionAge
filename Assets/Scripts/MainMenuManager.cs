using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;

public enum EMainMenuButton
{
    None,
    Start,
    Quit
}

// Necessary to make this wrapper according to: https://answers.unity.com/questions/214300/serializable-class-using-generics.html
[Serializable]
public class UIMainMenuButtonData : UIButtonDataGeneric<EMainMenuButton> { }

public class MainMenuManager : UIMenuStatic<EMainMenuButton>
{
    [Header("MainMenuManager Fields")]
    public new UIMainMenuButtonData[] buttonData;  // REQUIRED OVERRIDE HERE

    public Light mainLight;
    public Renderer lightBulbRenderer;
    
    [Header("Image Darkening")]
    public Color imageDarkenedColor;

    [Header("Camera Switching")]
    public List<CinemachineVirtualCamera> onPressPlayCameras;

    private Material lightBulbMaterial;
    private Color lightBulbStartColor;
    private bool isStarting;

    [Serializable]
    public class ButtonData
    {
        [Header("This enum must be unique among the list!")]
        public EMainMenuButton buttonType;
        public Button buttonComponent;
        public List<Graphic> imagesToDarknen;

        [HideInInspector] public Animator animator;
        [HideInInspector] public List<Color> originalColors = new List<Color>();
    }

    protected override void Awake()
    {
        base.Awake();

        InitializeButtonDataMappings(buttonData);
    }

    private void Start()
    {
        buttonMappings = new Dictionary<Tuple<EMainMenuButton, UIDirectionalMovement>, EMainMenuButton>()
        {
            { new Tuple<EMainMenuButton, UIDirectionalMovement>(EMainMenuButton.None, UIDirectionalMovement.Left), EMainMenuButton.Start },
            { new Tuple<EMainMenuButton, UIDirectionalMovement>(EMainMenuButton.None, UIDirectionalMovement.Right), EMainMenuButton.Quit },
            { new Tuple<EMainMenuButton, UIDirectionalMovement>(EMainMenuButton.Start, UIDirectionalMovement.Right), EMainMenuButton.Quit },
            { new Tuple<EMainMenuButton, UIDirectionalMovement>(EMainMenuButton.Quit, UIDirectionalMovement.Left), EMainMenuButton.Start },
            { new Tuple<EMainMenuButton, UIDirectionalMovement>(EMainMenuButton.None, UIDirectionalMovement.Any), EMainMenuButton.Start }  // catch all
        };

        previousButton = EMainMenuButton.None;
        currentButton = EMainMenuButton.None;

        lightBulbMaterial = lightBulbRenderer.material;
        lightBulbStartColor = lightBulbMaterial.GetColor("_EmissionColor");
    }

    protected override bool ShouldUpdate()
    {
        return !isStarting;
    }

    protected override void Setup(EMainMenuButton mainMenuButton)
    {
        if (buttonDataMappings.ContainsKey(mainMenuButton))
        {
            buttonDataMappings[mainMenuButton].imagesToDarknen.ForEach(i =>
            {
                i.color = imageDarkenedColor;
            });

            if (buttonDataMappings[mainMenuButton].animator)
            {
                buttonDataMappings[mainMenuButton].animator.SetBool("Start", true);
            }
        }

        switch (mainMenuButton)
        {
            case EMainMenuButton.Start:
                mainLight.enabled = false;
                break;
            case EMainMenuButton.Quit:
                lightBulbMaterial.SetColor("_EmissionColor", Color.red);
                break;
            default:
                break;
        }
    }

    protected override void Cleanup(EMainMenuButton mainMenuButton)
    {
        // Here, we want None to mean everything must be cleaned up
        if (mainMenuButton == EMainMenuButton.Start || mainMenuButton == EMainMenuButton.None)
        {
            GenericCleanup(EMainMenuButton.Start);

            mainLight.enabled = true;
        } 
        else if (mainMenuButton == EMainMenuButton.Quit || mainMenuButton == EMainMenuButton.None)
        {
            GenericCleanup(EMainMenuButton.Quit);

            lightBulbMaterial.SetColor("_EmissionColor", lightBulbStartColor);
        }
    }

    private void GenericCleanup(EMainMenuButton mainMenuButton)
    {
        ResetDarkenedColors(mainMenuButton);

        if (buttonDataMappings[mainMenuButton].animator)
        {
            buttonDataMappings[mainMenuButton].animator.SetBool("Start", false);
        }
    }

    private void ResetDarkenedColors(EMainMenuButton mainMenuButton)
    {
        if (buttonDataMappings.ContainsKey(mainMenuButton))
        {
            for(int i = 0; i < buttonDataMappings[mainMenuButton].imagesToDarknen.Count; i++)
            {
                buttonDataMappings[mainMenuButton].imagesToDarknen[i].color = buttonDataMappings[mainMenuButton].originalColors[i];
            }
        }
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
        UpdateMainMenuState(EMainMenuButton.Start);
    }

    public void OnStartButtonExit()
    {
        UpdateMainMenuState(EMainMenuButton.None);
    }

    public void HandlePressQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnQuitButtonEnter()
    {
        UpdateMainMenuState(EMainMenuButton.Quit);
    }

    public void OnQuitButtonExit()
    {
        UpdateMainMenuState(EMainMenuButton.None);
    }
}
