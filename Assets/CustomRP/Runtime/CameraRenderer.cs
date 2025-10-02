using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private ScriptableRenderContext _context;

    private Camera _camera;

    private const string BufferName = "Render Camera";

    private CommandBuffer _buffer = new CommandBuffer { name = BufferName };

    private CullingResults _cullingResults;
    
    private static readonly ShaderTagId _unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    private static readonly ShaderTagId _litShaderTagId = new ShaderTagId("CustomLit");

    private Lighting lighting = new Lighting();
        
    public void Render(ScriptableRenderContext context, Camera camera,
                        bool useDynamicBatching, bool useGPUInstancing)
    {
        _context = context;
        _camera = camera;
        
        PrepareBuffer();
        PrepareForSceneWindow();
        if(!Cull())
            return;
        Setup();
        lighting.Setup(context,_cullingResults);
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();
    }


    bool Cull()
    {
        if (_camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            _cullingResults = _context.Cull(ref p);
            return true;
        }

        return false;

    }

    private void Setup()
    {
        _context.SetupCameraProperties(_camera);
        CameraClearFlags flags = _camera.clearFlags;
        _buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth,
            flags <= CameraClearFlags.Color,
            flags == CameraClearFlags.Color ? _camera.backgroundColor.linear : Color.clear);
        
        _buffer.BeginSample(SampleName);

        ExecuteBuffer();

    }

    private void Submit()
    {
        _buffer.EndSample(SampleName);
        ExecuteBuffer();
        _context.Submit();
    }

    private void ExecuteBuffer()
    {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }
    

    private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        var sortingSettings = new SortingSettings(_camera){ criteria = SortingCriteria.CommonOpaque};
        var drawingSettings = new DrawingSettings(_unlitShaderTagId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        drawingSettings.SetShaderPassName(1,_litShaderTagId);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        
        _context.DrawRenderers(
            _cullingResults, ref drawingSettings, ref filteringSettings);
        
        _context.DrawSkybox(_camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        
        _context.DrawRenderers( _cullingResults, ref drawingSettings, ref filteringSettings);
    }
    


}
