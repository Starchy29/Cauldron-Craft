using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// selects a single item within range
public class RangeSelector : Selector
{
    public MonsterValue RangeGetter { get; private set; }
    public bool SelfSelectable { get; private set; }
    public bool NeedsLineOfSight { get; private set; }

    public RangeSelector(MonsterValue rangeFunction, bool selfSelectable, bool needsLineOfSight) {
        RangeGetter = rangeFunction;
        SelfSelectable = selfSelectable;
        NeedsLineOfSight = needsLineOfSight;
    }

    public List<List<Vector2Int>> GetSelectionGroups(Monster user) {
        LevelGrid level = LevelGrid.Instance;
        return LevelGrid.Instance.GetTilesInRange(user.Tile, RangeGetter(user), false)
            .Map((Vector2Int tile) => { return new List<Vector2Int>() { tile }; }); // put each tile in its own list
    }
}
