using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material material;
    public BlockType[] blockTypes;
    
    Chunk[,] chunks = new Chunk[VoxelData.worldSizeInChunks, VoxelData.worldSizeInChunks];

    private void Start() 
    {
        generateWorld();
    }

    /*
        Cria todos os chunks dentro do world Size
    */
    void generateWorld() 
    {
        for (int x = 0; x < VoxelData.worldSizeInChunks; x++) {
            for (int z = 0; z < VoxelData.worldSizeInChunks; z++) {
                
                createNewChunk(x, z);

            }
        }
    }


    /*
        Esta função 
    */
    public byte getVoxel(Vector3 pos) 
    {
        // ????????????
        if (!isVoxelInWorld(pos)) 
            return 0;

        if (pos.y < 1)
            return 1;
        else if (pos.y == VoxelData.chunkHeight - 1)
            return 3;
        else 
            return 2;
    }


    void createNewChunk(int x, int z) 
    {
        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
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