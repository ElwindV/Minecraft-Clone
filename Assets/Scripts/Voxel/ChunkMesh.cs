using System.Collections.Generic;
using UnityEngine;
using Voxel.Enums;
using Voxel.JsonObjects;

namespace Voxel
{
    public class ChunkMesh : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private Mesh _mesh;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;

        private GameObject _waterObject;
        private MeshFilter _waterMeshFilter;
        private Mesh _waterMesh;
        private MeshRenderer _waterMeshRenderer;

        private Chunk _chunk;
        private Material _material;

        private Material _waterMaterial;

        private List<Vector3> _vertexList;
        private List<int> _triangleList;
        private List<Vector3> _normalList;
        private List<Vector2> _uvList;

        private List<Vector3> _waterVertexList;
        private List<int> _waterTriangleList;
        private List<Vector3> _waterNormalList;
        private List<Vector2> _waterUvList;

        [HideInInspector]
        public Chunk leftChunk;
        [HideInInspector]
        public Chunk rightChunk;
        [HideInInspector]
        public Chunk frontChunk;
        [HideInInspector]
        public Chunk backChunk;

        public void Setup()
        {
            _meshFilter = this.gameObject.AddComponent<MeshFilter>();
            _meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            _meshCollider = this.gameObject.AddComponent<MeshCollider>();

            _waterObject = new GameObject();
            _waterObject.transform.parent = this.transform;
            _waterObject.name = "Water";
            _waterObject.transform.localPosition = Vector3.zero;
            _waterMeshFilter = _waterObject.AddComponent<MeshFilter>();
            _waterMeshRenderer = _waterObject.AddComponent<MeshRenderer>();

            _chunk = this.gameObject.GetComponent<Chunk>();

            _material = Resources.Load<Material>("Materials/Voxel");
            _meshRenderer.sharedMaterial = this._material;
        
            _waterMaterial = Resources.Load<Material>("Materials/Water");
            _waterMeshRenderer.sharedMaterial = this._waterMaterial;

            leftChunk = (_chunk.x - 1 >= 0)
                ? VoxelHandler.instance.chunks[_chunk.x - 1, _chunk.z].GetComponent<Chunk>()
                : null;
            rightChunk = (_chunk.x + 1 < VoxelHandler.instance.chunks.GetLength(0))
                ? VoxelHandler.instance.chunks[_chunk.x + 1, _chunk.z].GetComponent<Chunk>()
                : null;
            backChunk = (_chunk.z + 1 < VoxelHandler.instance.chunks.GetLength(1))
                ? VoxelHandler.instance.chunks[_chunk.x, _chunk.z + 1].GetComponent<Chunk>()
                : null;
            frontChunk = (_chunk.z - 1 >= 0)
                ? VoxelHandler.instance.chunks[_chunk.x, _chunk.z - 1].GetComponent<Chunk>()
                : null;
        }

        public void Refresh()
        {
            var mesh = new Mesh();
            var waterMesh = new Mesh();

            _vertexList = new List<Vector3>();
            _triangleList = new List<int>();
            _normalList = new List<Vector3>();
            _uvList = new List<Vector2>();
        
            _waterVertexList = new List<Vector3>();
            _waterTriangleList = new List<int>();
            _waterNormalList = new List<Vector3>();
            _waterUvList = new List<Vector2>();

            for (var x = 0; x < _chunk.blocks.GetLength(0); x++)
            {
                for (var y = 0; y < _chunk.blocks.GetLength(1); y++)
                {
                    for (var z = 0; z < _chunk.blocks.GetLength(2); z++)
                    {
                        var block = _chunk.blocks[x, y, z];
                        var currentPosition = new Vector3(x, y, z);

                        if (block == Blocks.Air)
                        {
                            continue;
                        }

                        var rightBlock = ((x + 1 < _chunk.blocks.GetLength(0)) ? _chunk.blocks[x + 1, y, z]
                            : ((rightChunk != null) ? rightChunk.blocks[0, y, z] : Blocks.Air));
                        var leftBlock = ((x - 1 >= 0) ? _chunk.blocks[x - 1, y, z]
                            : ((leftChunk != null) ? leftChunk.blocks[leftChunk.ChunkWidth - 1, y, z] : Blocks.Air));

                        var topBlock = ((y + 1 < _chunk.blocks.GetLength(1)) ? _chunk.blocks[x, y + 1, z] : Blocks.Air);
                        var bottomBlock = ((y - 1 >= 0) ? _chunk.blocks[x, y - 1, z] : Blocks.Air);

                        var backBlock = ((z + 1 < _chunk.blocks.GetLength(2)) ? _chunk.blocks[x, y, z + 1]
                            : ((backChunk != null) ? backChunk.blocks[x, y, 0] : Blocks.Air));
                        var frontBlock = ((z - 1 >= 0) ? _chunk.blocks[x, y, z - 1]
                            : ((frontChunk != null) ? frontChunk.blocks[x, y, frontChunk.ChunkDepth - 1] : Blocks.Air));

                        HandleRight(ref block, ref rightBlock, ref currentPosition);
                        HandleLeft(ref block, ref leftBlock, ref currentPosition);
                        HandleTop(ref block, ref topBlock, ref currentPosition);
                        if (y != 0)
                            HandleBottom(ref block, ref bottomBlock, ref currentPosition);
                        HandleBack(ref block, ref backBlock, ref currentPosition);
                        HandleFront(ref block, ref frontBlock, ref currentPosition);
                    }
                }
            }

            _meshFilter.mesh = mesh;
            mesh.vertices = _vertexList.ToArray();
            mesh.triangles = _triangleList.ToArray();
            mesh.normals = _normalList.ToArray();
            mesh.uv = _uvList.ToArray();
            _meshCollider.sharedMesh = mesh;
        
            _waterMeshFilter.mesh = waterMesh;
            waterMesh.vertices = _waterVertexList.ToArray();
            waterMesh.triangles = _waterTriangleList.ToArray();
            waterMesh.normals = _waterNormalList.ToArray();
            waterMesh.uv = _waterUvList.ToArray();
        }

