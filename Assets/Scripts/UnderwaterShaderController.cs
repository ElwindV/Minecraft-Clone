using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderwaterShaderController : MonoBehaviour
{
    [SerializeField]
    private Material underwaterMaterial;

    public float startTransition = 12.5f;

    public float endTransition = 11.5f;
    
    void Start()
    {
        underwaterMaterial.SetFloat("_ApplyPercentage", 0f);
    }

    void Update()
    {
        float value = 0f;
        
        if (transform.position.y < startTransition)
        {
            float v = Mathf.Clamp(transform.position.y, endTransition, startTransition);
            v -= endTransition;
            v /= startTransition - endTransition;
            value = 1f - v;
        }

        underwaterMaterial.SetFloat("_ApplyPercentage", value);
    }
}
