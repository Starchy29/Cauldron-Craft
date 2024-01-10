using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// selector specifically for navigating around the level
public class WalkSelector : Selector
{
    public WalkSelector() {}

    public List<List<Vector2Int>> GetSelectionGroups(Monster user) {
        return LevelGrid.Instance.GetTilesInRange(user.Tile, user.Stats.Speed, false)
            .Filter((Vector2Int tile) => { return tile != user.Tile && user.FindPath(tile) != null; })
            .Map((Vector2Int tile) => { return new List<Vector2Int>() { tile }; }); // put each tile in its own list
    }
}
