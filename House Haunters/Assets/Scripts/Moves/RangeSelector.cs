using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeSelector : Selector
{
    public int Range { get; private set; }
    public bool SelfSelectable { get; private set; }

    public List<List<Vector2Int>> GetSelectionGroups(Monster user) {
        LevelGrid level = LevelGrid.Instance;
        List<List<Vector2Int>> result = new List<List<Vector2Int>>();

        List<Vector2Int> reachable = level.GetTilesInRange(user.Tile, Range, false);
        if(!SelfSelectable) {
            reachable.Remove(user.Tile);
        }
        return reachable.Map((Vector2Int tile) => { return new List<Vector2Int>() { tile }; }); // put each tile in its own list
    }
}
