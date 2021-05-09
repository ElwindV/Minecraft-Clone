using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderwaterShaderController : MonoBehaviour
{
    [SerializeField]
    private Material underwaterMaterial;

    public float startTransition = 12.5f;

    public float endTransition = 11.5f;
    
    private static readonly int ApplyPercentage = Shader.PropertyToID("_ApplyPercentage");

    private void Start()
    {
        underwaterMaterial.SetFloat(ApplyPercentage, 0f);
    }

    private void Update()
    {
        var value = 0f;
        
        if (transform.position.y < startTransition)
        {
            var v = Mathf.Clamp(transform.position.y, endTransition, startTransition);
            v -= endTransition;
            v /= startTransition - endTransition;
            value = 1f - v;
        }

        underwaterMaterial.SetFloat(ApplyPercentage, value);
    }
}
