using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
 using UnityEngine.Rendering.Universal;


 public class BuffererdRenderPass : ScriptableRenderPass
{
    UnityEngine.Experimental.Rendering.Universal.RenderQueueType renderQueueType;
    FilteringSettings m_FilteringSettings;
    string m_ProfilerTag;
    ProfilingSampler m_ProfilingSampler;

    internal RTHandle clearingHandle;

    public Material overrideMaterial { get; set; }
    List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

    RenderStateBlock m_RenderStateBlock;

    public BuffererdRenderPass(string profilerTag, RenderPassEvent renderPassEvent, string[] shaderTags, UnityEngine.Experimental.Rendering.Universal.RenderQueueType renderQueueType, int layerMask)
    {
        m_ProfilerTag = profilerTag;
        m_ProfilingSampler = new ProfilingSampler(profilerTag);
        this.renderPassEvent = renderPassEvent;
        this.renderQueueType = renderQueueType;
        RenderQueueRange renderQueueRange = (renderQueueType == UnityEngine.Experimental.Rendering.Universal.RenderQueueType.Transparent)
            ? RenderQueueRange.transparent
            : RenderQueueRange.opaque;
        m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);

        if (shaderTags != null && shaderTags.Length > 0)
        {
            foreach (var passName in shaderTags)
                m_ShaderTagIdList.Add(new ShaderTagId(passName));
        }
        else
        {
            m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));
            m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));;
        }

        m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        SortingCriteria sortingCriteria = (renderQueueType == UnityEngine.Experimental.Rendering.Universal.RenderQueueType.Transparent)
            ? SortingCriteria.CommonTransparent
            : renderingData.cameraData.defaultOpaqueSortFlags;

        if(clearingHandle == null)
            clearingHandle = RTHandles.Alloc(16,16);

        DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
        CullingResults cullingResults = renderingData.cullResults;
        CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
        cmd.SetRenderTarget(clearingHandle);
        if (overrideMaterial != null)
        {
            cmd.DrawProcedural(Matrix4x4.identity, overrideMaterial, 0, MeshTopology.Triangles, 3, 1);
        }
        else
        {
            Debug.LogError("missing override material");
        }
        cmd.SetRenderTarget(UniversalRenderer.m_ActiveCameraColorAttachment, UniversalRenderer.m_ActiveCameraDepthAttachment);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        using (new ProfilingScope(cmd, m_ProfilingSampler)) {
            context.DrawRenderers(cullingResults, ref drawingSettings, ref m_FilteringSettings,
                ref m_RenderStateBlock);
        }
        CommandBufferPool.Release(cmd);
    }
}
