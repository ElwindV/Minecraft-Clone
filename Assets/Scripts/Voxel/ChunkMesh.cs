using System.Collections.Generic;
using UnityEngine;

public class ChunkMesh : MonoBehaviour
{
    private MeshFilter meshFilter;
    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    private GameObject waterObject;
    private MeshFilter waterMeshFilter;
    private Mesh waterMesh;
    private MeshRenderer waterMeshRenderer;

    private Chunk chunk;
    private Material material;

    private Material waterMaterial;

    private List<Vector3> vertexList;
    private List<int> triangleList;
    private List<Vector3> normalList;
    private List<Vector2> uvList;

    private List<Vector3> waterVertexList;
    private List<int> waterTriangleList;
    private List<Vector3> waterNormalList;
    private List<Vector2> waterUvList;

    [HideInInspector]
    public Chunk leftChunk;
    [HideInInspector]
    public Chunk rightChunk;
    [HideInInspector]
    public Chunk frontChunk;
    [HideInInspector]
    public Chunk backChunk;

    private void Start()
    {
        Setup();
        Refresh();
    }

    private void Setup()
    {
        meshFilter = this.gameObject.AddComponent<MeshFilter>();
        meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        meshCollider = this.gameObject.AddComponent<MeshCollider>();

        waterObject = new GameObject();
        waterObject.transform.parent = this.transform;
        waterObject.name = "Water";
        waterObject.transform.localPosition = Vector3.zero;
        waterMeshFilter = waterObject.AddComponent<MeshFilter>();
        waterMeshRenderer = waterObject.AddComponent<MeshRenderer>();

        chunk = this.gameObject.GetComponent<Chunk>();

        material = Resources.Load<Material>("Materials/Voxel");
        meshRenderer.sharedMaterial = this.material;
        
        waterMaterial = Resources.Load<Material>("Materials/Water");
        waterMeshRenderer.sharedMaterial = this.waterMaterial;

        leftChunk = (chunk.x - 1 >= 0)
            ? VoxelHandler.instance.chunks[chunk.x - 1, chunk.z].GetComponent<Chunk>()
            : null;
        rightChunk = (chunk.x + 1 < VoxelHandler.instance.chunks.GetLength(0))
            ? VoxelHandler.instance.chunks[chunk.x + 1, chunk.z].GetComponent<Chunk>()
            : null;
        backChunk = (chunk.z + 1 < VoxelHandler.instance.chunks.GetLength(1))
            ? VoxelHandler.instance.chunks[chunk.x, chunk.z + 1].GetComponent<Chunk>()
            : null;
        frontChunk = (chunk.z - 1 >= 0)
            ? VoxelHandler.instance.chunks[chunk.x, chunk.z - 1].GetComponent<Chunk>()
            : null;
    }

