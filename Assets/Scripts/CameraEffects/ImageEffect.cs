using UnityEngine;

[ExecuteInEditMode]
public class ImageEffect : MonoBehaviour
{
    [SerializeField]
    private Material _effect;

    public void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (_effect == null) return;
        Graphics.Blit(src, dst, _effect);
    }
}
