using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSelector : Selector
{
    public int SpeedModifier { get; private set; }

    public MovementSelector(int speedModifier) {
        SpeedModifier = speedModifier;
    }

    public List<List<Vector2Int>> GetSelectionGroups(Monster user) {
        LevelGrid level = LevelGrid.Instance;
        List<List<Vector2Int>> result = new List<List<Vector2Int>>();

        List<Vector2Int> walkable = level.GetTilesInRange(user.Tile, user.Stats.Speed + SpeedModifier, false)
            .Filter((Vector2Int tile) => { return user.FindPath(tile) != null; });
        walkable.Remove(user.Tile);
        return walkable.Map((Vector2Int tile) => { return new List<Vector2Int>() { tile }; }); // put each tile in its own list
    }
}
