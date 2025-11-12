using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RGBSplitFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material material;
        [Range(0f, 10f)] public float offset = 1f;
        public RenderPassEvent passEvent = RenderPassEvent.AfterRendering;
    }

    public Settings settings = new Settings();
    RGBSplitPass pass;

    class RGBSplitPass : ScriptableRenderPass
    {
        Material mat;
        float offset;

        RenderTargetIdentifier source;
        RTHandle tempTexRT;

        public RGBSplitPass(Material m, float o, RenderPassEvent evt)
        {
            mat = m;
            offset = o;
            renderPassEvent = evt;
        }

        [System.Obsolete("Use of OnCameraSetup is deprecated in newer URP, kept here for 2D Renderer compatibility.")]
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            RenderingUtils.ReAllocateIfNeeded(ref tempTexRT, desc, FilterMode.Bilinear, name: "_RGBSplitTempTex");
            mat.SetFloat("_Offset", offset);
        }

        [System.Obsolete("Use of OnCameraSetup is deprecated in newer URP, kept here for 2D Renderer compatibility.")]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
        //Debug.Log("RGBSplit Execute");
        if (mat == null) return;

        var cmd = CommandBufferPool.Get("RGBSplit");
        var source = renderingData.cameraData.renderer.cameraColorTargetHandle;

        Blitter.BlitCameraTexture(cmd, source, tempTexRT, mat, 0);
        Blitter.BlitCameraTexture(cmd, tempTexRT, source);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        // RTHandle 由系统管理，不需要手动 Release
    }
    }

    public override void Create()
    {
        if (settings.material)
            pass = new RGBSplitPass(settings.material, settings.offset, settings.passEvent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (pass == null)
            return;

        renderer.EnqueuePass(pass);
    }
}