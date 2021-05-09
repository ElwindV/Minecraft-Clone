using ScriptableObjects;
using UnityEngine;
using Voxel;

public class UnderwaterShaderController : MonoBehaviour
{
    [SerializeField]
    private Material underwaterMaterial;

    private WorldGenerationSettingsSO _settings;
    
    [Range(0, 1)] public float startTransition = 0.5f;
    [Range(-1, 0)] public float endTransition = -0.5f;
    
    private static readonly int ApplyPercentage = Shader.PropertyToID("_ApplyPercentage");

    private void Start()
    {
        underwaterMaterial.SetFloat(ApplyPercentage, 0f);

        _settings = VoxelHandler.instance.worldGenerationSettings;
    }

    private void Update()
    {
        var value = 0f;

        if (transform.position.y < _settings.waterLevel + startTransition)
        {
            var v = Mathf.Clamp(transform.position.y, _settings.waterLevel + endTransition, _settings.waterLevel + startTransition);
            v -= _settings.waterLevel + endTransition;
            v /= startTransition - endTransition;
            value = 1f - v;
        }

        underwaterMaterial.SetFloat(ApplyPercentage, value);
    }
}
