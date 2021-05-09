using ScriptableObjects;
using UnityEngine;
using Voxel.Enums;

namespace Voxel
{
    public class Chunk : MonoBehaviour
    {
        private WorldGenerationSettingsSO _settings;
        
        [HideInInspector]
        public byte[,,] blocks = new byte[16, 32, 16];

        public int ChunkWidth => blocks.GetLength(0);

        public int ChunkHeight => blocks.GetLength(1);

        public int ChunkDepth => blocks.GetLength(2);

        [HideInInspector]
        public int x;

        [HideInInspector]
        public int z;

        public void AddSettings(WorldGenerationSettingsSO worldGenerationSettingsSo)
        {
            _settings = worldGenerationSettingsSo;
        }

        public void Generate()
        {
            for (var x = 0; x < 16; x++)
            {
                for (var z = 0; z < 16; z++)
                {
                    var position = transform.position;
                    
                    var xComponent = _settings.seed + ((position.x + (x * 1f)) * _settings.factor);
                    var yComponent = _settings.seed + ((position.z + (z * 1f)) * _settings.factor);
                    var noiseFactor = Mathf.PerlinNoise(xComponent, yComponent);
                    var stoneLayer = (int)(10f + noiseFactor * 15f);

                    var worldX = Mathf.Pow(-64f + (position.x + (x * 1f)), 2f);
                    var worldZ = Mathf.Pow(-64f + (position.z + (z * 1f)), 2f);
                    var multiplier = 1 - (_settings.islandFactor * worldX + _settings.islandFactor * worldZ);
                    multiplier = Mathf.Clamp(multiplier, 0, 2f);

                    stoneLayer = (int)((stoneLayer * 1f) * multiplier);
                
                    for (var y = 0; y < 32; y++)
                    {
                        if (y == 0)
                        {
                            blocks[x, y, z] = (byte)Blocks.Bedrock;
                            
                            continue;
                        }
                        if (y < stoneLayer)
                        {
                            blocks[x, y, z] = (byte)Blocks.Stone;
                            
                            continue;
                        }
                        if (y < stoneLayer + 3 && stoneLayer > _settings.waterLevel)
                        {
                            blocks[x, y, z] = (byte)Blocks.Dirt;
                            
                            continue;
                        }
                        if (y == 1) {
                            blocks[x, y, z] = (byte)Blocks.Sand;
                            
                            continue;
                        }
                        if (y < stoneLayer + 4 && y < _settings.waterLevel)
                        {
                            blocks[x, y, z] = (byte)Blocks.Sand;
                            
                            continue;
                        }
                        if (y < _settings.waterLevel)
                        {
                            blocks[x, y, z] = (byte)Blocks.Water;
                            
                            continue;
                        }
                        if (y < stoneLayer + 4)
                        {
                            blocks[x, y, z] = (y < _settings.maxGrassLevel) 
                                ? (byte)Blocks.Grass 
                                : (y > _settings.snowLevel)
                                    ? (byte)Blocks.Snow
                                    : (byte)Blocks.Stone;
                            
                            continue;
                        }
                        blocks[x, y, z] = (byte)Blocks.Air;
                    }
                }
            }
        }
    }
}
