using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Must be attached to the tilemap. Vector2Ints store x as the column and y as the row
public class LevelGrid : MonoBehaviour
{
    [SerializeField] private GameObject TileHighlightPrefab;

    public const int width = 12;
    public const int height = 8;

    private GridEntity[,] entityGrid;
    private WorldTile[,] environmentGrid;
    private TileHighlighter[,] tileHighlights;

    public static LevelGrid Instance { get; private set; }
    public Tilemap Tiles { get; private set; }

    void Awake() {
        Instance = this;
        Tiles = GetComponent<Tilemap>();

        entityGrid = new GridEntity[height, width];
        environmentGrid = new WorldTile[height, width];
        tileHighlights = new TileHighlighter[height, width];

        Dictionary<TileType, WorldTile> typeToData = new Dictionary<TileType, WorldTile>() {
            { TileType.Ground, new WorldTile(true, false, 1) },
            { TileType.Pit, new WorldTile(false, false, 1) },
            { TileType.Wall, new WorldTile(false, true, 3) }
        };

        for(int y = 0; y < height; y++) {
            for(int x = 0; x < width; x++) {
                TypedTile tile = Tiles.GetTile<TypedTile>(new Vector3Int(x, y, 0));
                environmentGrid[y, x] = typeToData[tile == null ? TileType.Ground : tile.Type];

                tileHighlights[y, x] = Instantiate(TileHighlightPrefab).GetComponent<TileHighlighter>();
                tileHighlights[y, x].transform.position = Tiles.GetCellCenterWorld(new Vector3Int(x, y, 0));
            }
        }

        Camera.main.transform.position = new Vector3(width / 2, height / 2, Camera.main.transform.position.z);
        Debug.Log("moved camera from LevelGrid.cs");
    }

    
    void Update() {
        
    }

    public List<Vector2Int> GetTilesInRange(Vector2Int spot, int range, bool squareArea) {
        List<Vector2Int> result = new List<Vector2Int>();

        for(int x = -range; x <= range; x++) {
            int width = squareArea || range == 1 ? range : range - Mathf.Abs(x);
            for(int y = -width; y <= width; y++) {
                Vector2Int tile = spot + new Vector2Int(x, y);
                if(IsInGrid(tile)) {
                    result.Add(tile);
                }
            }
        }

        return result;
    }

    public GridEntity GetEntity(Vector2Int tile) {
        return entityGrid[tile.y, tile.x];
    }

    public Monster GetMonster(Vector2Int tile) {
        GridEntity result = entityGrid[tile.y, tile.x];
        if(result == null) {
            return null;
        }

        return result == null || !(result is Monster) ? null : (Monster)result;
    }

    public WorldTile GetTile(Vector2Int tile) {
        return environmentGrid[tile.y, tile.x];
    }

    public bool IsInGrid(Vector2Int tile) {
        return tile.x >= 0 && tile.y >= 0 && tile.x < width && tile.y < height;
    }

    public void PlaceEntity(GridEntity entity, Vector2Int tile) {
        entityGrid[tile.y, tile.x] = entity;
        entityGrid[tile.y, tile.x].Tile = tile;
    }

    public void MoveEntity(GridEntity entity, Vector2Int tile) {
        ClearEntity(entity.Tile);
        PlaceEntity(entity, tile);
    }

    public void ClearEntity(Vector2Int tile) {
        entityGrid[tile.y, tile.x] = null;
    }

    // lights up notable tiles for the player. can overlap with selected tiles
    public void ColorTiles(List<Vector2Int> tiles, TileHighlighter.State colorType) {
        for(int y = 0; y < height; y++) {
            for(int x = 0; x < width; x++) {
                tileHighlights[y, x].SetState(colorType, false);
            }
        }

        if(tiles == null) {
            return;
        }

        for(int i = 0; i < tiles.Count; i++) {
            tileHighlights[tiles[i].y, tiles[i].x].SetState(colorType, true);
        }
    }

    public void ColorTiles(Vector2Int tile, TileHighlighter.State colorType) {
        ColorTiles(new List<Vector2Int>() { tile }, colorType);
    }
}
