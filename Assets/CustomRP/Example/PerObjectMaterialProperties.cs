using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    private static int baseColorId = Shader.PropertyToID("_BaseColor");
    static int cutoffId = Shader.PropertyToID("_Cutoff");
    private static int metallicId = Shader.PropertyToID("_Metallic");
    private static int smoothnessId = Shader.PropertyToID("_Smoothness");
    
    [SerializeField]
    Color baseColor = Color.white;
    
    [SerializeField, Range(0f, 1f)]
    float cutoff = 0.5f;
    private float metallic = 0f;
    private float smoothness = 0.5f;

    private static MaterialPropertyBlock block;

    private void OnValidate()
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }
        block.SetFloat(metallicId,metallic);
        block.SetFloat(smoothnessId,smoothness);
        block.SetColor(baseColorId,baseColor);
        block.SetFloat(cutoffId, cutoff);
        GetComponent<Renderer>().SetPropertyBlock(block);
    }

    private void Awake()
    {
        OnValidate();
    }
}