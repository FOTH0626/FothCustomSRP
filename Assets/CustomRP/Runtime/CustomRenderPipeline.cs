using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    private CameraRenderer _renderer = new CameraRenderer();
    
    [Obsolete("Render with an array parameter is deprecated. Use Render with a list parameter instead. If you\'re extending the RenderPipeline class, override the Render method with a List parameter to perform rendering in order to avoid unnecessary allocations and copies. #from 6000.1", false)]
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        
    }
    
    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach (var eachCamera in cameras)
        {
            _renderer.Render(context, eachCamera);
        }
    }

}
