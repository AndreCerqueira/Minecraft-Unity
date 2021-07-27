using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly int chunkWidth = 16;
    public static readonly int chunkHeight = 128;
    
    public static readonly int worldSizeInChunks = 10;
    public static int worldSizeInVoxels {
        get {return worldSizeInChunks * chunkWidth; }
    }

    public static readonly int viewDistanceInChunks = 5;

    public static readonly int textureAtlasSizeInBlocks = 4;
    // Valor que varia entre 0.0f e 1.0f dependendo do size da textura em cima. Se for 4 é 0.25f
    public static float NormalizedBlockTextureSize {
        get { return 1f / (float)textureAtlasSizeInBlocks; }
    }

    // Vertices do cubo
    public static readonly Vector3[] voxelVerts = new Vector3[8] {

        new Vector3(0.0f, 0.0f, 0.0f), // 0
        new Vector3(1.0f, 0.0f, 0.0f), // 1
        new Vector3(1.0f, 1.0f, 0.0f), // 2
        new Vector3(0.0f, 1.0f, 0.0f), // 3
        new Vector3(0.0f, 0.0f, 1.0f), // 4
        new Vector3(1.0f, 0.0f, 1.0f), // 5
        new Vector3(1.0f, 1.0f, 1.0f), // 6
        new Vector3(0.0f, 1.0f, 1.0f)  // 7

    };

    // Direções das Faces do cubo
    public static readonly Vector3[] faceChecks = new Vector3[6] {

        new Vector3(0.0f, 0.0f, -1.0f), // Back Face
        new Vector3(0.0f, 0.0f, 1.0f),  // Front Face
        new Vector3(0.0f, 1.0f, 0.0f),  // Top Face
        new Vector3(0.0f, -1.0f, 0.0f), // Bottom Face
        new Vector3(-1.0f, 0.0f, 0.0f), // Left Face
        new Vector3(1.0f, 0.0f, 0.0f)   // Right Face

    };

    // Triangulos do cubo, 2 triangulos compoem uma face, sao 6 valores no total em cada, mas 2 sao repetidos por isso ficam 4
    public static readonly int[,] voxelTris = new int[6, 4] {

        // Back, Front, Top, Bottom, Left, Right

        {0, 3, 1, 2}, // Back Face
        {5, 6, 4, 7}, // Front Face
        {3, 7, 2, 6}, // Top Face
        {1, 5, 0, 4}, // Bottom Face
        {4, 7, 0, 3}, // Left Face
        {1, 2, 5, 6}  // Right Face

    };

    // Os uvs são Vector2s de valores que variam entre 0.0f e 1.0f. Estes valores representam os offsets de uma textura
    public static readonly Vector2[] voxelUvs = new Vector2[4] {

        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f),

    };

}
