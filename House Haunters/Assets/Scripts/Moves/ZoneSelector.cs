using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// selects a square area within a larger square area
public class ZoneSelector : Selector
{
    public int Radius { get; private set; }
    public int Width { get; private set; }

    private LevelGrid level;

    public List<List<Vector2Int>> GetSelectionGroups(Monster user) {
        level = LevelGrid.Instance;
        List<List<Vector2Int>> groups = new List<List<Vector2Int>>();

        for(int x = -Radius; x <= Radius - Width; x++) {
            for(int y = -Radius; y <= Radius - Width; y++) {
                List<Vector2Int> group = GenerateSquare(new Vector2Int(x, y));
                if(group.Count > 0) {
                    groups.Add(group);
                }
            }
        }

        return groups;
    }

    private List<Vector2Int> GenerateSquare(Vector2Int bottomLeft) {
        List<Vector2Int> result = new List<Vector2Int>();
        for(int x = 0; x < Width; x++) {
            for(int y = 0; y < Width; y++) {
                if(level.IsInGrid(new Vector2Int(x, y))) {
                    result.Add(bottomLeft + new Vector2Int(x, y));
                }
            }
        }
        return result;
    }
}
