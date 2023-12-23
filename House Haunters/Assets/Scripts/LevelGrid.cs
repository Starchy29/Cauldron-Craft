using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Vector2Ints store x as the column and y as the row
public class LevelGrid : MonoBehaviour
{
    private Monster[,] characterGrid;
    private WorldTile[,] environmentGrid;

    private const int width = 5;
    private const int height = 5;

    public static LevelGrid Instance { get; private set; }

    public Tilemap Tiles { get; private set; }

    void Awake() {
        Instance = this;
        Tiles = GetComponent<Tilemap>();
    }

    
    void Update() {
        
    }

    public List<Monster> FindMonstersInRange(Vector2Int spot, int range, bool? onPlayerTeam = null) {
        List<Monster> result = new List<Monster>();

        for(int x = -range; x <= range; x++) {
            int width = range == 1 ? 1 : range - Mathf.Abs(x); // melee attacks allow the extra diagonals
            for(int y = -width; y <= width; y++) {
                Monster monster = GetMonsterOnTile(spot + new Vector2Int(x, y));
                if(monster != null && (onPlayerTeam == null || onPlayerTeam.Value == monster.OnPlayerTeam)) {
                    result.Add(monster);
                }
            }
        }

        return result;
    }

    private Monster GetMonsterOnTile(Vector2Int tile) {
        if(tile.x < 0 || tile.y < 0 || tile.x > width - 1 || tile.y > height - 1) {
            return null;
        }

        return characterGrid[tile.y, tile.x];
    }
}
