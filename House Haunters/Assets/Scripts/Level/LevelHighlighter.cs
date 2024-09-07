using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelHighlighter : MonoBehaviour
{
    [SerializeField] private ComputeShader computeShader;

    private MeshRenderer meshRenderer;
    private MeshFilter filter;
    private RenderTexture texture;

    private const int RESOLUTION_HEIGHT = 900;
    private int RESOLUTION_WIDTH;

    private ComputeBuffer tileBuffer;
    private Vector3Int groupCounts;

    private float t;

    void Start() {
        LevelGrid level = LevelGrid.Instance;
        float aspectRatio = (float)level.Width / level.Height;
        RESOLUTION_WIDTH = (int)(RESOLUTION_HEIGHT * aspectRatio);

        SetUpTexture();
        SetUpShader();
    }

    ~LevelHighlighter() {
        tileBuffer.Dispose();
        texture.Release();
    }

    void Update() {
        t += Time.deltaTime;
        t %= 1f;
        computeShader.SetFloat("t", t);

        UpdateTileData();
        computeShader.Dispatch(0, groupCounts.x, groupCounts.y, groupCounts.z);
    }

    private void SetUpTexture() {
        LevelGrid level = LevelGrid.Instance;
        Camera camera = Camera.main;
        Vector2 levelWorldScale = new Vector2(level.Width, level.Height);

        transform.position = new Vector3(levelWorldScale.x / 2f, levelWorldScale.y / 2f, 0); // center in the level

        // create rectangle mesh covering the level
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[4] {
            new Vector3(-levelWorldScale.x / 2f, -levelWorldScale.y / 2f, 0), // bottom left
            new Vector3(levelWorldScale.x / 2f, -levelWorldScale.y / 2f, 0), // bottom right
            new Vector3(levelWorldScale.x / 2f, levelWorldScale.y / 2f, 0), // top right
            new Vector3(-levelWorldScale.x / 2f, levelWorldScale.y / 2f, 0) // top left
        };
        mesh.triangles = new int[6] { // clockwise winding order
            0, 3, 2,
            0, 2, 1
        };
        mesh.uv = new Vector2[4] {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/SimpleShader"));

        filter = gameObject.AddComponent<MeshFilter>();
        filter.mesh = mesh;

        // Create texture
        texture = new RenderTexture(RESOLUTION_WIDTH, RESOLUTION_HEIGHT, 1);
        texture.filterMode = FilterMode.Point;
        texture.enableRandomWrite = true;
        texture.Create();

        meshRenderer.sharedMaterial.mainTexture = texture;
        meshRenderer.sortingLayerName = "Tile Highlight";
    }

    private void SetUpShader() {
        LevelGrid level = LevelGrid.Instance;
        tileBuffer = new ComputeBuffer(level.Width * level.Height, 4);
        //tileBuffer.SetData();
        //computeShader.SetBuffer(0, "");

        computeShader.SetTexture(0, "_Texture", texture);
        computeShader.SetInts("tileDims", level.Width, level.Height);
        computeShader.SetFloats("pixelDims", RESOLUTION_WIDTH, RESOLUTION_HEIGHT);

        uint sizeX, sizeY, sizeZ;
        computeShader.GetKernelThreadGroupSizes(0, out sizeX, out sizeY, out sizeZ);
        groupCounts = new Vector3Int(
            Mathf.CeilToInt((float)RESOLUTION_WIDTH / sizeX), 
            Mathf.CeilToInt((float)RESOLUTION_HEIGHT / sizeY),
            1
        );
    }

    private void UpdateTileData() {

    }
}