    public void Refresh()
    {
        var mesh = new Mesh();
        var waterMesh = new Mesh();

        vertexList = new List<Vector3>();
        triangleList = new List<int>();
        normalList = new List<Vector3>();
        uvList = new List<Vector2>();
        
        waterVertexList = new List<Vector3>();
        waterTriangleList = new List<int>();
        waterNormalList = new List<Vector3>();
        waterUvList = new List<Vector2>();

        for (int x = 0; x < chunk.blocks.GetLength(0); x++)
        {
            for (int y = 0; y < chunk.blocks.GetLength(1); y++)
            {
                for (int z = 0; z < chunk.blocks.GetLength(2); z++)
                {
                    byte block = chunk.blocks[x, y, z];
                    var currentPosition = new Vector3(x, y, z);

                    if (block == (byte)Blocks.Air)
                    {
                        continue;
                    }

                    byte rightBlock = ((x + 1 < chunk.blocks.GetLength(0)) ? chunk.blocks[x + 1, y, z]
                        : ((rightChunk != null) ? rightChunk.blocks[0, y, z] : (byte)Blocks.Air));
                    byte leftBlock = ((x - 1 >= 0) ? chunk.blocks[x - 1, y, z]
                        : ((leftChunk != null) ? leftChunk.blocks[leftChunk.ChunkWidth - 1, y, z] : (byte)Blocks.Air));

                    byte topBlock = ((y + 1 < chunk.blocks.GetLength(1)) ? chunk.blocks[x, y + 1, z] : (byte)Blocks.Air);
                    byte bottomBlock = ((y - 1 >= 0) ? chunk.blocks[x, y - 1, z] : (byte)Blocks.Air);

                    byte backBlock = ((z + 1 < chunk.blocks.GetLength(2)) ? chunk.blocks[x, y, z + 1]
                        : ((backChunk != null) ? backChunk.blocks[x, y, 0] : (byte)Blocks.Air));
                    byte frontBlock = ((z - 1 >= 0) ? chunk.blocks[x, y, z - 1]
                        : ((frontChunk != null) ? frontChunk.blocks[x, y, frontChunk.ChunkDepth - 1] : (byte)Blocks.Air));

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

        meshFilter.mesh = mesh;
        mesh.vertices = vertexList.ToArray();
        mesh.triangles = triangleList.ToArray();
        mesh.normals = normalList.ToArray();
        mesh.uv = uvList.ToArray();
        meshCollider.sharedMesh = mesh;
        
        waterMeshFilter.mesh = waterMesh;
        waterMesh.vertices = waterVertexList.ToArray();
        waterMesh.triangles = waterTriangleList.ToArray();
        waterMesh.normals = waterNormalList.ToArray();
        waterMesh.uv = waterUvList.ToArray();
    }

    private void AddFaceNormals(Vector3 direction, bool isWater = false)
    {
        for (short i = 0; i < 4; i++)
        {
            if (isWater)
            {
                waterNormalList.Add(direction);
                continue;
            }

            normalList.Add(direction);
        }
    }

    private void AddTriangles(int baseVertexNumber, bool isWater = false)
    {
        if (isWater)
        {
            waterTriangleList.Add(baseVertexNumber + 0);
            waterTriangleList.Add(baseVertexNumber + 3);
            waterTriangleList.Add(baseVertexNumber + 1);

            waterTriangleList.Add(baseVertexNumber + 0);
            waterTriangleList.Add(baseVertexNumber + 2);
            waterTriangleList.Add(baseVertexNumber + 3);
            
            return;
        }

        triangleList.Add(baseVertexNumber + 0);
        triangleList.Add(baseVertexNumber + 3);
        triangleList.Add(baseVertexNumber + 1);

        triangleList.Add(baseVertexNumber + 0);
        triangleList.Add(baseVertexNumber + 2);
        triangleList.Add(baseVertexNumber + 3);
    }

    private void AddUVs(byte blockByte, Sides side, bool isWater = false)
    {
        if (isWater)
        {
            waterUvList.Add(Vector2.zero);
            waterUvList.Add(Vector2.right);
            waterUvList.Add(Vector2.up);
            waterUvList.Add(Vector2.right + Vector2.up);

            return;
        }

        Vector2 textureStart;

        Block block = VoxelHandler.instance.blockData[((Blocks)blockByte).ToString()];
        string texture = (side == Sides.Top)
            ? block.textures.top
            : (side == Sides.Bottom)
                ? block.textures.bottom
                : block.textures.side;
        textureStart = Atlas.uvs[texture];

        float divider = 1f / (Atlas.dimensions);
        uvList.Add(textureStart + divider * Vector2.zero);
        uvList.Add(textureStart + divider * Vector2.right);
        uvList.Add(textureStart + divider * Vector2.up);
        uvList.Add(textureStart + divider * Vector2.right + divider * Vector2.up);
    }

    private void HandleRight(ref byte blockByte, ref byte rightBlockByte, ref Vector3 currentPosition)
    {
        Block block = VoxelHandler.instance.blockData[((Blocks)blockByte).ToString()];
        Block rightBlock = VoxelHandler.instance.blockData[((Blocks)rightBlockByte).ToString()];

        bool isVisible = false;

        if (blockByte == (byte) Blocks.Water)
        {
            return;
        }

        if (!block.transparant && rightBlock.transparant)
        {
            isVisible = true;
        }
        if (rightBlockByte == (byte)Blocks.Air)
        {
            isVisible = true;
        }

        if (!isVisible)
        {
            return;
        }

        int size = vertexList.Count;
        vertexList.Add(currentPosition + Vector3.right);                                    // + 0
        vertexList.Add(currentPosition + Vector3.right + Vector3.forward);                  // + 1
        vertexList.Add(currentPosition + Vector3.right + Vector3.up);                       // + 2
        vertexList.Add(currentPosition + Vector3.right + Vector3.forward + Vector3.up);     // + 3

        AddFaceNormals(Vector3.right);
        AddTriangles(size);
        AddUVs(blockByte, Sides.Right);
    }

    private void HandleLeft(ref byte blockByte, ref byte leftBlockByte, ref Vector3 currentPosition)
    {
        Block block = VoxelHandler.instance.blockData[((Blocks)blockByte).ToString()];
        Block leftBlock = VoxelHandler.instance.blockData[((Blocks)leftBlockByte).ToString()];

        bool isVisible = false;

        if (blockByte == (byte) Blocks.Water)
        {
            return;
        }
        
        if (!block.transparant && leftBlock.transparant)
        {
            isVisible = true;
        }
        if (leftBlockByte == (byte)Blocks.Air)
        {
            isVisible = true;
        }

        if (!isVisible)
        {
            return;
        }

        int size = vertexList.Count;
        vertexList.Add(currentPosition + Vector3.forward);                  // + 0
        vertexList.Add(currentPosition);                                    // + 1
        vertexList.Add(currentPosition + Vector3.forward + Vector3.up);     // + 2
        vertexList.Add(currentPosition + Vector3.up);                       // + 3

        AddFaceNormals(Vector3.left);
        AddTriangles(size);
        AddUVs(blockByte, Sides.Left);
    }

    private void HandleTop(ref byte blockByte, ref byte topBlockByte, ref Vector3 currentPosition)
    {
        Block block = VoxelHandler.instance.blockData[((Blocks)blockByte).ToString()];
        Block topBlock = VoxelHandler.instance.blockData[((Blocks)topBlockByte).ToString()];

        bool isVisible = false;

        if (!block.transparant && topBlock.transparant)
        {
            isVisible = true;
        }
        if (topBlockByte == (byte)Blocks.Air)
        {
            isVisible = true;
        }

        if (!isVisible)
        {
            return;
        }

        if (blockByte == (byte) Blocks.Water)
        {
            int waterSize = waterVertexList.Count;
            
            waterVertexList.Add(currentPosition + Vector3.up);                                    // + 0
            waterVertexList.Add(currentPosition + Vector3.up + Vector3.right);                    // + 1
            waterVertexList.Add(currentPosition + Vector3.up + Vector3.forward);                  // + 2
            waterVertexList.Add(currentPosition + Vector3.up + Vector3.right + Vector3.forward);  // + 3
            
            AddFaceNormals(Vector3.up, true);
            AddTriangles(waterSize, true);
            AddUVs(blockByte, Sides.Top, true);
            
            return;
        }
        
        int size = vertexList.Count;
        vertexList.Add(currentPosition + Vector3.up);                                    // + 0
        vertexList.Add(currentPosition + Vector3.up + Vector3.right);                    // + 1
        vertexList.Add(currentPosition + Vector3.up + Vector3.forward);                  // + 2
        vertexList.Add(currentPosition + Vector3.up + Vector3.right + Vector3.forward);  // + 3

        AddFaceNormals(Vector3.up);
        AddTriangles(size);
        AddUVs(blockByte, Sides.Top);
    }

    private void HandleBottom(ref byte blockByte, ref byte bottomBlockByte, ref Vector3 currentPosition)
    {
        Block block = VoxelHandler.instance.blockData[((Blocks)blockByte).ToString()];
        Block bottomBlock = VoxelHandler.instance.blockData[((Blocks)bottomBlockByte).ToString()];

        bool isVisible = false;

        if (blockByte == (byte) Blocks.Water)
        {
            return;
        }
        
        if (!block.transparant && bottomBlock.transparant)
        {
            isVisible = true;
        }
        if (bottomBlockByte == (byte)Blocks.Air)
        {
            isVisible = true;
        }

        if (!isVisible)
        {
            return;
        }

        int size = vertexList.Count;
        vertexList.Add(currentPosition + Vector3.right);                    // + 0
        vertexList.Add(currentPosition);                                          // + 1
        vertexList.Add(currentPosition + Vector3.right + Vector3.forward);  // + 2
        vertexList.Add(currentPosition + Vector3.forward);                  // + 3

        AddFaceNormals(Vector3.down);
        AddTriangles(size);
        AddUVs(blockByte, Sides.Bottom);
    }

    private void HandleBack(ref byte blockByte, ref byte backBlockByte, ref Vector3 currentPosition)
    {
        Block block = VoxelHandler.instance.blockData[((Blocks)blockByte).ToString()];
        Block backBlock = VoxelHandler.instance.blockData[((Blocks)backBlockByte).ToString()];

        bool isVisible = false;

        if (blockByte == (byte) Blocks.Water)
        {
            return;
        }
        
        if (!block.transparant && backBlock.transparant)
        {
            isVisible = true;
        }
        if (backBlockByte == (byte)Blocks.Air)
        {
            isVisible = true;
        }

        if (!isVisible)
        {
            return;
        }

        int size = vertexList.Count;
        vertexList.Add(currentPosition + Vector3.forward + Vector3.right);                   // + 0
        vertexList.Add(currentPosition + Vector3.forward);                                   // + 1
        vertexList.Add(currentPosition + Vector3.forward + Vector3.right + Vector3.up);      // + 2
        vertexList.Add(currentPosition + Vector3.forward + Vector3.up);                      // + 3

        AddFaceNormals(Vector3.forward);
        AddTriangles(size);
        AddUVs(blockByte, Sides.Back);
    }

    private void HandleFront(ref byte blockByte, ref byte frontBlockByte, ref Vector3 currentPosition)
    {
        Block block = VoxelHandler.instance.blockData[((Blocks)blockByte).ToString()];
        Block frontBlock = VoxelHandler.instance.blockData[((Blocks)frontBlockByte).ToString()];

        bool isVisible = false;

        if (blockByte == (byte) Blocks.Water)
        {
            return;
        }
        
        if (!block.transparant && frontBlock.transparant)
        {
            isVisible = true;
        }
        if (frontBlockByte == (byte)Blocks.Air)
        {
            isVisible = true;
        }

        if (!isVisible)
        {
            return;
        }

        int size = vertexList.Count;
        vertexList.Add(currentPosition);                                 // + 0
        vertexList.Add(currentPosition + Vector3.right);                 // + 1
        vertexList.Add(currentPosition + Vector3.up);                    // + 2
        vertexList.Add(currentPosition + Vector3.right + Vector3.up);    // + 3

        AddFaceNormals(Vector3.back);
        AddTriangles(size);
        AddUVs(blockByte, Sides.Front);
    }
}
