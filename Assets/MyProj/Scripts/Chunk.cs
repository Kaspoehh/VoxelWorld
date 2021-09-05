using System;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {

	public MeshRenderer meshRenderer;
	public MeshFilter meshFilter;
	public float Seed;
	int vertexIndex = 0;
	List<Vector3> vertices = new List<Vector3> ();
	List<int> triangles = new List<int> ();
	List<Vector2> uvs = new List<Vector2> ();

	byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
	// private List<VoxelMapData> voxelList = new List<VoxelMapData>();
	
	World world;
	private Camera _cam;
	
	void Start () {
		world = GameObject.Find("World").GetComponent<World>();
		_cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
		PopulateVoxelMap ();
		CreateMeshData ();
		CreateMesh ();
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
			
			if (Physics.Raycast(ray, out hit))
			{
				Debug.Log(hit.point);
				int x = Mathf.FloorToInt (hit.point.x);
				int y = Mathf.FloorToInt (hit.point.y);
				int z = Mathf.FloorToInt (hit.point.z);
				
				//Debug.Log(voxelMap[x,y,z]);
			}
		}
	}


	void PopulateVoxelMap () {
		for (int y = 0; y < VoxelData.ChunkHeight; y++) {
			for (int x = 0; x < VoxelData.ChunkWidth; x++) {
				for (int z = 0; z < VoxelData.ChunkWidth; z++) {
					
					int terrainHeight = Mathf.FloorToInt(16 * Get2DPerlin(new Vector2(this.transform.position.x + x,  this.transform.position.z + z),
						Seed, 1));

					terrainHeight = Math.Abs(terrainHeight);
					terrainHeight += 50;
					
					//Debug.Log(terrainHeight);
					
					try
					{
						int caveStartingPoint = terrainHeight - 25;
						int caveEndPoint = 40 + Mathf.FloorToInt(16 * Get2DPerlin(new Vector2(this.transform.position.x + x,  this.transform.position.z + z),
							12, 1));

						
						voxelMap[x, terrainHeight, z] = 2;
						
						for (int i = 0; i < VoxelData.ChunkHeight; i++)
						{
							if (terrainHeight != i)
							{
								if (i == 0)
								{
									//Create Bedrock add the bottom
									voxelMap[x, i, z] = 0;
								}
								else
								{
									if(i > caveStartingPoint)
									{
										voxelMap[x, i, z] = 1; //Create Stone
									}

									if (i < caveEndPoint)
									{
										voxelMap[x, i, z] = 1; //Create Stone

									}
									//	Debug.Log(noiseValue);
									if (i < caveStartingPoint && i > caveEndPoint)
									{
										float noiseScale = .05F;
										// float noiseValue = Perlin3D((this.transform.position.x + x),
										// 	(this.transform.position.y + y), (this.transform.position.z + z), .05F,
										// 	50); //get value of the noise at given x, y, and z.
										float noiseValue = Perlin3D(this.transform.position.x + x * noiseScale, this.transform.position.y + y * noiseScale, this.transform.position.z + z * noiseScale);
										float threshold = .5f;
										if (noiseValue >= threshold)
										{
											voxelMap[x, i, z] = 4; //Create Air
										}
										else
										{
											voxelMap[x, i, z] = 1; //Create Stone
										}
									}
									if (i > terrainHeight)
									{
										voxelMap[x, i, z] = 4; //Create Air block above the ground
									}
								}
							}
						}
					}
					catch (IndexOutOfRangeException e)  // CS0168
					{
						Debug.Log(e.Message);
						Debug.Log("trying to get: " + x + terrainHeight + z);
						// Set IndexOutOfRangeException to the new exception's InnerException.
						throw new ArgumentOutOfRangeException("index parameter is out of range.", e);
					}
					// if (terrainHeight < 1)                                         
                    //     voxelMap[x, terrainHeight, z] = 0;                         
                    // else if (terrainHeight == VoxelData.ChunkHeight - 1)           
                    //     voxelMap[x, terrainHeight, z] = 2;                         
                    // else
                    //     voxelMap [x, terrainHeight, z] = 1;
    
				}
			}
		}

	}

	void CreateMeshData () {

		for (int y = 0; y < VoxelData.ChunkHeight; y++) {
			for (int x = 0; x < VoxelData.ChunkWidth; x++) {
				for (int z = 0; z < VoxelData.ChunkWidth; z++) {
					AddVoxelDataToChunk (new Vector3(x, y, z));
				}
			}
		}

	}

	bool CheckVoxel (Vector3 pos) {

		int x = Mathf.FloorToInt (pos.x);
		int y = Mathf.FloorToInt (pos.y);
		int z = Mathf.FloorToInt (pos.z);

		if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
		{
			return false; 
		}

		return world.blocktypes[voxelMap[x, y, z]].isSolid;

	}

	void AddVoxelDataToChunk (Vector3 pos) {

		for (int p = 0; p < 6; p++) {
			if (!CheckVoxel(pos + VoxelData.faceChecks[p])) {
                byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
                
                if (world.blocktypes[voxelMap[(int) pos.x, (int) pos.y, (int) pos.z]].isSolid)       
                {                                                                                    
					vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 0]]);
					vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 1]]);
					vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 2]]);
					vertices.Add (pos + VoxelData.voxelVerts [VoxelData.voxelTris [p, 3]]);
					AddTexture(world.blocktypes[blockID].GetTextureID(p));
					triangles.Add(vertexIndex);
					triangles.Add(vertexIndex + 1);
					triangles.Add(vertexIndex + 2);
					triangles.Add(vertexIndex + 2);
					triangles.Add(vertexIndex + 1);
					triangles.Add(vertexIndex + 3);

					vertexIndex += 4;
				}
			}
		}
	}

	void CreateMesh () {

		Mesh mesh = new Mesh ();
		mesh.vertices = vertices.ToArray ();
		mesh.triangles = triangles.ToArray ();
		mesh.uv = uvs.ToArray ();
		
		mesh.RecalculateNormals ();
		
		
		meshFilter.mesh = mesh;
		
		MeshCollider myMC = GetComponent<MeshCollider>();
		
		
		mesh.RecalculateBounds();
		myMC.sharedMesh = mesh;

	}

    void AddTexture (int textureID) {

        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));


    }
    
    public static float Get2DPerlin (Vector2 position, float offset, float scale) {
	    return Mathf.PerlinNoise((position.x + 0.1f) / 16 * scale + offset, (position.y + 0.1f) / 16 * scale + offset);
    }
    //
    // public static float Perlin3D(float x, float y, float z, float scale, float offset) {
	   //  // float ab = Mathf.PerlinNoise(x, y);
	   //  // float bc = Mathf.PerlinNoise(y, z);
	   //  // float ac = Mathf.PerlinNoise(x, z);
	   //  //
	   //  // float ba = Mathf.PerlinNoise(y, x);
	   //  // float cb = Mathf.PerlinNoise(z, y);
	   //  // float ca = Mathf.PerlinNoise(z, x);
	   //  //
	   //  // float abc = ab + bc + ac + ba + cb + ca;
	   //  // return abc / 6f;
	   //  float AB = Mathf.PerlinNoise((x + 0.1f) / 16 * scale + offset, (y + 0.1f) / 16 * scale + offset);
	   //  float BC = Mathf.PerlinNoise((y + 0.1f) / 16 * scale + offset, (z + 0.1f) / 16 * scale + offset);
	   //  float AC = Mathf.PerlinNoise((x + 0.1f) / 16 * scale + offset, (z + 0.1f) / 16 * scale + offset);
    //     
	   //  float BA = Mathf.PerlinNoise((y + 0.1f) / 16 * scale + offset, (x + 0.1f) / 16 * scale + offset);
	   //  float CB = Mathf.PerlinNoise((z + 0.1f) / 16 * scale + offset, (y + 0.1f) / 16 * scale + offset);
	   //  float CA = Mathf.PerlinNoise((z + 0.1f) / 16 * scale + offset, (x + 0.1f) / 16 * scale + offset);
    //
	   //  float ABC = AB + BC + AC + BA + CB + CA;
    //
	   //  return ABC / 6F;
    // }

    public static float Perlin3D(float x, float y, float z) {
	    float ab = Mathf.PerlinNoise(x, y);
	    float bc = Mathf.PerlinNoise(y, z);
	    float ac = Mathf.PerlinNoise(x, z);

	    float ba = Mathf.PerlinNoise(y, x);
	    float cb = Mathf.PerlinNoise(z, y);
	    float ca = Mathf.PerlinNoise(z, x);

	    float abc = ab + bc + ac + ba + cb + ca;
	    return abc / 6f;
    }
}
