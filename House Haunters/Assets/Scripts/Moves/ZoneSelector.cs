using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// selects a square area within a larger square area
public class ZoneSelector : Selector
{
    public int Radius { get; private set; }
    public int Width { get; private set; }

    public List<List<Vector2Int>> GetSelectionGroups(Monster user) {
        LevelGrid level = LevelGrid.Instance;
        List<List<Vector2Int>> groups = new List<List<Vector2Int>>();

        for(int x = -Radius; x <= Radius - Width; x++) {
            for(int y = -Radius; y <= Radius - Width; y++) {
                groups.Add(GenerateSquare(new Vector2Int(x, y)));
            }
        }

        return null;
    }

    private List<Vector2Int> GenerateSquare(Vector2Int bottomLeft) {
        List<Vector2Int> result = new List<Vector2Int>();
        for(int x = 0; x < Width; x++) {
            for(int y = 0; y < Width; y++) {
                result.Add(bottomLeft + new Vector2Int(x, y));
            }
        }
        return result;
    }
}