        private void AddFaceNormals(Vector3 direction, bool isWater = false)
        {
            for (short i = 0; i < 4; i++)
            {
                if (isWater)
                {
                    _waterNormalList.Add(direction);
                    continue;
                }

                _normalList.Add(direction);
            }
        }

        private void AddTriangles(int baseVertexNumber, bool isWater = false)
        {
            if (isWater)
            {
                _waterTriangleList.Add(baseVertexNumber + 0);
                _waterTriangleList.Add(baseVertexNumber + 3);
                _waterTriangleList.Add(baseVertexNumber + 1);

                _waterTriangleList.Add(baseVertexNumber + 0);
                _waterTriangleList.Add(baseVertexNumber + 2);
                _waterTriangleList.Add(baseVertexNumber + 3);
            
                return;
            }

            _triangleList.Add(baseVertexNumber + 0);
            _triangleList.Add(baseVertexNumber + 3);
            _triangleList.Add(baseVertexNumber + 1);

            _triangleList.Add(baseVertexNumber + 0);
            _triangleList.Add(baseVertexNumber + 2);
            _triangleList.Add(baseVertexNumber + 3);
        }

        private void AddUVs(Blocks block, Sides side, bool isWater = false)
        {
            if (isWater)
            {
                _waterUvList.Add(Vector2.zero);
                _waterUvList.Add(Vector2.right);
                _waterUvList.Add(Vector2.up);
                _waterUvList.Add(Vector2.right + Vector2.up);

                return;
            }

            var blockData = VoxelHandler.instance.blockData[block.ToString()];
            var texture = (side == Sides.Top)
                ? blockData.textures.top
                : (side == Sides.Bottom)
                    ? blockData.textures.bottom
                    : blockData.textures.side;
            var textureStart = Atlas.uvs[texture];

            var divider = 1f / (Atlas.dimensions);
            _uvList.Add(textureStart + divider * Vector2.zero);
            _uvList.Add(textureStart + divider * Vector2.right);
            _uvList.Add(textureStart + divider * Vector2.up);
            _uvList.Add(textureStart + divider * Vector2.right + divider * Vector2.up);
        }

        private void HandleRight(ref Blocks block, ref Blocks rightBlock, ref Vector3 currentPosition)
        {
            if (block ==  Blocks.Water)
            {
                return;
            }
            
            var blockData = VoxelHandler.instance.blockData[block.ToString()];
            var rightBlockData = VoxelHandler.instance.blockData[rightBlock.ToString()];

            var isVisible = !blockData.transparent && rightBlockData.transparent || rightBlock == Blocks.Air;

            if (!isVisible)
            {
                return;
            }

            var size = _vertexList.Count;
            _vertexList.Add(currentPosition + Vector3.right);                                    // + 0
            _vertexList.Add(currentPosition + Vector3.right + Vector3.forward);                  // + 1
            _vertexList.Add(currentPosition + Vector3.right + Vector3.up);                       // + 2
            _vertexList.Add(currentPosition + Vector3.right + Vector3.forward + Vector3.up);     // + 3

            AddFaceNormals(Vector3.right);
            AddTriangles(size);
            AddUVs(block, Sides.Right);
        }

        private void HandleLeft(ref Blocks block, ref Blocks leftBlock, ref Vector3 currentPosition)
        {
            if (block ==  Blocks.Water)
            {
                return;
            }
            
            var blockData = VoxelHandler.instance.blockData[block.ToString()];
            var leftBlockData = VoxelHandler.instance.blockData[leftBlock.ToString()];

            var isVisible = !blockData.transparent && leftBlockData.transparent || leftBlock == Blocks.Air;

            if (!isVisible)
            {
                return;
            }

            var size = _vertexList.Count;
            _vertexList.Add(currentPosition + Vector3.forward);                  // + 0
            _vertexList.Add(currentPosition);                                    // + 1
            _vertexList.Add(currentPosition + Vector3.forward + Vector3.up);     // + 2
            _vertexList.Add(currentPosition + Vector3.up);                       // + 3

            AddFaceNormals(Vector3.left);
            AddTriangles(size);
            AddUVs(block, Sides.Left);
        }

