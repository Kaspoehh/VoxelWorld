using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class World : MonoBehaviour
{
    public Dictionary<ChunkPos, Chunk> activeChunks = new Dictionary<ChunkPos, Chunk>();
    [SerializeField] private List<BlockTypes> blockTypesList = new List<BlockTypes>();
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private Vector2 startWorldSize;
    [SerializeField] private GameObject player;
    [SerializeField] private float renderDistance = 32;

    public List<BlockTypes> BlockTypesList => blockTypesList;

    private float chunkCheckTimer = 5;
    private List<Chunk> _pooledChunks = new List<Chunk>();

    private void Start()
    {
        for (int y = 0; y < startWorldSize.y; y++)
        {
            for (int x = 0; x < startWorldSize.x; x++)
            {
                CreateChunk(new Vector2(x * ChunkData.chunkWidth, y * ChunkData.chunkWidth));
            }
        }

        float posX = startWorldSize.x * ChunkData.chunkWidth / 2;
        float posY = 4 + ChunkData.chunkHeight;
        float posZ = startWorldSize.x * ChunkData.chunkWidth / 2;

        player.transform.position = new Vector3(posX, posY, posZ);
    }

    private void Update()
    {
        CheckForNewChunk();
    }

    private void CreateChunk(Vector2 chunkSpawnPos)
    {
        Chunk chunk = new Chunk();
        if (_pooledChunks.Count > 0)
        {
            chunk = _pooledChunks[0];
            chunk.gameObject.SetActive(true);
            _pooledChunks.RemoveAt(0);
            chunk.transform.position = new Vector3(chunkSpawnPos.x, 0, chunkSpawnPos.y);
            
        }
        else
        {
            chunk = Instantiate(chunkPrefab).GetComponent<Chunk>();
            chunk.transform.position = new Vector3(chunkSpawnPos.x, 0, chunkSpawnPos.y);
            chunk.world = this;
        }
            
        chunk.name = "Chunk: " + chunk.transform.position;
        ChunkPos chunkPos = new ChunkPos((int)chunkSpawnPos.x, (int)chunkSpawnPos.y);
        activeChunks.Add(chunkPos, chunk);
    }

    public bool CheckVoxel(Vector3 pos, Chunk chunk)
    {

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (x < 0 || x > ChunkData.chunkWidth - 1 || y < 0 || y > ChunkData.chunkHeight - 1 || z < 0 ||
            z > ChunkData.chunkWidth - 1)
        {
            return false;
        }

        int blockID = 0;

        if (chunk.voxelMap.TryGetValue(pos, out blockID))
        {
            return BlockTypesList[blockID].isSolid;
        }

        return false;
    }


    private int curChunkX;
    private int curChunkY;
    private Chunk _currentChunk;
    private Chunk _lastChunk;
    
    private void CheckForNewChunk()
    {
        curChunkX = Mathf.FloorToInt(player.transform.position.x / 16) * 16;
        curChunkY = Mathf.FloorToInt(player.transform.position.z / 16) * 16;
            
        if(activeChunks.TryGetValue(new ChunkPos(curChunkX, curChunkY), out _currentChunk))
        {
            if (_currentChunk != _lastChunk || _currentChunk == null)
            {
                _lastChunk = _currentChunk;
                
                for(int i = curChunkX - 16 * 2; i <= curChunkX + 16 * 2; i += 16)
                    for(int j = curChunkY - 16 * 2; j <= curChunkY + 16 * 2; j += 16)
                    {
                        ChunkPos cp = new ChunkPos(i, j);

                        if(!activeChunks.ContainsKey(cp) )//&& !toGenerate.Contains(cp))
                        {
                            Debug.Log("Creating Chunk!");
                            CreateChunk(new Vector2(i, j));
                        }
                    }
                
                //remove chunks that are too far away
                List<ChunkPos> toDestroy = new List<ChunkPos>();
                //unload chunks
                foreach(KeyValuePair<ChunkPos, Chunk> c in activeChunks)
                {
                    ChunkPos cp = c.Key;
                    if(Mathf.Abs(curChunkX - cp.x) > 16 * (2 + 3) || 
                       Mathf.Abs(curChunkY - cp.z) > 16 * (2 + 3))
                    {
                        toDestroy.Add(c.Key);
                    }
                }
                
                foreach (ChunkPos cp in toDestroy)
                {
                    activeChunks[cp].gameObject.SetActive(false);
                    _pooledChunks.Add(activeChunks[cp]);
                    activeChunks.Remove(cp);
                }
            }
        }
    }
}

public static class ChunkData
{
    public static readonly int chunkHeight = 64;
    public static readonly int chunkWidth = 16;
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

public struct ChunkPos
{
    public int x, z;
    public ChunkPos(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
}
