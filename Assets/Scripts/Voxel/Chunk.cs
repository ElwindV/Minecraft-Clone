using UnityEngine;
using Voxel.Enums;

namespace Voxel
{
    public class Chunk : MonoBehaviour
    {
        [HideInInspector]
        public byte[,,] blocks = new byte[16, 32, 16];

        public int ChunkWidth => blocks.GetLength(0);

        public int ChunkHeight => blocks.GetLength(1);

        public int ChunkDepth => blocks.GetLength(2);

        [HideInInspector]
        public int seed = 4113;

        [HideInInspector]
        public float factor = .07f;

        [HideInInspector]
        public float islandFactor = .0002f;

        [HideInInspector]
        public int waterLevel = 12;

        [HideInInspector]
        public int maxGrassLevel = 18;

        [HideInInspector]
        public int snowLevel = 21;

        [HideInInspector]
        public int x;

        [HideInInspector]
        public int z;

        [HideInInspector]
        public float caveFactor = .09f;

        [HideInInspector]
        public float caveCutoff = .40f;

        public void Generate()
        {
            for (var x = 0; x < 16; x++)
            {
                for (var z = 0; z < 16; z++)
                {
                    var xComponent = seed + ((transform.position.x + (x * 1f)) * factor);
                    var yComponent = seed + ((transform.position.z + (z * 1f)) * factor);
                    var noiseFactor = Mathf.PerlinNoise(xComponent, yComponent);
                    var stoneLayer = (int)(10f + noiseFactor * 15f);

                    var worldX = Mathf.Pow(-64f + (transform.position.x + (x * 1f)), 2f);
                    var worldZ = Mathf.Pow(-64f + (transform.position.z + (z * 1f)), 2f);
                    var multiplier = 1 - (islandFactor * worldX + islandFactor * worldZ);
                    multiplier = Mathf.Clamp(multiplier, 0, 2f);

                    stoneLayer = (int)((stoneLayer * 1f) * multiplier);
                
                    for (var y = 0; y < 32; y++)
                    {
                        if (y == 0)
                        {
                            blocks[x, y, z] = (byte)Blocks.Bedrock;
                        }
                        else if (y < stoneLayer)
                        {
                            blocks[x, y, z] = (byte)Blocks.Stone;
                        }
                        else if (y < stoneLayer + 3 && stoneLayer > waterLevel)
                        {
                            blocks[x, y, z] = (byte)Blocks.Dirt;
                        }
                        else if (y == 1) {
                            blocks[x, y, z] = (byte)Blocks.Sand;
                        }
                        else if (y < stoneLayer + 4 && y < waterLevel)
                        {
                            blocks[x, y, z] = (byte)Blocks.Sand;
                        }
                        else if (y < waterLevel)
                        {
                            blocks[x, y, z] = (byte)Blocks.Water;
                        }
                        else if (y < stoneLayer + 4)
                        {
                            blocks[x, y, z] = (y < maxGrassLevel) 
                                ? (byte)Blocks.Grass 
                                : (y > snowLevel)
                                    ? (byte)Blocks.Snow
                                    : (byte)Blocks.Stone;
                        }
                        else
                        {
                            blocks[x, y, z] = (byte)Blocks.Air;
                        }
                    }
                }
            }
        }
    }
}
