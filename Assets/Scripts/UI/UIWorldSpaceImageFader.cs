using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWorldSpaceImageFader : MonoBehaviour
{
    public float fadeInRadius;
    public float fadeInSeconds;
    [Range(0f, 1f)] public float goalAlpha;

    private Graphic[] graphics;
    private bool fadedIn = false;
    private bool fadedOut = true;

    private Coroutine currentCoroutine;

    void Start()
    {
        graphics = GetComponentsInChildren<Graphic>();
        fadedOut = true;
        SetAlpha(0f);
    }

    void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, GameManager.Instance.GetPlayerTransform().position) < fadeInRadius)
        {
            if (!fadedIn)
            {
                if (currentCoroutine != null) StopCoroutine(currentCoroutine);

                fadedIn = true;
                fadedOut = false;
                currentCoroutine = StartCoroutine(FadeTowards(goalAlpha));
            }
        } 
        else
        {
            if (!fadedOut)
            {
                if (currentCoroutine != null) StopCoroutine(currentCoroutine);

                fadedIn = false;
                fadedOut = true;
                currentCoroutine = StartCoroutine(FadeTowards(0f));
            }
        }
    }

    private IEnumerator FadeTowards(float endAlpha)
    {
        if (graphics.Length == 0) yield break;

        float startAlpha = graphics[0].color.a;
        float currentAlpha = startAlpha;
        while (Mathf.Abs(currentAlpha - startAlpha) < Mathf.Abs(startAlpha - endAlpha))
        {
            foreach (Graphic graphic in graphics)
            {
                graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, currentAlpha);
                currentAlpha += fadeInSeconds * Time.deltaTime * Mathf.Sign(endAlpha - startAlpha);
            }
            yield return null;
        }

        SetAlpha(endAlpha);
    }

    private void SetAlpha(float alpha)
    {
        foreach (Graphic graphic in graphics)
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, fadeInRadius);
    }
}
