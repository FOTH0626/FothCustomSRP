using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private ScriptableRenderContext _context;

    private Camera _camera;

    private const string BufferName = "Render Camera";

    private CommandBuffer _buffer = new CommandBuffer { name = BufferName };

    private CullingResults _cullingResults;
    
    private static ShaderTagId _unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    
        
    public void Render(ScriptableRenderContext context, Camera camera)
    {
        _context = context;
        _camera = camera;
        
        PrepareBuffer();
        PrepareForSceneWindow();
        if(!Cull())
            return;
        Setup();
        DrawVisibleGeometry();
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
    

    private void DrawVisibleGeometry()
    {
        var sortingSettings = new SortingSettings(_camera){ criteria = SortingCriteria.CommonOpaque};
        var drawingSettings = new DrawingSettings(_unlitShaderTagId, sortingSettings);
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
