using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFader : MonoBehaviour
{
    public float fadeSpeed;

    private Renderer[] renderers;
    private Dictionary<Material, MaterialColorSettings> defaultTransparentMapping;

    public delegate void FadingComplete();
    public event FadingComplete OnFadeToOpaqueComplete;
    public event FadingComplete OnFadeToTransparentComplete;

    private const string renderTagRefString = "RenderType";
    private const string renderTagTransparentRefString = "Transparent";
    private const string baseColorRefString = "_BaseColor";

    // these two enums are used according to this: 
    //  https://answers.unity.com/questions/1608815/change-surface-type-with-lwrp.html
    public enum SurfaceType
    {
        Opaque,
        Transparent
    }

    public enum BlendMode
    {
        Alpha,
        Premultiply,
        Additive,
        Multiply
    }

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
        renderers = GetComponentsInChildren<Renderer>();

        defaultTransparentMapping = new Dictionary<Material, MaterialColorSettings>();
        foreach (Renderer r in renderers)
        {
            foreach (Material material in r.materials)
            {
                defaultTransparentMapping.Add(
                    material,
                    new MaterialColorSettings(
                        material.GetTag(renderTagRefString, true, "Nothing") == renderTagTransparentRefString,
                        GetBaseColor(material).a));
            }
        }
    }

    private Color GetBaseColor(Material material)
    {
        return material.GetColor(baseColorRefString);
    }

    public void FadeToOpaque()
    {
        StartCoroutine(FadeToOpaqueCoroutine());
    }

    public void FadeToTransparent()
    {
        StartCoroutine(FadeToTransparentCoroutine());
    }

    // Code from here was changed for our own uses: 
    //  https://forum.unity.com/threads/how-to-write-a-general-shader-code-for-different-render-type-like-standard-shader.685960/
    //  https://www.youtube.com/watch?v=nNjNWDZSkAI
    public void SetMaterialsTransparent()
    {
        foreach (Renderer r in renderers)
        {
            foreach (Material material in r.materials)
            {
                material.SetFloat("_Surface", (float)SurfaceType.Transparent);
                material.SetFloat("_Blend", (float)BlendMode.Alpha);
                SetupMaterialBlendMode(material);
            }
        }
    }

    public void SetMaterialsOpaque()
    {
        foreach (Renderer r in renderers)
        {
            foreach (Material material in r.materials)
            {
                // Keep materials that were already transparent, as transparent rather than changing them to opaque
                if (defaultTransparentMapping.TryGetValue(material, out MaterialColorSettings transparentSettings) && transparentSettings.isTransparent)
                {
                    material.SetFloat("_Surface", (float)SurfaceType.Transparent);
                }
                else
                {
                    material.SetFloat("_Surface", (float)SurfaceType.Opaque);
                }
                SetupMaterialBlendMode(material);
            }
        }
    }

    private IEnumerator FadeToOpaqueCoroutine()
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

        OnFadeToOpaqueComplete?.Invoke();
    }

    private IEnumerator FadeToTransparentCoroutine()
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
        
        OnFadeToTransparentComplete?.Invoke();
    }

    private void SetAllMaterialsLerpedTransparency(float lerpPercentage)
    {
        foreach (Renderer r in renderers)
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

    void SetupMaterialBlendMode(Material material)
    {
        if (material == null)
        {
            throw new System.ArgumentNullException("material");
        }
        bool alphaClip = material.GetFloat("_AlphaClip") == 1;
        if (alphaClip)
        {
            material.EnableKeyword("_ALPHATEST_ON");
        }
        else
        {
            material.DisableKeyword("_ALPHATEST_ON");
        }
        SurfaceType surfaceType = (SurfaceType)material.GetFloat("_Surface");
        if (surfaceType == SurfaceType.Opaque)
        {
            material.SetOverrideTag("RenderType", "");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = -1;
            material.SetShaderPassEnabled("ShadowCaster", true);
        }
        else
        {
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetShaderPassEnabled("ShadowCaster", false);

            BlendMode blendMode = (BlendMode)material.GetFloat("_Blend");
            switch (blendMode)
            {
                case BlendMode.Alpha:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    break;
                case BlendMode.Premultiply:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    break;
                case BlendMode.Additive:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    break;
                case BlendMode.Multiply:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    break;
            }
        }
    }
}
