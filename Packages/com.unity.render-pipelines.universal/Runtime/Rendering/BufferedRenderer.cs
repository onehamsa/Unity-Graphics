using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BufferedRenderer : ScriptableRendererFeature
{
    [System.Serializable]
    public class RenderObjectsSettings
    {
        public string passTag = "BufferedRender";
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

        public FilterSettings filterSettings = new FilterSettings();

        public Material overrideMaterial = null;
    }

    [System.Serializable]
    public class FilterSettings
    {
        // TODO: expose opaque, transparent, all ranges as drop down
        public UnityEngine.Experimental.Rendering.Universal.RenderQueueType RenderQueueType;
        public LayerMask LayerMask;
        public string[] PassNames;

        public FilterSettings()
        {
            RenderQueueType = UnityEngine.Experimental.Rendering.Universal.RenderQueueType.Opaque;
            LayerMask = 0;
        }
    }

    public RenderObjectsSettings settings = new RenderObjectsSettings();

    BuffererdRenderPass renderObjectsPass;

    public override void Create()
    {
        FilterSettings filter = settings.filterSettings;
        renderObjectsPass = new BuffererdRenderPass(settings.passTag, settings.Event, filter.PassNames,
            filter.RenderQueueType, filter.LayerMask);

        renderObjectsPass.overrideMaterial = settings.overrideMaterial;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderObjectsPass);
    }
}
