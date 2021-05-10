using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
using Voxel.Enums;
using Voxel.JsonObjects;

namespace Voxel
{
    public class VoxelHandler : MonoBehaviour
    {
        public WorldGenerationSettingsSO worldGenerationSettings;

        [HideInInspector]
        public GameObject[,] chunks = new GameObject[0, 0];

        [HideInInspector]
        public Dictionary<string, Block> blockData;
        
        public static VoxelHandler instance = null;
        
        private Transform _cameraTransform;
        private Transform _playerTransform;
    
        private readonly Vector3[] _chunkOffsets = new[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(16f, 0f, 0f),
            new Vector3(0f, 32f, 0f),
            new Vector3(16f, 32f, 0f),
            new Vector3(0f, 0f, 16f),
            new Vector3(16f, 0f, 16f),
            new Vector3(0f, 32f, 16f),
            new Vector3(16f, 32f, 16f),
        };

        [Range(-1f, 1f)]
        public float cutOffThreshold = 0.5f;

        [Range(2f, 100f)]
        public float cutOffDistance = 4f;

        public void Awake()
        {
            if (Camera.main != null) _cameraTransform = Camera.main.transform;
            if (Camera.main != null) _playerTransform = Camera.main.transform.parent.transform;

            instance = this;
            gameObject.GetComponent<Atlas>().GenerateAtlas();
            LoadBlockData();
            chunks = new GameObject[worldGenerationSettings.chunkCountX, worldGenerationSettings.chunkCountZ];

            for (var x = 0; x < chunks.GetLength(0); x++)
            {
                for (var z = 0; z < chunks.GetLength(1); z++)
                {
                    var chunk = new GameObject();

                    chunk.transform.SetParent(this.transform);
                    chunk.name = $"Chunk {x}:{z}";
                    chunk.transform.position = new Vector3(16 * x, 0, 16 * z);

                    var chunkComponent = chunk.AddComponent<Chunk>();
                    chunkComponent.x = x;
                    chunkComponent.z = z;
                    chunkComponent.AddSettings(worldGenerationSettings);
                    chunkComponent.Generate();

                    chunks[x, z] = chunk;
                }
            }

            var turnFraction = (1f + Mathf.Sqrt(5)) / 2f;

            var numberOfTrees = worldGenerationSettings.chunkCountX * worldGenerationSettings.chunkCountZ * worldGenerationSettings.treesPerChunk;
            for (var i = 0; i < numberOfTrees; i++) {
                var distance = i / (numberOfTrees - 1f);
                var angle = 2 * Mathf.PI * turnFraction * i;

                var x = 64f + (64f * (distance * Mathf.Cos(angle)));
                var z = 64f + (64f * (distance * Mathf.Sin(angle)));

                PlaceTree((int) x, (int) z, false);
            }

            // This loop is separate since all Meshes should be generated AFTER the chunks
            for (var x = 0; x < chunks.GetLength(0); x++)
            {
                for (var z = 0; z < chunks.GetLength(1); z++)
                {
                    chunks[x, z].AddComponent<ChunkMesh>();
                }
            }
        }

        public void Update()
        {
            if (_cameraTransform == null || _playerTransform == null)
            {
                return;
            }
        
            var forward = _cameraTransform.rotation * Vector3.forward;
            var playerPosition = _playerTransform.position;

            for (var x = 0; x < chunks.GetLength(0); x++)
            {
                for (var z = 0; z < chunks.GetLength(1); z++)
                {
                    var chunk = chunks[x, z];
                    var currentChunkX = Mathf.FloorToInt(playerPosition.x / 16f);
                    var currentChunkZ = Mathf.FloorToInt(playerPosition.z / 16f);
    
                    // If the player is standing on a chunk keep if for collision
                    if (x == currentChunkX && z == currentChunkZ)
                    {
                        chunk.SetActive(true);
                        continue;
                    }

                    float chunkGapX = currentChunkX - x;
                    float chunkGapZ = currentChunkZ - z;
                    if ((chunkGapX * chunkGapX) + (chunkGapZ * chunkGapZ) > (cutOffDistance * cutOffDistance))
                    {
                        chunk.SetActive(false);
                        continue;
                    }

                    var isActive = false;

                    for (var i = 0; i < _chunkOffsets.Length; i++)
                    {
                        var offset = _chunkOffsets[i];
                        var relativeChunkCornerPosition = (chunk.transform.position + offset) - playerPosition;

                        if (!(Vector3.Dot(forward.normalized, relativeChunkCornerPosition.normalized) >
                              cutOffThreshold)) continue;
                        isActive = true;
                        break;
                    }

                    chunk.SetActive(isActive);
                }
            }
        }

