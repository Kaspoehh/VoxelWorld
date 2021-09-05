using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public List<Chunk> activeChunks = new List<Chunk>();
    [SerializeField] private List<BlockTypes> blockTypesList = new List<BlockTypes>();
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private Vector2 startWorldSize;

    public List<BlockTypes> BlockTypesList => blockTypesList;
    
    private void Start()
    {
        for (int y = 0; y < startWorldSize.y; y++)
        {
            for (int x = 0; x < startWorldSize.x; x++)
            {
                Chunk chunk = Instantiate(chunkPrefab).GetComponent<Chunk>();
                chunk.transform.position = new Vector3(x * ChunkData.chunkWidth, 0, y * ChunkData.chunkWidth);
                chunk.world = this;
                activeChunks.Add(chunk);
            }
        }
    }
    
    public bool CheckVoxel (Vector3 pos, Chunk chunk) {

        int x = Mathf.FloorToInt (pos.x);
        int y = Mathf.FloorToInt (pos.y);
        int z = Mathf.FloorToInt (pos.z);

        if (x < 0 || x > ChunkData.chunkWidth - 1 || y < 0 || y > ChunkData.chunkHeight - 1 || z < 0 || z > ChunkData.chunkWidth - 1)
        {
            return false; 
        }

        int blockID = 0;

        if (chunk.voxelMap.TryGetValue(pos, out blockID))
        {
            return BlockTypesList[blockID].isSolid;	
        }

        return false;
        //return world.blocktypes[voxelMap[x, y, z]].isSolid;
    }

}

public static class ChunkData
{
    public const int chunkHeight = 64;
    public const int chunkWidth = 16;
}

[System.Serializable]
public class BlockTypes
{

    public string blockName;
    public bool isSolid;

    [Header("Texture Values")] public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // Back, Front, Top, Bottom, Left, Right

    public int GetTextureID(int faceIndex)
    {

        switch (faceIndex)
        {

            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
        }

    }

}