        private void HandleTop(ref Blocks block, ref Blocks topBlock, ref Vector3 currentPosition)
        {
            var blockData = VoxelHandler.instance.blockData[block.ToString()];
            var topBlockData = VoxelHandler.instance.blockData[topBlock.ToString()];

            var isVisible = false || !blockData.transparent && topBlockData.transparent || topBlock == Blocks.Air;

            if (!isVisible)
            {
                return;
            }

            if (block ==  Blocks.Water)
            {
                var waterSize = _waterVertexList.Count;
            
                _waterVertexList.Add(currentPosition + Vector3.up);                                    // + 0
                _waterVertexList.Add(currentPosition + Vector3.up + Vector3.right);                    // + 1
                _waterVertexList.Add(currentPosition + Vector3.up + Vector3.forward);                  // + 2
                _waterVertexList.Add(currentPosition + Vector3.up + Vector3.right + Vector3.forward);  // + 3
            
                AddFaceNormals(Vector3.up, true);
                AddTriangles(waterSize, true);
                AddUVs(block, Sides.Top, true);
            
                return;
            }
        
            var size = _vertexList.Count;
            _vertexList.Add(currentPosition + Vector3.up);                                    // + 0
            _vertexList.Add(currentPosition + Vector3.up + Vector3.right);                    // + 1
            _vertexList.Add(currentPosition + Vector3.up + Vector3.forward);                  // + 2
            _vertexList.Add(currentPosition + Vector3.up + Vector3.right + Vector3.forward);  // + 3

            AddFaceNormals(Vector3.up);
            AddTriangles(size);
            AddUVs(block, Sides.Top);
        }

        private void HandleBottom(ref Blocks block, ref Blocks bottomBlock, ref Vector3 currentPosition)
        {
            if (block == Blocks.Water)
            {
                return;
            }
            
            var blockData = VoxelHandler.instance.blockData[block.ToString()];
            var bottomBlockData = VoxelHandler.instance.blockData[bottomBlock.ToString()];
            
            var isVisible = !blockData.transparent && bottomBlockData.transparent || bottomBlock == Blocks.Air;

            if (!isVisible)
            {
                return;
            }

            var size = _vertexList.Count;
            _vertexList.Add(currentPosition + Vector3.right);                    // + 0
            _vertexList.Add(currentPosition);                                    // + 1
            _vertexList.Add(currentPosition + Vector3.right + Vector3.forward);  // + 2
            _vertexList.Add(currentPosition + Vector3.forward);                  // + 3

            AddFaceNormals(Vector3.down);
            AddTriangles(size);
            AddUVs(block, Sides.Bottom);
        }

        private void HandleBack(ref Blocks block, ref Blocks backBlock, ref Vector3 currentPosition)
        {
            if (block == Blocks.Water)
            {
                return;
            }

            var blockData = VoxelHandler.instance.blockData[block.ToString()];
            var backBlockData = VoxelHandler.instance.blockData[backBlock.ToString()];

            var isVisible = !blockData.transparent && backBlockData.transparent || backBlock == Blocks.Air;
            
            if (!isVisible)
            {
                return;
            }

            var size = _vertexList.Count;
            _vertexList.Add(currentPosition + Vector3.forward + Vector3.right);                   // + 0
            _vertexList.Add(currentPosition + Vector3.forward);                                   // + 1
            _vertexList.Add(currentPosition + Vector3.forward + Vector3.right + Vector3.up);      // + 2
            _vertexList.Add(currentPosition + Vector3.forward + Vector3.up);                      // + 3

            AddFaceNormals(Vector3.forward);
            AddTriangles(size);
            AddUVs(block, Sides.Back);
        }

        private void HandleFront(ref Blocks block, ref Blocks frontBlock, ref Vector3 currentPosition)
        {
            if (block == Blocks.Water)
            {
                return;
            }
            
            var blockData = VoxelHandler.instance.blockData[block.ToString()];
            var frontBlockData = VoxelHandler.instance.blockData[frontBlock.ToString()];

            var isVisible = !blockData.transparent && frontBlockData.transparent || frontBlock == Blocks.Air;

            if (!isVisible)
            {
                return;
            }

            var size = _vertexList.Count;
            _vertexList.Add(currentPosition);                                          // + 0
            _vertexList.Add(currentPosition + Vector3.right);                          // + 1
            _vertexList.Add(currentPosition + Vector3.up);                             // + 2
            _vertexList.Add(currentPosition + Vector3.right + Vector3.up);             // + 3

            AddFaceNormals(Vector3.back);
            AddTriangles(size);
            AddUVs(block, Sides.Front);
        }
    }
}
