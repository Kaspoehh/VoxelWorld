using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{   
    

    private List<GameObject> _activeChunks = new List<GameObject>();
    
    public Material material;
    public BlockType[] blocktypes;

    [Header("World Settings")] [SerializeField]
    private GameObject chunkPrefab; 
    [SerializeField] private Vector2 worldStartSize = new Vector2(20, 20);
    [SerializeField] private float seed = 20;

    private void Start()
    {
        StartCoroutine(CreateChunks());
    }

    private IEnumerator CreateChunks()
    {
        for (int y = 0; y < worldStartSize.y; y++)
        {
            for (int x = 0; x < worldStartSize.x; x++)
            {
                Vector2 position = new Vector2(x * VoxelData.ChunkWidth, y * VoxelData.ChunkWidth);
                _activeChunks.Add(CreateChunk(position));
                yield return new WaitForSeconds(.1F);
            }
        }
    }
    private GameObject CreateChunk(Vector2 position)
    {
        var chunk = Instantiate(chunkPrefab);
        chunk.name = "Chunk: " + position;
        chunk.transform.position = new Vector3(position.x, 0, position.y);
        chunk.GetComponent<Chunk>().Seed = seed;
        return chunk;
    }
    
}

[System.Serializable]
public class BlockType {

    public string blockName;
    public bool isSolid;
        
    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // Back, Front, Top, Bottom, Left, Right

    public int GetTextureID (int faceIndex) {

        switch (faceIndex) {

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
