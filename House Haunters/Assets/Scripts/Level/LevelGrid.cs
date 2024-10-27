using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Must be attached to the tilemap. Vector2Ints store x as the column and y as the row
public class LevelGrid : MonoBehaviour
{
    public const float TILE_WIDTH = 1f;

    private GridEntity[,] entityGrid;
    private WorldTile[,] environmentGrid;

    public static LevelGrid Instance { get; private set; }
    public Tilemap Tiles { get; private set; }
    public const float TileWordSize = 1;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public event MonsterTrigger OnMonsterMove;

    void Awake() {
        Instance = this;
        Tiles = GetComponent<Tilemap>();
        Width = 19;
        Height = 19;

        Monster.pathDistances = new Monster.PathData[Height, Width];
        entityGrid = new GridEntity[Height, Width];
        environmentGrid = new WorldTile[Height, Width];

        Dictionary<TileType, WorldTile> typeToData = new Dictionary<TileType, WorldTile>() {
            { TileType.Ground, new WorldTile(true, false, 1) },
            { TileType.Pit, new WorldTile(false, false, 1) },
            { TileType.Wall, new WorldTile(false, true, 3) }
        };

        for(int y = 0; y < Height; y++) {
            for(int x = 0; x < Width; x++) {
                TypedTile tile = Tiles.GetTile<TypedTile>(new Vector3Int(x, y, 0));
                environmentGrid[y, x] = typeToData[tile == null ? TileType.Ground : tile.Type];
            }
        }
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
        return result == null || !(result is Monster) ? null : (Monster)result;
    }

    public WorldTile GetTile(Vector2Int tile) {
        return environmentGrid[tile.y, tile.x];
    }

    public void SetTileAffect(Vector2Int tile, TileAffector effect) {
        environmentGrid[tile.y, tile.x].CurrentEffect = effect;
        AnimationsManager.Instance.QueueFunction(() => { LevelHighlighter.Instance.UpdateZoneController(tile, effect == null ? null : effect.Controller); });
    }

    public bool IsInGrid(Vector2Int tile) {
        return tile.x >= 0 && tile.y >= 0 && tile.x < Width && tile.y < Height;
    }

    public void PlaceEntity(GridEntity entity, Vector2Int tile) {
        entityGrid[tile.y, tile.x] = entity;
        entity.Tile = tile;

        if(entity is Monster) {
            OnMonsterMove?.Invoke((Monster)entity);
        }

        TileAffector effect = environmentGrid[tile.y, tile.x].CurrentEffect;
        if(entity is Monster && effect != null && effect.Controller != entity.Controller) {
            effect.LandMonster((Monster)entity);
        }
    }

    // determines if this spot is an open space for a monster to move to
    public bool IsOpenTile(Vector2Int tile) {
        return environmentGrid[tile.y, tile.x].Walkable && entityGrid[tile.y, tile.x] == null;
    }

    // allows simulation of entities at new positions without triggering any effects.
    // Entities are expected to be placed back to their original position at the end
    public void TestEntity(GridEntity entity, Vector2Int tempPosition) {
        if(entityGrid[tempPosition.y, tempPosition.x] != null) {
            throw new ArgumentException($"Error: Cannot test a tile that is currently occupied (tried to test {tempPosition}).");
        }

        entityGrid[entity.Tile.y, entity.Tile.x] = null;
        entityGrid[tempPosition.y, tempPosition.x] = entity;
        entity.Tile = tempPosition;
    }

    public void MoveEntity(GridEntity entity, Vector2Int tile) {
        ClearEntity(entity.Tile);
        PlaceEntity(entity, tile);
    }

    public void ClearEntity(Vector2Int tile) {
        entityGrid[tile.y, tile.x] = null;
    }
}
