using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting {

    const string bufferName = "Lighting";

    CommandBuffer buffer = new CommandBuffer {
        name = bufferName
    };

    private const int MaxDirLightCount = 4;

    private static readonly int DirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    private static readonly int DirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
    private static readonly int DirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

    private static  Vector4[] DirLightColors = new Vector4[MaxDirLightCount];
    private static  Vector4[] DirLightDirections = new Vector4[MaxDirLightCount];

    private CullingResults _cullingResults;
	
    public void Setup (ScriptableRenderContext context, CullingResults cullingResults)
    {
        this._cullingResults = cullingResults;
        buffer.BeginSample(bufferName);
        // SetupDirectionalLight();
        SetupLights();
        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    private void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = _cullingResults.visibleLights;
        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++) {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                SetupDirectionalLight(dirLightCount++, ref visibleLight);
                if (dirLightCount >= MaxDirLightCount)
                {
                    break;
                }
            }
        }

        buffer.SetGlobalInt(DirLightCountId, dirLightCount);
        buffer.SetGlobalVectorArray(DirLightColorsId, DirLightColors);
        buffer.SetGlobalVectorArray(DirLightDirectionsId, DirLightDirections);
    }

    void SetupDirectionalLight (int index, ref VisibleLight visibleLight) {
        DirLightColors[index] = visibleLight.finalColor;
        DirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
    }
}