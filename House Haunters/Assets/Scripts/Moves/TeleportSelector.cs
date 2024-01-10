using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportSelector : Selector
{
    private int range;

    public TeleportSelector(int range) {
        this.range = range;
    }

    public List<List<Vector2Int>> GetSelectionGroups(Monster user) {
        return LevelGrid.Instance.GetTilesInRange(user.Tile, range, false)
            .Filter((Vector2Int tile) => { return tile != user.Tile && user.CanStandOn(tile); })
            .Map((Vector2Int tile) => { return new List<Vector2Int>() { tile }; }); // put each tile in its own list
    }
}
