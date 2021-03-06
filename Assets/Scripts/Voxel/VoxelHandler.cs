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
        public Chunk[,] chunks = new Chunk[0, 0];

        [HideInInspector]
        public Dictionary<string, JsonBlock> blockData;
        
        public static VoxelHandler instance = null;

        public void Awake()
        {
            instance = this;

            HandleAtlas();
            LoadBlockData();
            GenerateChunks();
        }

        private void HandleAtlas() => gameObject.GetComponent<Atlas>().GenerateAtlas();

        private void LoadBlockData()
        {
            var jsonFile = Resources.Load<TextAsset>("Data/blocks");
            var blocksContainer = JsonUtility.FromJson<JsonBlockContainer>(jsonFile.text);

            blockData = new Dictionary<string, JsonBlock>();

            foreach (var block in blocksContainer.blocks)
            {
                blockData[block.name] = block;
            }
        }

        private void GenerateChunks()
        {
            chunks = new Chunk[worldGenerationSettings.chunkCountX, worldGenerationSettings.chunkCountZ];

            for (var x = 0; x < chunks.GetLength(0); x++)
            {
                for (var z = 0; z < chunks.GetLength(1); z++)
                {
                    var chunk = new GameObject();

                    chunk.transform.SetParent(transform);
                    chunk.name = $"Chunk {x}:{z}";
                    chunk.transform.position = new Vector3(16 * x, 0, 16 * z);

                    var chunkComponent = chunk.AddComponent<Chunk>();
                    chunkComponent.x = x;
                    chunkComponent.z = z;
                    chunkComponent.AddSettings(worldGenerationSettings);
                    chunkComponent.Generate();
                    
                    chunkComponent.Mesh = chunk.AddComponent<ChunkMesh>();

                    chunks[x, z] = chunkComponent;
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
                    var chunkMesh = chunks[x, z].Mesh;
                    chunkMesh.Setup();
                    chunkMesh.Refresh();
                }
            }
        }

        private void PlaceTree(int x, int z, bool updateMeshes = false)
        {
            // DETERMINE CHUNK
            var chunkX = x / 16;
            var chunkZ = z / 16;

            if (chunkX > worldGenerationSettings.chunkCountX || chunkX < 0 || chunkZ > worldGenerationSettings.chunkCountZ || chunkZ < 0)
            {
                return;
            }

            var chunkObject = chunks[chunkX, chunkZ];

            var localX = x % 16;
            var localZ = z % 16;

            // DETERMINE BEGIN
            
            int? root = null;
            for (var y = chunkObject.ChunkHeight - 1; y >= 0; y--) {
                var block = chunkObject.blocks[localX, y, localZ];

                if (block == Blocks.Grass) {
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
            chunk.blocks[localX, y, localZ] = block;

            if (!updateMeshes)
            {
                return;
            }

            // UPDATE MESH
            var chunkMesh = chunk.Mesh;
            chunkMesh.Refresh();

            // UPDATE NEIGHBOURS
            if (localX == 0) chunkMesh.leftChunk?.Mesh.Refresh();
            if (localX == 15) chunkMesh.rightChunk?.Mesh.Refresh();
            
            if (localZ == 0) chunkMesh.frontChunk?.Mesh.Refresh();
            if (localZ == 15) chunkMesh.backChunk?.Mesh.Refresh();
        }
    }
}
