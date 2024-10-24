using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class CustomPostProcessPass : ScriptableRenderPass
{
    // Used to render from camera to post processings
    // back and forth, until we render the final image to
    // the camera
    RTHandle source;
    RTHandle destinationA;
    RTHandle destinationB;
    RTHandle latestDest;

    readonly int temporaryRTIdA = Shader.PropertyToID("_TempRT");
    readonly int temporaryRTIdB = Shader.PropertyToID("_TempRTB");

    public CustomPostProcessPass()
    {
        // Set the render pass event
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // Grab the camera target descriptor. We will use this when creating a temporary render texture.
        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;

        var renderer = renderingData.cameraData.renderer;
        source = renderer.cameraColorTargetHandle;

        // Create a temporary render texture using the descriptor from above.
        cmd.GetTemporaryRT(temporaryRTIdA, descriptor, FilterMode.Bilinear);
        RenderingUtils.ReAllocateHandleIfNeeded(ref destinationA, descriptor, FilterMode.Bilinear);
        cmd.GetTemporaryRT(temporaryRTIdB, descriptor, FilterMode.Bilinear);
        RenderingUtils.ReAllocateHandleIfNeeded(ref destinationB, descriptor, FilterMode.Bilinear);
    }

    // The actual execution of the pass. This is where custom rendering occurs.
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // Skipping post processing rendering inside the scene view
        //if (renderingData.cameraData.isSceneViewCamera) return;

        // Here you get your materials from your custom class
        var materials = CustomPostProcessingMaterials.Instance;
        if (materials == null)
        {
            Debug.LogError("Custom Post Processing Materials instance is null.");
            return;
        }
        CommandBuffer cmd = CommandBufferPool.Get("Custom Post Processing");
        cmd.Clear();

        // This holds all the current Volumes information
        // which we will need later
        var stack = VolumeManager.instance.stack;

        #region Local Methods
        // Swaps render destinations back and forth, so that
        // we can have multiple passes and similar with only a few textures
        void BlitTo(Material mat, int pass = 0)
        {
            var first = latestDest;
            var last = first == destinationA ? destinationB : destinationA;
            cmd.Blit(first, last, mat, pass);

            latestDest = last;
        }
        #endregion

        // Starts with the camera source
        latestDest = source;

        // Custom effect here
        var customEffect = stack.GetComponent<CustomEffectComponent>();
        // Only process if the effect is active
        if (customEffect.IsActive())
        {
            var material = materials.customEffect;
            // P.s. optimize by caching the property ID somewhere else
            material.SetFloat("_Intensity", customEffect.intensity.value);
            material.SetColor("_OverlayColor", customEffect.overlayColor.value);

            BlitTo(material);
        }

        // Add any other custom effect/component you want, in your preferred order
        // Custom effect 2, 3 , ...

        // DONE! Now that we have processed all our custom effects,
        // applies the final result to the camera
        cmd.Blit(latestDest, source);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    // Cleans the temporary RTs when we don't need them anymore
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(temporaryRTIdA);
        cmd.ReleaseTemporaryRT(temporaryRTIdB);
    }
}
