using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class CustomPostProcessing : MonoBehaviour
{
    public PostProcessingEffect[] effects;
    Shader defaultShader;
    Material defaultMat;
    List<RenderTexture> temporaryTextures = new List<RenderTexture>();
    public bool debugOceanMask;

    public event System.Action<RenderTexture> onPostProcessingComplete;
    public event System.Action<RenderTexture> onPostProcessingBegin;

    RenderTexture initialSource;
    RenderTexture finalDestination;

    void Awake()
    {
        RenderPipelineManager.endCameraRendering += Render;
    }

    void OnDestroy()
    {
        RenderPipelineManager.endCameraRendering -= Render;
    }

    void Render(ScriptableRenderContext context, Camera cam)
    {
        if (cam.gameObject != gameObject) return;

        if (onPostProcessingBegin != null)
        {
            onPostProcessingBegin(finalDestination);
        }
        Init();
        temporaryTextures.Clear();

        RenderTexture currentSource = initialSource;
        RenderTexture currentDestination = null;

        if (effects != null)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                PostProcessingEffect effect = effects[i];
                if (effect != null)
                {
                    if (i == effects.Length - 1)
                    {
                        // Final effect, so render into final destination texture
                        currentDestination = finalDestination;
                    }
                    else
                    {
                        // Get temporary texture to render this effect into
                        currentDestination = TemporaryRenderTexture(finalDestination);
                        temporaryTextures.Add(currentDestination);
                    }

                    effect.Render(currentSource, currentDestination); // render the effect
                    currentSource = currentDestination; // output of this effect becomes input for next effect
                }
            }
        }

        // In case dest texture was not rendered into (due to being provided a null effect),
        // copy current src to dest
        if (currentDestination != finalDestination)
        {
            Graphics.Blit(currentSource, finalDestination, defaultMat);
        }

        foreach (var texture in temporaryTextures)
        {
            RenderTexture.ReleaseTemporary(texture);
        }
        
        if (debugOceanMask)
        {
            // TODO: Render ocean mask
        }

        // Trigger post processing complete event
        if (onPostProcessingComplete != null)
        {
            onPostProcessingComplete(finalDestination);
        }
    }

    void Init()
    {
        if (defaultShader == null)
        {
            defaultShader = Shader.Find("Unlit/Texture");
        }
        defaultMat = new Material(defaultShader);

        if (initialSource == null)
        {
            initialSource = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBHalf);
        }
        if (finalDestination == null)
        {
            finalDestination = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBHalf);
        }
    }

    public static RenderTexture TemporaryRenderTexture(RenderTexture template)
    {
        return RenderTexture.GetTemporary(template.descriptor);
    }
}
