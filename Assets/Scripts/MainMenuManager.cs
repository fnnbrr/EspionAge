using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;

public class MainMenuManager : MonoBehaviour
{
    public Light mainLight;
    public Renderer lightBulbRenderer;

    [Header("Input")]
    public float axisInputBetweenDelay;
    private float nextAxisInputTime;
    
    [Header("Image Darkening")]
    public Color imageDarkenedColor;
    public List<ButtonData> buttonData;
    private Dictionary<ButtonType, ButtonData> buttonDataMappings;

    [Header("Camera Switching")]
    public List<CinemachineVirtualCamera> onPressPlayCameras;

    private Material lightBulbMaterial;
    private Color lightBulbStartColor;
    private bool isStarting;
    private bool stateUpdated;

    public enum ButtonType
    {
        None,
        Start,
        Quit
    }
    private ButtonType currentButton;
    private ButtonType previousButton;

    private enum DirectionalMovement
    {
        Any,
        Up,
        Down,
        Left,
        Right
    }

    private readonly Dictionary<Tuple<ButtonType, DirectionalMovement>, ButtonType> buttonMappings = new Dictionary<Tuple<ButtonType, DirectionalMovement>, ButtonType>()
    {
        { new Tuple<ButtonType, DirectionalMovement>(ButtonType.None, DirectionalMovement.Left), ButtonType.Start },
        { new Tuple<ButtonType, DirectionalMovement>(ButtonType.None, DirectionalMovement.Right), ButtonType.Quit },
        { new Tuple<ButtonType, DirectionalMovement>(ButtonType.Start, DirectionalMovement.Right), ButtonType.Quit },
        { new Tuple<ButtonType, DirectionalMovement>(ButtonType.Quit, DirectionalMovement.Left), ButtonType.Start },
        { new Tuple<ButtonType, DirectionalMovement>(ButtonType.None, DirectionalMovement.Any), ButtonType.Start }  // catch all
    };

    [Serializable]
    public class ButtonData
    {
        [Header("This enum must be unique among the list!")]
        public ButtonType buttonType;
        public Button buttonComponent;
        public List<Graphic> imagesToDarknen;

        [HideInInspector] public Animator animator;
        [HideInInspector] public List<Color> originalColors = new List<Color>();
    }

    private void Start()
    {
        previousButton = ButtonType.None;
        currentButton = ButtonType.None;
        stateUpdated = true;

        lightBulbMaterial = lightBulbRenderer.material;
        lightBulbStartColor = lightBulbMaterial.GetColor("_EmissionColor");

        buttonDataMappings = new Dictionary<ButtonType, ButtonData>();
        foreach (ButtonData b in buttonData)
        {
            b.originalColors = b.imagesToDarknen.Select(i => i.color).ToList();
            b.animator = b.buttonComponent.GetComponent<Animator>();
            buttonDataMappings.Add(b.buttonType, b);
        }
    }

    private void Update()
    {
        if (!isStarting)
        {
            HandleInput();

            if (stateUpdated && currentButton != previousButton)
            {
                Cleanup(previousButton);
                Setup(currentButton);

                stateUpdated = false;
            }
        }
    }

    private void HandleInput()
    {
        HandleAxisInput();
        HandleButtonInput();
    }

    private void HandleAxisInput()
    {
        // Early exit to limit how fast a user can move between buttons
        if (nextAxisInputTime > Time.time) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool hasInput = hasHorizontalInput || hasVerticalInput;
        if (!hasInput) return;  // if no input, let's just do an early exit

        bool mostlyHorizontal = Mathf.Abs(horizontal) > Mathf.Abs(vertical);

        if (mostlyHorizontal)
        {
            Tuple<ButtonType, DirectionalMovement> currentMovement;
            if (hasHorizontalInput)
            {
                if (horizontal > 0f)
                {
                    currentMovement = GetCurrentMovementPair(DirectionalMovement.Right);
                }
                else
                {
                    currentMovement = GetCurrentMovementPair(DirectionalMovement.Left);
                }
            }
            else // hasVerticalInput
            {
                if (vertical > 0f)
                {
                    currentMovement = GetCurrentMovementPair(DirectionalMovement.Up);
                }
                else
                {
                    currentMovement = GetCurrentMovementPair(DirectionalMovement.Down);
                }
            }

            if (buttonMappings.ContainsKey(currentMovement))
            {
                UpdateMainMenuState(buttonMappings[currentMovement]);
            }
            else 
            {
                currentMovement = GetCurrentMovementPair(DirectionalMovement.Any);
                if (buttonMappings.ContainsKey(currentMovement))
                {
                    UpdateMainMenuState(buttonMappings[currentMovement]);
                }
            }
        }
    }
    private void HandleButtonInput()
    {
        if (Input.GetButtonDown(Constants.INPUT_INTERACTABLE_GETDOWN))
        {
            if (buttonDataMappings.ContainsKey(currentButton))
            {
                buttonDataMappings[currentButton].buttonComponent.onClick.Invoke();
            }
        }
    }

    private Tuple<ButtonType, DirectionalMovement> GetCurrentMovementPair(DirectionalMovement directionalMovement)
    {
        return new Tuple<ButtonType, DirectionalMovement>(currentButton, directionalMovement);
    }

    private void UpdateMainMenuState(ButtonType newState)
    {
        if (currentButton != newState)
        {
            previousButton = currentButton;
            currentButton = newState;

            stateUpdated = true;

            nextAxisInputTime = Time.time + axisInputBetweenDelay;
        }
    }

    private void Setup(ButtonType mainMenuButton)
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
            case ButtonType.Start:
                mainLight.enabled = false;
                break;
            case ButtonType.Quit:
                lightBulbMaterial.SetColor("_EmissionColor", Color.red);
                break;
            default:
                break;
        }
    }

    private void Cleanup(ButtonType mainMenuButton)
    {
        // Here, we want None to mean everything must be cleaned up
        if (mainMenuButton == ButtonType.Start || mainMenuButton == ButtonType.None)
        {
            GenericCleanup(ButtonType.Start);

            mainLight.enabled = true;
        } 
        else if (mainMenuButton == ButtonType.Quit || mainMenuButton == ButtonType.None)
        {
            GenericCleanup(ButtonType.Quit);

            lightBulbMaterial.SetColor("_EmissionColor", lightBulbStartColor);
        }
    }

    private void GenericCleanup(ButtonType mainMenuButton)
    {
        ResetDarkenedColors(mainMenuButton);

        if (buttonDataMappings[mainMenuButton].animator)
        {
            buttonDataMappings[mainMenuButton].animator.SetBool("Start", false);
        }
    }

    private void ResetDarkenedColors(ButtonType mainMenuButton)
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
        UpdateMainMenuState(ButtonType.Start);
    }

    public void OnStartButtonExit()
    {
        UpdateMainMenuState(ButtonType.None);
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
        UpdateMainMenuState(ButtonType.Quit);
    }

    public void OnQuitButtonExit()
    {
        UpdateMainMenuState(ButtonType.None);
    }
}
