using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public enum UIDirectionalMovement
{
    Any,
    Up,
    Down,
    Left,
    Right
}

[Serializable]
public class UIButtonDataGeneric<T>
{
    [Header("This enum must be unique among the list!")]
    public T buttonType;
    public Button buttonComponent;
    public List<Graphic> imagesToDarknen;

    [HideInInspector] public Animator animator;
    [HideInInspector] public List<Color> originalColors = new List<Color>();
}

public class UIMenuStatic<T> : MonoBehaviour where T : Enum
{
    // TO BE OVERIDDEN
    public UIButtonDataGeneric<T>[] buttonData;
    protected Dictionary<T, UIButtonDataGeneric<T>> buttonDataMappings;

    [Header("UIMenuStatic Generic Fields")]
    public float axisInputBetweenDelay;
    private float nextAxisInputTime;

    // Internal State Variables
    protected T currentButton;
    protected T previousButton;
    private bool stateUpdated = true;

    [Space(10)]
    protected Dictionary<Tuple<T, UIDirectionalMovement>, T> buttonMappings;

    protected virtual void Awake()
    {
        if (!typeof(T).IsEnum)
        {
            Utils.LogErrorAndStopPlayMode($"{name}: T must be an enumerated type");
        }
    }

    protected void InitializeButtonDataMappings(UIButtonDataGeneric<T>[] initializeWith)
    {
        buttonDataMappings = new Dictionary<T, UIButtonDataGeneric<T>>();
        foreach (UIButtonDataGeneric<T> b in initializeWith)
        {
            b.originalColors = b.imagesToDarknen.Select(i => i.color).ToList();
            b.animator = b.buttonComponent.GetComponent<Animator>();
            buttonDataMappings.Add(b.buttonType, b);
        }
    }

    protected void UpdateMainMenuState(T newState)
    {
        if (!EqualityComparer<T>.Default.Equals(currentButton, newState))
        {
            previousButton = currentButton;
            currentButton = newState;

            stateUpdated = true;

            nextAxisInputTime = Time.time + axisInputBetweenDelay;
        }
    }

    protected virtual bool ShouldUpdate()
    {
        return true;
    }

    private void Update()
    {
        if (ShouldUpdate())
        {
            HandleInput();

            if (stateUpdated && !EqualityComparer<T>.Default.Equals(currentButton, previousButton))
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
            Tuple<T, UIDirectionalMovement> currentMovement;
            if (hasHorizontalInput)
            {
                if (horizontal > 0f)
                {
                    currentMovement = GetCurrentMovementPair(UIDirectionalMovement.Right);
                }
                else
                {
                    currentMovement = GetCurrentMovementPair(UIDirectionalMovement.Left);
                }
            }
            else // hasVerticalInput
            {
                if (vertical > 0f)
                {
                    currentMovement = GetCurrentMovementPair(UIDirectionalMovement.Up);
                }
                else
                {
                    currentMovement = GetCurrentMovementPair(UIDirectionalMovement.Down);
                }
            }

            if (buttonMappings.ContainsKey(currentMovement))
            {
                UpdateMainMenuState(buttonMappings[currentMovement]);
            }
            else
            {
                currentMovement = GetCurrentMovementPair(UIDirectionalMovement.Any);
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

    private Tuple<T, UIDirectionalMovement> GetCurrentMovementPair(UIDirectionalMovement directionalMovement)
    {
        return new Tuple<T, UIDirectionalMovement>(currentButton, directionalMovement);
    }

    protected virtual void Cleanup(T button)
    {
        throw new NotImplementedException();
    }

    protected virtual void Setup(T button)
    {
        throw new NotImplementedException();
    }
}
