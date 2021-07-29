using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int seed;
    public BiomeAttributes biome;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public BlockType[] blockTypes;
    
    Chunk[,] chunks = new Chunk[VoxelData.worldSizeInChunks, VoxelData.worldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    bool isCreatingChunks;

    private void Start() 
    {
        Random.InitState(seed);

        spawnPosition = new Vector3(
            (VoxelData.worldSizeInChunks * VoxelData.chunkWidth) / 2f, 
            VoxelData.chunkHeight - 50f, 
            (VoxelData.worldSizeInChunks * VoxelData.chunkWidth) / 2f
        );

        generateWorld();
        
        player.position = spawnPosition;
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    private void Update() 
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        
        if (!playerChunkCoord.Equals(playerLastChunkCoord)) {
            checkViewDistance();
        }

        if (chunksToCreate.Count > 0 && !isCreatingChunks)
            StartCoroutine(createChunks());
    }

    /*
        Cria todos os chunks dentro do world Size
    */
    void generateWorld() 
    {
        for (int x = (VoxelData.worldSizeInChunks / 2) - VoxelData.viewDistanceInChunks; x < (VoxelData.worldSizeInChunks / 2) + VoxelData.viewDistanceInChunks; x++) {
            for (int z = (VoxelData.worldSizeInChunks / 2) - VoxelData.viewDistanceInChunks; z < (VoxelData.worldSizeInChunks / 2) + VoxelData.viewDistanceInChunks; z++) {
                
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                activeChunks.Add(new ChunkCoord(x, z));

            }
        }
    }

    IEnumerator createChunks()
    {
        isCreatingChunks = true;
        while (chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].z].init();
            chunksToCreate.RemoveAt(0);
            yield return null;

        }
    }

    /*
        Saber que chunk é que esta em uma determinada posiçao
    */
    ChunkCoord GetChunkCoordFromVector3(Vector3 pos) 
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.chunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.chunkWidth);

        return new ChunkCoord(x, z);
    }


    void checkViewDistance() 
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        playerLastChunkCoord = playerChunkCoord;

        for (int x = (coord.x - VoxelData.viewDistanceInChunks); x < (coord.x + VoxelData.viewDistanceInChunks); x++) {
            for (int z = (coord.z - VoxelData.viewDistanceInChunks); z < (coord.z + VoxelData.viewDistanceInChunks); z++) {
                
                if (isChunkInWorld(new ChunkCoord(x, z)))
                {
                    if (chunks[x, z] == null) {
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        chunksToCreate.Add(new ChunkCoord(x, z));
                    }
                    else if (!chunks[x, z].isActive) 
                    {
                        chunks[x, z].isActive = true;
                        
                    }
                    activeChunks.Add(new ChunkCoord(x, z));
                }

                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                        previouslyActiveChunks.RemoveAt(i);

                }

            }
        }

        foreach (ChunkCoord c in previouslyActiveChunks)
            chunks[c.x, c.z].isActive = false;

    }

    
    public bool checkForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!isChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.chunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isVoxelMapPopulated)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].getVoxelFromGlovalVector3(pos)].isSolid;

        return blockTypes[getVoxel(pos)].isSolid;
    }


    /*
        Esta função 
    */
    public byte getVoxel(Vector3 pos) 
    {
        int yPos = Mathf.FloorToInt(pos.y);

        /* IMMUTABLE PASS */

        // If outside world, return air.
        if (!isVoxelInWorld(pos)) 
            return 0;

        // If bottom block of chunk, return bedrock
        if (yPos == 0)
            return 1;

        /* BASIC TERRAIN PASS */

        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        byte voxelValue = 0;

        if (yPos == terrainHeight)
            voxelValue = 3;
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = 5;
        else if (yPos > terrainHeight)
            return 0;
        else
            voxelValue = 2;

        /* SECOND PASS */
        if (voxelValue == 2) 
        {
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                    if (Noise.get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
            }
        }
        
        return voxelValue;

    }

    bool isChunkInWorld(ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.worldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.worldSizeInChunks - 1)
            return true;
        else
            return false;
    }


    bool isVoxelInWorld(Vector3 pos) 
    {
        if (pos.x >= 0 && pos.x < VoxelData.worldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.chunkHeight && pos.z >= 0 && pos.z < VoxelData.worldSizeInVoxels)
             return true;
        else
            return false;
    }

}


/*
    Classe do tipo de bloco
*/
[System.Serializable] 
public class BlockType 
{
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

    public int getTextureID(int faceIndex) {

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
                Debug.Log("Error in GetTextureID");
                return 0;

        }

    }

}