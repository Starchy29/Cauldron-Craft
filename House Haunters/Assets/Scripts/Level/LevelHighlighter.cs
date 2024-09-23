using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum HighlightType {
    AreaVisual,
    Highlight,
    Option,
    Hovered,
    Selected
}

public class LevelHighlighter : MonoBehaviour
{
    [SerializeField] private ComputeShader computeShader;

    private MeshRenderer meshRenderer;
    private MeshFilter filter;
    private RenderTexture texture;

    private Vector2Int resolution;
    private const int TILE_PIXEL_WIDTH = 96;

    private ComputeBuffer tileBuffer;
    private Vector3Int groupCounts;

    private float t;

    private struct TileTraits {
        public bool areaVisual;
        public bool highlight;
        public bool option;
        public bool hovered;
        public bool selected;
    }

    private struct TileInfo {
        public const int STRIDE = 4 * sizeof(int);

        public int floorType; // 0: ground, 1: wall, 2: pit
        public int highlightType;
        public int terrainController;
        public int capturer;
    }

    private Team[] teams;
    private TileTraits[,] traitArray;
    private TileInfo[] infoArray;

    private Dictionary<HighlightType, List<Vector2Int>> highlightedTiles;

    public static LevelHighlighter Instance { get; private set; }
    public Vector2Int? CursorTile;
    public ResourcePile HoveredResource;

    void Start() {
        Instance = this;
        LevelGrid level = LevelGrid.Instance;
        resolution = new Vector2Int(level.Width * TILE_PIXEL_WIDTH, level.Height * TILE_PIXEL_WIDTH);

        highlightedTiles = new Dictionary<HighlightType, List<Vector2Int>>();
        foreach(HighlightType type in Enum.GetValues(typeof(HighlightType))) {
            highlightedTiles[type] = null;
        }

        SetUpTexture();
        SetUpShader();
    }

    ~LevelHighlighter() {
        tileBuffer.Dispose();
        texture.Release();
    }

    void Update() {
        t += Time.deltaTime / 2f;
        t %= 1f;
        computeShader.SetFloat("t", t);

        if(CursorTile.HasValue) {
            computeShader.SetInts("cursorTile", CursorTile.Value.x, CursorTile.Value.y);
        } else {
            computeShader.SetInts("cursorTile", -1, -1);
        }
        if(HoveredResource == null) {
            computeShader.SetInts("hoveredZoneCenter", -20, -20);
        } else {
            computeShader.SetInts("hoveredZoneCenter", HoveredResource.Tile.x, HoveredResource.Tile.y);
        }

        UpdateHighlightData();
        tileBuffer.SetData(infoArray);
        computeShader.SetBuffer(0, "_TileData", tileBuffer);
        computeShader.Dispatch(0, groupCounts.x, groupCounts.y, groupCounts.z);
    }

    public void ColorTiles(List<Vector2Int> tiles, HighlightType type) {
        if(highlightedTiles[type] != null) {
            foreach(Vector2Int tile in highlightedTiles[type]) {
                SetState(tile, type, false);
            }
        }

        highlightedTiles[type] = tiles;

        if(tiles != null) {
            foreach(Vector2Int tile in tiles) {
                SetState(tile, type, true);
            }
        }
    }

    public void ColorTile(Vector2Int tile, HighlightType type) {
        ColorTiles(new List<Vector2Int> { tile }, type);
    }

    public void UpdateCapture(ResourcePile resource) {
        foreach(Vector2Int tile in LevelGrid.Instance.GetTilesInRange(resource.Tile, 2, true)) {
            int index = tile.x + traitArray.GetLength(0) * tile.y;
            int captureCode = -1;
            if(resource.Contested) {
                captureCode = 3;
            }
            else if(resource.Controller != null) {
                captureCode = resource.Controller == teams[0] ? 1 : 2;
            }

            TileInfo info = infoArray[index];
            info.capturer = captureCode;
            infoArray[index] = info;
        }
    }

    private void SetState(Vector2Int tile, HighlightType type, bool active) {
        TileTraits traits = traitArray[tile.x, tile.y];
        switch(type) {
            case HighlightType.AreaVisual:
                traits.areaVisual = active;
                break;
            case HighlightType.Highlight:
                traits.highlight = active;
                break;
            case HighlightType.Option:
                traits.option = active;
                break;
            case HighlightType.Hovered:
                traits.hovered = active;
                break;
            case HighlightType.Selected:
                traits.selected = active;
                break;
        }
        traitArray[tile.x, tile.y] = traits;
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
        texture = new RenderTexture(resolution.x, resolution.y, 1);
        texture.filterMode = FilterMode.Point;
        texture.enableRandomWrite = true;
        texture.Create();

        meshRenderer.sharedMaterial.mainTexture = texture;
        meshRenderer.sortingLayerName = "Tile Highlight";
    }

    private void SetUpShader() {
        LevelGrid level = LevelGrid.Instance;
        traitArray = new TileTraits[level.Width, level.Height];
        tileBuffer = new ComputeBuffer(level.Width * level.Height, TileInfo.STRIDE);
        infoArray = new TileInfo[level.Width * level.Height];
        for(int y = 0; y < level.Height; y++) {
            for(int x = 0; x < level.Width; x++) {
                byte groundType = 2;
                WorldTile tile = level.GetTile(new Vector2Int(x, y));
                if(tile.Walkable) {
                    groundType = 0;
                }
                else if(tile.IsWall) {
                    groundType = 1;
                }

                infoArray[x + y * level.Width] = new TileInfo {
                    floorType = groundType
                };
            }
        }

        teams = GameManager.Instance.AllTeams;
        computeShader.SetVector("team1Color", teams[0].TeamColor);
        computeShader.SetVector("team2Color", teams[1].TeamColor);
        computeShader.SetTexture(0, "_Texture", texture);
        computeShader.SetInts("tileDims", level.Width, level.Height);
        computeShader.SetInt("pixPerTile", TILE_PIXEL_WIDTH);

        uint sizeX, sizeY, sizeZ;
        computeShader.GetKernelThreadGroupSizes(0, out sizeX, out sizeY, out sizeZ);
        groupCounts = new Vector3Int(
            Mathf.CeilToInt((float)resolution.x / sizeX), 
            Mathf.CeilToInt((float)resolution.y / sizeY),
            1
        );
    }

    private void UpdateHighlightData() {
        LevelGrid level = LevelGrid.Instance;
        for(int y = 0; y < traitArray.GetLength(1); y++) {
            for(int x = 0; x < traitArray.GetLength(0); x++) {
                // transfer highlight data
                int index = x + traitArray.GetLength(0) * y;
                TileTraits traits = traitArray[x, y];
                TileInfo data = infoArray[index];

                int highlightCode = 0;
                if(traits.selected) {
                    highlightCode = 5;
                }
                else if(traits.hovered) {
                    highlightCode = 4;
                }
                else if(traits.option) {
                    highlightCode = 3;
                }
                else if(traits.highlight) {
                    highlightCode = 2;
                }
                else if(traits.areaVisual) {
                    highlightCode = 1;
                }

                data.highlightType = highlightCode;
                infoArray[index] = data;
            }
        }
    }

    // needs to be queued in the animation manager to appear and disappear at the correct times
    public void UpdateZoneController(Vector2Int tile, Team controller) {
        int index = tile.x + traitArray.GetLength(0) * tile.y;
        TileInfo data = infoArray[index];

        // check zone controller
        if(controller == null) {
            data.terrainController = 0;
        } else {
            data.terrainController = controller == teams[0] ? 1 : 2;
        }

        infoArray[index] = data;
    }
}
