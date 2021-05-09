using System.Collections.Generic;
using UnityEngine;

namespace Voxel
{
    public class Atlas : MonoBehaviour
    {
        public Material material;

        public static Dictionary<string, Vector2> uvs;
        public static int dimensions = 0;

        private Texture2D[] _textures;

        private readonly int _textureWidth = 16;
        private readonly int _textureHeight = 16;

        public void GenerateAtlas()
        {
            uvs = new Dictionary<string, Vector2>();
            _textures = Resources.LoadAll<Texture2D>("Atlas");

            var textureCount = _textures.Length;

            dimensions = GetAtlasDimension(textureCount);

            var atlas = new Texture2D(_textureWidth * dimensions, _textureHeight * dimensions)
            {
                anisoLevel = 1,
                filterMode = FilterMode.Point
            };

            for (var i = 0; i < _textures.Length; i++) {
                var texture = _textures[i];

                var horizontalAtlasOffset = (i % dimensions) * _textureWidth;
                var verticalAtlasOffset = (i / dimensions) * _textureHeight;

                var textureX = i % dimensions;
                var textureY = i / dimensions;

                uvs.Add(
                    texture.name,
                    new Vector2((textureX * 1f) / (dimensions * 1f), (textureY * 1f) / (dimensions * 1f))
                );

                var pixels = texture.GetPixels(0, 0, texture.width, texture.height);
                for (var y = 0; y < texture.height; y++)
                {
                    for (var x = 0; x < texture.width; x++)
                    {
                        atlas.SetPixel(x + horizontalAtlasOffset, y + verticalAtlasOffset, pixels[x + y * 16]);
                    }
                }
            }
            atlas.Apply();

            material.mainTexture = atlas;
        }

        private static int GetAtlasDimension(int count) => (int)Mathf.Pow(2, Mathf.Ceil(Mathf.Log(count) / Mathf.Log(4)));
    }
}
