using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    // Variables
    public ChunkCoord coord;

    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    byte[,,] VoxelMap = new byte[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];

    World world;


    /*
        Construtor do chunk
        Ao inicializar um chunk
        é criado um gameobject vazio. atribuido um mesh filter e renderer
        definido o material, a posição do chunk já vem definida e aqui é atribuida.
    */
    public Chunk (ChunkCoord _coord, World _world) {

        coord = _coord;
        world = _world;
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.chunkWidth, 0f, coord.z * VoxelData.chunkWidth);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.z;

        populateVoxelMap();
        createMeshData();
        createMesh();

    } 


    /*
        Esta função é responsavel por popular o chunk (voxel map), definindo a posicao de um voxel com o bloco selecionado.
    */
    void populateVoxelMap() {

        for (int y = 0; y < VoxelData.chunkHeight; y++) {
            for (int x = 0; x < VoxelData.chunkWidth; x++) {
                for (int z = 0; z < VoxelData.chunkWidth; z++) {

                    VoxelMap[x, y, z] = world.getVoxel(new Vector3(x, y, z) + position);
 
                }
            }
        }

    }


    /*
        Esta função é responsavel por preencher o voxel map com os blocos.
    */
    void createMeshData() 
    {
        for (int y = 0; y < VoxelData.chunkHeight; y++) {
            for (int x = 0; x < VoxelData.chunkWidth; x++) {
                for (int z = 0; z < VoxelData.chunkWidth; z++) {

                    if (world.blockTypes[VoxelMap[x, y, z]].isSolid)
                        addVoxelDataToChunk(new Vector3(x, y, z));

                }
            }
        }
    }


    /*
        Esta função tem o proposito de facilitar a manipulaçao do estado ativo do chunk
    */
    public bool isActive 
    {
        get { return chunkObject.activeSelf; }
        set { chunkObject.SetActive(value); }
    }


    /*
        Esta função tem o proposito de facilitar o retorno da posiçao do chunk.
        Em vez de escrever chunkObject.transform.position assim só é preciso usar position
    */
    public Vector3 position
    {
        get { return chunkObject.transform.position; }
    }


    /*
        Esta função permite saber se o voxel esta dentro dos limites do chunk
    */
    bool IsVoxelInChunk(int x, int y, int z) 
    {
        if (x < 0 || x > VoxelData.chunkWidth - 1 || y < 0 || y > VoxelData.chunkHeight - 1 || z < 0 || z > VoxelData.chunkWidth - 1)
            return false;
        else
            return true;
    }


    /*
        Esta função é responsavel por retornar ????
    */
    bool checkVoxel(Vector3 pos) 
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        // Verificar se o voxel está dentro dos limites do chunk
        if (!IsVoxelInChunk(x, y, z))
            return world.blockTypes[world.getVoxel(pos + position)].isSolid;

        return world.blockTypes[VoxelMap[x, y, z]].isSolid;
    }


    /*
        Esta função adiciona os dados do voxel ás respetivas listas de dados que compoem um chunk.
        1º - É recebido o voxel pela posiçao dele.
        2º - Primeiramente é feita uma verificação se voxel não tem outro voxel a cobri-lo. 
             Para isto é feita uma soma á posiçao do voxel com a posiçao da direçao dele.
             Se tiver então o voxel não é criado, fazendo o chunk parecer ser oco por dentro para melhorar a performance.
        3º - Os vertices são adicionados á lista dos vertices do chunk
        4º - Os uvs são adicionados á lista dos uvs do chunk através do getTexture que adiciona os uvs do bloco que esta no voxel.
        5º - Os triangulos são adicionados á lista de triangulos do chunk
    */
    void addVoxelDataToChunk(Vector3 pos) 
    {
        // Este loop acontece 6 vezes para cada 1 voxel.
        for (int j = 0; j < 6; j++) 
        {
            if (!checkVoxel(pos + VoxelData.faceChecks[j])) 
            {
                // Add vertices
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[j, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[j, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[j, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[j, 3]]);

                // Add texture
                byte blockID = VoxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
                AddTexture(world.blockTypes[blockID].getTextureID(j));

                // Add voxel triangles
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


    /*
        Com todos os dados guardados nas listas. é feita a criação do chunk
    */
    void createMesh() 
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
    

    /*
        Esta função é responsavel por saber a posiçao de uma textura dependendo do id dela.
        E no final adicionar á lista de uvs
    */
    void AddTexture (int textureID) 
    {
        float y = textureID / VoxelData.textureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.textureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }

}


/*
    Cordenadas do chunk
*/
public class ChunkCoord 
{
    public int x;
    public int z;

    public ChunkCoord (int _x, int _z) 
    {
        x = _x;
        z = _z;
    }

    public bool Equals (ChunkCoord other)
    {
        if (other == null)
            return false;
        else if (other.x == x && other.z == z)
            return true;
        else 
            return false;
    }

}