        public void PlaceTree(int x, int z, bool updateMeshes = false)
        {
            // DETERMINE CHUNK
            var chunkX = x / 16;
            var chunkZ = z / 16;

            if (chunkX > worldGenerationSettings.chunkCountX || chunkX < 0 || chunkZ > worldGenerationSettings.chunkCountZ || chunkZ < 0)
            {
                return;
            }

            var chunk = chunks[chunkX, chunkZ];

            var localX = x % 16;
            var localZ = z % 16;

            var chunkObject = chunk.GetComponent<Chunk>();
            // DETERMINE BEGIN

            int? root = null;
            for (var y = chunkObject.ChunkHeight - 1; y >= 0; y--) {
                var block = chunkObject.blocks[localX, y, localZ];

                if (block == (byte) Blocks.Grass) {
                    root = y + 1;
                }
            }

            if (root == null) {
                return;
            }

            for (int y = (int) root, i = 0; y < root + 8; y++, i++) {
                SetBlock(x, y, z, Blocks.Log, updateMeshes);
                if (i >= 3) {
                    SetBlock(x-1, y, z, Blocks.Leaf, updateMeshes);
                    SetBlock(x+1, y, z, Blocks.Leaf, updateMeshes);
                    SetBlock(x, y, z-1, Blocks.Leaf, updateMeshes);
                    SetBlock(x, y, z+1, Blocks.Leaf, updateMeshes);
                }
                if (i == 5) {
                    SetBlock(x - 1, y, z - 1, Blocks.Leaf, updateMeshes);
                    SetBlock(x + 1, y, z - 1, Blocks.Leaf, updateMeshes);
                    SetBlock(x - 1, y, z + 1, Blocks.Leaf, updateMeshes);
                    SetBlock(x + 1, y, z + 1, Blocks.Leaf, updateMeshes);
                }
                SetBlock(x, y + 1, z, Blocks.Leaf, updateMeshes);
            }
        }

        public void SetBlock(int x, int y, int z, Blocks block = Blocks.Stone, bool updateMeshes = true)
        {
            // DETERMINE CHUNK
            var chunkX = x / 16;
            var chunkZ = z / 16;

            if (chunkX > worldGenerationSettings.chunkCountX || chunkX < 0)
            {
                return;
            }

            if (chunkZ > worldGenerationSettings.chunkCountZ || chunkZ < 0)
            {
                return;
            }
            
            if (y >= 32 || y < 0)
            {
                return;
            }

            var chunk = chunks[chunkX, chunkZ];

            var localX = x % 16;
            var localZ = z % 16;

            // REMOVE BLOCK
            chunk.GetComponent<Chunk>().blocks[localX, y, localZ] = (byte)block;

            if (!updateMeshes)
            {
                return;
            }

            // UPDATE MESH
            var chunkMesh = chunk.GetComponent<ChunkMesh>();
            chunkMesh.Refresh();

            // UPDATE NEIGHBOURS
            if (localX == 0) chunkMesh?.leftChunk?.gameObject?.GetComponent<ChunkMesh>()?.Refresh();
            if (localX == 15) chunkMesh?.rightChunk?.gameObject?.GetComponent<ChunkMesh>()?.Refresh();
            
            if (localZ == 0) chunkMesh?.frontChunk?.gameObject?.GetComponent<ChunkMesh>()?.Refresh();
            if (localZ == 15) chunkMesh?.backChunk?.gameObject?.GetComponent<ChunkMesh>()?.Refresh();
        }

        private void LoadBlockData()
        {
            var jsonFile = Resources.Load<TextAsset>("Data/blocks");
            var blocksContainer = JsonUtility.FromJson<BlockContainer>(jsonFile.text);

            blockData = new Dictionary<string, Block>();

            foreach (var block in blocksContainer.blocks)
            {
                blockData[block.name] = block;
            }
        }
    }
}
