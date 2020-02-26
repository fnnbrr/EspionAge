using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;

public class CameraZone : MonoBehaviour 
{
    public CinemachineVirtualCamera mainCamera;
    public bool isRestricted;
    public bool isFirst;
    
    [Header("Fog Particles")]
    public List<ParticleSystem> fogs;
    public float enabledFogAlpha = 50f / 255f;
    public float disabledFogAlpha = 0f;
    public float fadeSpeed = 0.1f;

    public delegate void EnterAction();
    public delegate void ExitAction();
    public event EnterAction OnPlayerEnter;
    public event ExitAction OnPlayerExit;

    private void Start()
    {
        fogs.ForEach(fog =>
        {
            fog.gameObject.SetActive(GameManager.Instance.enableFog);
        });
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            if (isFirst && isRestricted) //to be removed when alex finds where he calls it
            {
                UIManager.Instance.staminaBar.Glow();
            }
            if (isRestricted)
            {
                UIManager.Instance.staminaBar.FadeIn();
            } 
            else
            {
                UIManager.Instance.staminaBar.FadeOut();
            }
            // so if there is no mainCamera set, we will just keep whatever the current camera is (which may not be preferable in most cases)
            if (mainCamera)
            {
                CameraManager.Instance.OnBlendingComplete += AlertBlendingComplete;
                CameraManager.Instance.BlendTo(mainCamera);
            }

            if (GameManager.Instance.enableFog)
            {
                StartCoroutine(FadeFog(enabledFogAlpha, disabledFogAlpha));
            }
        }
    }

    private void AlertBlendingComplete(CinemachineVirtualCamera fromCamera, CinemachineVirtualCamera toCamera)
    {
        if (toCamera == mainCamera)  // we do not want to care about unrelated camera blending events
        {
            CameraManager.Instance.OnBlendingComplete -= AlertBlendingComplete;
            OnPlayerEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            OnPlayerExit?.Invoke();

            if (GameManager.Instance.enableFog)
            {
                StartCoroutine(FadeFog(disabledFogAlpha, enabledFogAlpha));
            }
        }
    }

    private IEnumerator FadeFog(float startAlpha, float endAlpha)
    {
        IEnumerable<Color> startColors = fogs.Select(fog =>
            new Color(fog.main.startColor.color.r, fog.main.startColor.color.g, fog.main.startColor.color.b, startAlpha));
        IEnumerable<Color> endColors = fogs.Select(fog => 
            new Color(fog.main.startColor.color.r, fog.main.startColor.color.g, fog.main.startColor.color.b, endAlpha));

        float t = 0f;
        while (t < 1f)
        {
            t += fadeSpeed;
            
            for (int i = 0; i < fogs.Count; i++)
            {
                var main = fogs[i].main;
                main.startColor = Color.Lerp(startColors.ElementAt(i), endColors.ElementAt(i), t);

            }

            yield return null;
        }
    }
}
