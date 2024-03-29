﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class UIManager : Singleton<UIManager>
{
    public Canvas mainUICanvas;
    public StaminaBar staminaBar;
    public PauseMenuManager pauseMenu;
    public UITextOverlay textOverlay;
    public GameObject pressAPrompt;
    public Animator pauseDisabledPrompt;
    public ZoneText zoneText;
    public Credits credits;

    [Header("Fading Settings")]
    public Image fader;
    public float fadeSpeed = 2f;

    private bool isPaused = false;

    private bool _canPause = true;
    public bool CanPause
    { 
        get { return _canPause; }
        set 
        {
            // should never happen, but if we set CanPause to false, but are aleady paused...
            //  this should unpause the game first so we are not stuck in the pause screen
            if (!value && isPaused)
            {
                PauseGame(false);
            }
            _canPause = value; 
        }
    }

    public delegate void FadingComplete();
    public event FadingComplete OnFadingComplete;

    private void Awake()
    {
        textOverlay.RegisterForTextCutscenes();
    }

    private void Start()
    {
        fader.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetButtonDown(Constants.INPUT_STARTBUTTON_GETDOWN))
        {
            PauseGame(!isPaused);
        }
    }

    private Color GetFaderColorWithAlpha(float alpha)
    {
        return new Color(fader.color.r, fader.color.g, fader.color.b, alpha);
    }

    public Coroutine FadeIn()
    {
        // full black --> invisible
        return StartCoroutine(FadeCoroutine(1f, 0f));
    }

    public Coroutine FadeOut()
    {
        // invisible --> full black
        return StartCoroutine(FadeCoroutine(0f, 1f));
    }

    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha)
    {
        float currentAlpha = startAlpha;
        while (Mathf.Abs(currentAlpha - startAlpha) < Mathf.Abs(startAlpha - endAlpha))
        {
            fader.color = GetFaderColorWithAlpha(currentAlpha);
            currentAlpha += fadeSpeed * Time.deltaTime * Mathf.Sign(endAlpha - startAlpha);
            yield return null;
        }

        OnFadingComplete?.Invoke();

        yield return null;
    }

    public void InstantFadeIn()
    {
        fader.color = GetFaderColorWithAlpha(0f);
    }

    public void InstantFadeOut()
    {
        fader.color = GetFaderColorWithAlpha(1f);
    }

    public bool IsGamePaused()
    {
        return isPaused;
    }

    public void PauseGame(bool toPause)
    {
        if (!CanPause)
        {
            if (toPause)
            {
                // If someone tries to pause but we cannot, always display this prompt
                pauseDisabledPrompt.SetTrigger(Constants.ANIMATION_PAUSE_DISABLED_DISPLAY);
            }
            return;
        }

        if (toPause)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
        isPaused = toPause;
        pauseMenu.gameObject.SetActive(toPause);

        // Fixes bug where player can move again after pausing and un-pausing during mid conversation
        if (DialogueManager.Instance.RestrictMoveWhenConversing()) return;
        GameManager.Instance.GetPlayerController().EnablePlayerInput = !toPause;
    }
}
