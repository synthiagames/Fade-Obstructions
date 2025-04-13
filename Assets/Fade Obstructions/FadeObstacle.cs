using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class FadeObstacle : MonoBehaviour
{
    public float fadeSpeed = 2f;
    public float transparentAlpha = 0.3f;

    private Material[] materials;
    private float[] originalAlphas;
    private Coroutine fadeCoroutine;

    private enum Pipeline
    {
        BuiltIn,
        URP,
        HDRP
    }

    private Pipeline currentPipeline;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        materials = renderer.materials;
        originalAlphas = new float[materials.Length];

        currentPipeline = DetectRenderPipeline();
        Debug.Log("Render Pipeline Detected: " + currentPipeline);

        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];
            if (mat != null)
            {
                SetMaterialToFadeMode(mat);

                Color col = GetColor(mat);
                originalAlphas[i] = col.a;

                // Start fully opaque
                col.a = 1f;
                SetColor(mat, col);
            }
        }
    }

    public void FadeOut()
    {
        StartFade(transparentAlpha);
    }

    public void FadeIn()
    {
        StartFade(1f); // We’ll restore originalAlpha inside coroutine
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeTo(targetAlpha));
    }

    private IEnumerator FadeTo(float target)
    {
        bool allDone = false;

        while (!allDone)
        {
            allDone = true;

            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                Color col = GetColor(mat);

                float finalTarget = target == 1f ? originalAlphas[i] : transparentAlpha;
                float newAlpha = Mathf.MoveTowards(col.a, finalTarget, Time.deltaTime * fadeSpeed);
                col.a = newAlpha;
                SetColor(mat, col);

                if (Mathf.Abs(newAlpha - finalTarget) > 0.01f)
                    allDone = false;
            }

            yield return null;
        }

        fadeCoroutine = null;
    }

    private Color GetColor(Material mat)
    {
        if (mat.HasProperty("_BaseColor")) return mat.GetColor("_BaseColor");
        if (mat.HasProperty("_Color")) return mat.GetColor("_Color");
        return Color.white;
    }

    private void SetColor(Material mat, Color col)
    {
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", col);
    }

    private Pipeline DetectRenderPipeline()
    {
        var pipeline = GraphicsSettings.currentRenderPipeline;
        if (pipeline == null) return Pipeline.BuiltIn;
        if (pipeline.GetType().Name.Contains("Universal")) return Pipeline.URP;
        if (pipeline.GetType().Name.Contains("HD")) return Pipeline.HDRP;
        return Pipeline.BuiltIn;
    }

    private void SetMaterialToFadeMode(Material mat)
    {
        switch (currentPipeline)
        {
            case Pipeline.BuiltIn:
                mat.SetFloat("_Mode", 2); // Fade
                mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.SetFloat("_SmoothnessTextureChannel", 1f);
                //if (mat.HasProperty("_SmoothnessTextureChannel"))
                mat.renderQueue = 3000;
                break;

            case Pipeline.URP:
                mat.SetFloat("_Surface", 1f);        // 0 = Opaque, 1 = Transparent
                mat.SetFloat("_Blend", 0f);          // 0 = Alpha, 1 = Premultiply, 2 = Additive, 3 = Multiply
                mat.SetFloat("_ZWrite", 0f);         // Don't write to Z-buffer
                mat.SetFloat("_AlphaClip", 0f);      // Disable Alpha Clipping
                mat.SetFloat("_SmoothnessSource", 1f); // Use Albedo Alpha for smoothness (optional)

                // Set render queue
                mat.renderQueue = (int)RenderQueue.Transparent;

                // Set proper shader tags and keywords
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.DisableKeyword("_SURFACE_TYPE_OPAQUE");
                break;

            case Pipeline.HDRP:
                if (mat.HasProperty("_SurfaceType")) mat.SetFloat("_SurfaceType", 1f); // Transparent
                if (mat.HasProperty("_BlendMode")) mat.SetFloat("_BlendMode", 0f); // Alpha
                mat.renderQueue = (int)RenderQueue.Transparent;
                break;
        }
    }
}