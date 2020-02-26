using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum EPauseMenuButton
{
    None,
    //Progress,
    Resume,
    ExitToMainMenu
}

// Necessary to make this wrapper according to: https://answers.unity.com/questions/214300/serializable-class-using-generics.html
[Serializable]
public class UIPauseMenuButtonData : UIButtonDataGeneric<EPauseMenuButton> { }

public class PauseMenuManager : UIMenuStatic<EPauseMenuButton>
{
    [Header("PauseMenuManager Fields")]
    public new UIPauseMenuButtonData[] buttonData;  // REQUIRED OVERRIDE HERE

    protected override void Awake()
    {
        base.Awake();

        InitializeButtonDataMappings(buttonData);
    }

    private void Start()
    {
        buttonMappings = new Dictionary<Tuple<EPauseMenuButton, UIDirectionalMovement>, EPauseMenuButton>()
        {
            { new Tuple<EPauseMenuButton, UIDirectionalMovement>(EPauseMenuButton.None, UIDirectionalMovement.Any), EPauseMenuButton.Resume },
            { new Tuple<EPauseMenuButton, UIDirectionalMovement>(EPauseMenuButton.Resume, UIDirectionalMovement.Down), EPauseMenuButton.ExitToMainMenu },
            { new Tuple<EPauseMenuButton, UIDirectionalMovement>(EPauseMenuButton.ExitToMainMenu, UIDirectionalMovement.Up), EPauseMenuButton.Resume }
        };

        previousButton = EPauseMenuButton.None;
        currentButton = EPauseMenuButton.Resume;
    }

    private void OnEnable()
    {
        UpdateMainMenuState(EPauseMenuButton.Resume);
    }

    private void OnDisable()
    {
        UpdateMainMenuState(EPauseMenuButton.None);
    }

    public void HandleResumeButtonPress()
    {
        UIManager.Instance.PauseGame(false);
    }

    public void HandleExitToMainMenuButtonPress()
    {
        UIManager.Instance.PauseGame(false);
        SceneManager.LoadScene(Constants.SCENE_MAINMENU);
    }

    public void OnResumeButtonEnter()
    {
        UpdateMainMenuState(EPauseMenuButton.Resume);
    }

    public void OnResumeButtonExit()
    {
        UpdateMainMenuState(EPauseMenuButton.None);
    }

    public void OnExitButtonEnter()
    {
        UpdateMainMenuState(EPauseMenuButton.ExitToMainMenu);
    }

    public void OnExitButtonExit()
    {
        UpdateMainMenuState(EPauseMenuButton.None);
    }
}
