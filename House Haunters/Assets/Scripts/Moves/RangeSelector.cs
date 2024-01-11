using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeSelector : Selector
{
    public int Range { get; private set; }
    public bool SelfSelectable { get; private set; }
    public bool SeekAllies { get; private set; } // false: seek enemies

    public List<List<Vector2Int>> GetSelectionGroups(Monster user) {
        LevelGrid level = LevelGrid.Instance;
        return LevelGrid.Instance.GetTilesInRange(user.Tile, Range, false)
            .Filter((Vector2Int tile) => {
                GridEntity entity = level.GetEntity(tile);
                return entity != null && entity is Monster && (SelfSelectable || entity != user) && (((Monster)entity).Controller == user.Controller) == SeekAllies; 
            })
            .Map((Vector2Int tile) => { return new List<Vector2Int>() { tile }; }); // put each tile in its own list
    }
}
