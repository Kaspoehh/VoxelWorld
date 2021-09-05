using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Chunk : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    [HideInInspector] public World world;

    private int _vertexIndex = 0;
    private List<Vector3> _vertices = new List<Vector3>();
    private List<int> _triangles = new List<int>();
    private List<Vector2> _uvs = new List<Vector2>();
    private Mesh _mesh;
    private Dictionary<Vector3, int> _voxelMap = new Dictionary<Vector3, int>();
    public Dictionary<Vector3, int> voxelMap => _voxelMap;


    private void Start()
    {
        _mesh = new Mesh();

        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();

    }

    //Step (1)
    private void PopulateVoxelMap()
    {
        for (int y = 0; y < ChunkData.chunkHeight; y++)
        {
            for (int x = 0; x < ChunkData.chunkWidth; x++)
            {
                for (int z = 0; z < ChunkData.chunkWidth; z++)
                {
                    _voxelMap.Add(new Vector3(x, y, z), 1);
                }
            }
        }
    }

    //Step (2)
    private void CreateMeshData()
    {
        for (int y = 0; y < ChunkData.chunkHeight; y++)
        {
            for (int x = 0; x < ChunkData.chunkWidth; x++)
            {
                for (int z = 0; z < ChunkData.chunkWidth; z++)
                {
                    AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    private void AddVoxelDataToChunk(Vector3 voxelPos)
    {
        for (int p = 0; p < 6; p++)
        {
            if (!world.CheckVoxel(voxelPos + VoxelData.FaceChecks[p], this))
            {
                int blockID = 0;
                
                if (_voxelMap.TryGetValue(voxelPos, out blockID))
                {
                    if (world.BlockTypesList[blockID].isSolid)
                    {
                        _vertices.Add(voxelPos + VoxelData.VoxelVerts[VoxelData.VoxelTris[p, 0]]);
                        _vertices.Add(voxelPos + VoxelData.VoxelVerts[VoxelData.VoxelTris[p, 1]]);
                        _vertices.Add(voxelPos + VoxelData.VoxelVerts[VoxelData.VoxelTris[p, 2]]);
                        _vertices.Add(voxelPos + VoxelData.VoxelVerts[VoxelData.VoxelTris[p, 3]]);
                        AddTexture(world.BlockTypesList[blockID].GetTextureID(p));
                        _triangles.Add(_vertexIndex);
                        _triangles.Add(_vertexIndex + 1);
                        _triangles.Add(_vertexIndex + 2);
                        _triangles.Add(_vertexIndex + 2);
                        _triangles.Add(_vertexIndex + 1);
                        _triangles.Add(_vertexIndex + 3);
                        _vertexIndex += 4;
                    }
                }
            }
        }

        //STEP (4)
        void CreateMesh()
        {
            _mesh.vertices = _vertices.ToArray();
            _mesh.triangles = _triangles.ToArray();
            _mesh.uv = _uvs.ToArray();
            _mesh.RecalculateNormals();
            meshFilter.mesh = _mesh;
            MeshCollider myMC = GetComponent<MeshCollider>();
            _mesh.RecalculateBounds();
            myMC.sharedMesh = _mesh;
        }

    }
    
    void AddTexture (int textureID) {

        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        _uvs.Add(new Vector2(x, y));
        _uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        _uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        _uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }

    private void CreateMesh()
    {
        _mesh.vertices = _vertices.ToArray ();
        _mesh.triangles = _triangles.ToArray ();
        _mesh.uv = _uvs.ToArray ();
        _mesh.RecalculateNormals ();
        meshFilter.mesh = _mesh;
        MeshCollider myMC = GetComponent<MeshCollider>();
        _mesh.RecalculateBounds();
        myMC.sharedMesh = _mesh; 
    }

}

public class VoxelData
{
    public static readonly int TextureAtlasSizeInBlocks = 4;

    public static float NormalizedBlockTextureSize
    {

        get { return 1f / (float) TextureAtlasSizeInBlocks; }

    }

    public static readonly Vector3[] VoxelVerts = new Vector3[8]
    {

        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),

    };

    public static readonly Vector3[] FaceChecks = new Vector3[6]
    {

        new Vector3(0.0f, 0.0f, -1.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, -1.0f, 0.0f),
        new Vector3(-1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f)

    };

    public static readonly int[,] VoxelTris = new int[6, 4]
    {

        // Back, Front, Top, Bottom, Left, Right

        // 0 1 2 2 1 3
        {0, 3, 1, 2}, // Back Face
        {5, 6, 4, 7}, // Front Face
        {3, 7, 2, 6}, // Top Face
        {1, 5, 0, 4}, // Bottom Face
        {4, 7, 0, 3}, // Left Face
        {1, 2, 5, 6} // Right Face

    };

    public static readonly Vector2[] VoxelUvs = new Vector2[4]
    {

        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f)

    };
}


