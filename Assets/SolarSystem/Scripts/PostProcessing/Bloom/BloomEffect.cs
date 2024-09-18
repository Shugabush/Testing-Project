// Based on bloom effect by Keijiro Takahashi (see accompanying readme and license for details)
// The original can be found here: https://github.com/keijiro/KinoBloom/releases
// Minor modifications made

using UnityEngine;

public class BloomEffect : PostProcessingEffect
{
    /// <summary>
    /// Prefilter threshold (gamma-encoded) Filters out pixels under this level of brightness
    /// </summary>
    public float ThresholdGamma
    {
        get { return Mathf.Max(threshold, 0); }
        set { threshold = value; }
    }

    /// <summary>
    /// Prefilter threshold (linearly-encoded)
    /// Filters out pixels under this level of brightness.
    /// </summary>
    public float ThresholdLinear
    {
        get { return GammaToLinear(ThresholdGamma); }
        set { threshold = LinearToGamma(value); }
    }

    [SerializeField]
    [Tooltip("Filters out pixels under this level of brightness")]
    float threshold = 0.8f;

    [Range(0f, 1f)]
    [Tooltip("Makes transition between under/over-threshold gradual.")]
    public float softKnee = 0.5f;

    [Range(1f, 7f)]
    [Tooltip("Changes extent of veiling effects\n" + 
        "in a screen resolution-independent fashion.")]
    public float radius = 2.5f;

    public float Intensity
    {
        get { return Mathf.Max(intensity, 0); }
        set { intensity = value; }
    }

    [SerializeField]
    [Tooltip("Blend factor of the result image")]
    float intensity = 0.8f;

    [Tooltip("Controls filter quality and buffer resolution")]
    public bool highQuality;

    [Tooltip("Reduces flashing noise with an additional filter")]
    public bool antiFlicker = true;

    [SerializeField, HideInInspector]
    Shader shader;

    const int kMaxIterations = 16;
    RenderTexture[] blurBuffer1 = new RenderTexture[kMaxIterations];
    RenderTexture[] blurBuffer2 = new RenderTexture[kMaxIterations];

    float LinearToGamma(float x)
    {
        return Mathf.LinearToGammaSpace(x);
    }

    float GammaToLinear(float x)
    {
        return Mathf.GammaToLinearSpace(x);
    }

    void OnEnable()
    {
        this.shader = null;
        var shader = this.shader ? this.shader : Shader.Find("Hidden/Kino/Bloom");
        material = new Material(shader);
        material.hideFlags = HideFlags.DontSave;
    }

    void OnDisable()
    {
        DestroyImmediate(material);
    }

    public override void Render(RenderTexture source, RenderTexture destination)
    {
        var useRGBM = Application.isMobilePlatform;

        // source texture size
        var tw = source.width;
        var th = source.height;

        // halve the texture size for the low quality mode
        if (!highQuality)
        {
            tw /= 2;
            th /= 2;
        }

        // blur buffer format
        var rtFormat = useRGBM ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR;

        // determine the iteration count
        var logh = Mathf.Log(th, 2) + radius - 8;
        var logh_i = (int)logh;
        var iterations = Mathf.Clamp(logh_i, 1, kMaxIterations);

        // update the shader properties
        var lthresh = ThresholdLinear;
        material.SetFloat("_Threshold", lthresh);

        var knee = lthresh * softKnee + 1e-5f;
        var curve = new Vector3(lthresh - knee, knee * 2f, 0.25f / knee);
        material.SetVector("_Curve", curve);

        var pfo = highQuality && antiFlicker;
        material.SetFloat("_PrefilterOffs", pfo ? -0.5f : 0.0f);

        material.SetFloat("_SampleScale", 0.5f + logh - logh_i);
        material.SetFloat("_Intensity", intensity);

        // prefilter pass
        var prefiltered = RenderTexture.GetTemporary(tw, th, 0, rtFormat);
        var pass = antiFlicker ? 1 : 0;
        Graphics.Blit(source, prefiltered, material, pass);

        // construct a mip pyramid
        var last = prefiltered;
        for (var level = 0; level < iterations; level++)
        {
            blurBuffer1[level] = RenderTexture.GetTemporary(last.width / 2, last.height / 2, 0, rtFormat);

            pass = level == 0 ? (antiFlicker ? 3 : 2) : 4;
            Graphics.Blit(last, blurBuffer1[level], material, pass);

            last = blurBuffer1[level];
        }

        // unsample and combine loop
        for (var level = iterations - 2; level >= 0; level--)
        {
            var baseTex = blurBuffer1[level];
            material.SetTexture("_BaseTex", baseTex);

            blurBuffer2[level] = RenderTexture.GetTemporary(baseTex.width, baseTex.height, 0, rtFormat);

            pass = highQuality ? 6 : 5;
            Graphics.Blit(last, blurBuffer2[level], material, pass);
            last = blurBuffer2[level];
        }

        // finish process
        material.SetTexture("_BaseTex", source);
        pass = highQuality ? 8 : 7;
        Graphics.Blit(last, destination, material, pass);

        // release the temporary buffers
        for (var i = 0; i < kMaxIterations; i++)
        {
            if (blurBuffer1[i] != null)
            {
                RenderTexture.ReleaseTemporary(blurBuffer1[i]);
            }
            if (blurBuffer2[i] != null)
            {
                RenderTexture.ReleaseTemporary(blurBuffer2[i]);
            }

            blurBuffer1[i] = null;
            blurBuffer2[i] = null;
        }

        RenderTexture.ReleaseTemporary(prefiltered);
    }
}
