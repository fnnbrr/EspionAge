using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFader : MonoBehaviour
{
    public float fadeSpeed;

    private Renderer r;
    private Dictionary<Material, MaterialColorSettings> defaultTransparentMapping;

    public delegate void FadingComplete();
    public event FadingComplete OnFadingComplete;

    private const string renderTagRefString = "RenderType";
    private const string renderTagTransparentRefString = "Transparent";
    private const string baseColorRefString = "_BaseColor";

    private class MaterialColorSettings
    {
        public bool isTransparent;
        public float defaultAlpha;

        public MaterialColorSettings(bool _isTransparent, float _defaultAlpha)
        {
            isTransparent = _isTransparent;
            defaultAlpha = _defaultAlpha;
        }
    }

    private void Awake()
    {
        r = Utils.GetRequiredComponent<Renderer>(this);

        defaultTransparentMapping = new Dictionary<Material, MaterialColorSettings>();
        foreach (Material material in r.materials)
        {
            defaultTransparentMapping.Add(
                material, 
                new MaterialColorSettings(
                    material.GetTag(renderTagRefString, true, "Nothing") == renderTagTransparentRefString, 
                    GetBaseColor(material).a));
        }
    }

    private Color GetBaseColor(Material material)
    {
        return material.GetColor(baseColorRefString);
    }

    public void FadeIn()
    {
        StartCoroutine(FadeToOpaque());
    }

    public void FadeOut()
    {
        StartCoroutine(FadeToTransparent());
    }

    // Code from here was changed for our own uses: 
    //  https://forum.unity.com/threads/how-to-write-a-general-shader-code-for-different-render-type-like-standard-shader.685960/
    public void SetMaterialsTransparent()
    {
        foreach (Material material in r.materials)
        {
            material.SetOverrideTag(renderTagRefString, renderTagTransparentRefString);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
    }

    public void SetMaterialsOpaque()
    {
        foreach (Material material in r.materials)
        {
            // Keep materials that were already transparent, as transparent rather than changing them to opaque
            if (defaultTransparentMapping.TryGetValue(material, out MaterialColorSettings transparentSettings) && transparentSettings.isTransparent)
            {
                material.SetOverrideTag(renderTagRefString, renderTagTransparentRefString);
            }
            else
            {
                material.SetOverrideTag("RenderType", "Opaque");
            }
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.DisableKeyword("_ALPHABLEND_ON");
            material.renderQueue = -1;
        }
    }

    private IEnumerator FadeToOpaque()
    {
        SetMaterialsTransparent();

        float currentAlphaPercentage = 0f;
        while (currentAlphaPercentage <= 1f)
        {
            SetAllMaterialsLerpedTransparency(currentAlphaPercentage);

            currentAlphaPercentage += fadeSpeed * Time.deltaTime;
            yield return null;
        }

        SetAllMaterialsLerpedTransparency(1f); // we want to make sure we actually set it to the final color in the end (fading may be off by a little bit)

        SetMaterialsOpaque();
    }

    private IEnumerator FadeToTransparent()
    {
        SetMaterialsTransparent();

        float currentAlphaPercentage = 1f;
        while (currentAlphaPercentage >= 0f)
        {
            SetAllMaterialsLerpedTransparency(currentAlphaPercentage);

            currentAlphaPercentage -= fadeSpeed * Time.deltaTime;
            yield return null;
        }
        SetAllMaterialsLerpedTransparency(0f); // we want to make sure we actually set it to 0 in the end (fading may be off by a little bit)
    }

    private void SetAllMaterialsLerpedTransparency(float lerpPercentage)
    {
        foreach (Material material in r.materials)
        {
            Color currentMaterialColor = GetBaseColor(material);

            if (defaultTransparentMapping.TryGetValue(material, out MaterialColorSettings transparentMaterialSettings))
            {
                material.SetColor(baseColorRefString, new Color(currentMaterialColor.r, currentMaterialColor.g, currentMaterialColor.b, Mathf.Lerp(0f, transparentMaterialSettings.defaultAlpha, lerpPercentage)));
            }
        }
    }
}
