using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Must be attached to the tilemap. Vector2Ints store x as the column and y as the row
public class LevelGrid : MonoBehaviour
{
    private GridEntity[,] entityGrid;
    private WorldTile[,] environmentGrid;

    private const int width = 20;
    private const int height = 20;

    public static LevelGrid Instance { get; private set; }

    public Tilemap Tiles { get; private set; }

    void Awake() {
        Instance = this;
        Tiles = GetComponent<Tilemap>();

        entityGrid = new GridEntity[width, height];
        environmentGrid = new WorldTile[width, height];
    }

    
    void Update() {
        
    }

    public List<Monster> FindMonstersInRange(Vector2Int spot, int range, bool? onPlayerTeam = null) {
        List<Monster> result = new List<Monster>();

        for(int x = -range; x <= range; x++) {
            int width = range == 1 ? 1 : range - Mathf.Abs(x); // melee attacks allow the extra diagonals
            for(int y = -width; y <= width; y++) {
                Monster monster = GetEntityOnTile(spot + new Vector2Int(x, y)).GetComponent<Monster>();
                if(monster != null && (onPlayerTeam == null || onPlayerTeam.Value == monster.OnPlayerTeam)) {
                    result.Add(monster);
                }
            }
        }

        return result;
    }

    public GridEntity GetEntityOnTile(Vector2Int tile) {
        if(tile.x < 0 || tile.y < 0 || tile.x > width - 1 || tile.y > height - 1) {
            return null;
        }

        return entityGrid[tile.y, tile.x];
    }

    public void SpawnEntity(GameObject prefab, Vector2Int tile) {
        if(entityGrid[tile.y, tile.x] != null) {
            Destroy(entityGrid[tile.y, tile.x]);
        }

        GameObject spawned = Instantiate(prefab);
        entityGrid[tile.y, tile.x] = spawned.GetComponent<GridEntity>();
        spawned.transform.position = Tiles.GetCellCenterWorld((Vector3Int)tile);
    }

    public void MoveEntity(GridEntity entity, Vector2Int tile) {
        Vector2Int previousTile = entity.Tile;
        entityGrid[previousTile.y, previousTile.x] = null;
        entityGrid[tile.y, tile.x] = entity;
        entity.transform.position = Tiles.GetCellCenterWorld((Vector3Int)tile);
    }

    public void ClearTile(Vector2Int tile) {
        entityGrid[tile.y, tile.x] = null;
    }
}
