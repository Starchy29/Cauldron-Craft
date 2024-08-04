using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

// specifically for monsters moving; validates which tiles can be navigated to
public class PathSelector : ISelector
{
    public int Range { get { return -1; } } // check monster's speed stat instead

    public static readonly PathSelector Singleton = new PathSelector();
    private PathSelector() {}

    public List<List<Vector2Int>> GetSelectionGroups(Monster user) {
        List<Vector2Int> inRange = LevelGrid.Instance.GetTilesInRange(user.Tile, user.CurrentSpeed, false);

        // look at further tiles first to pathfind less
        inRange.Sort((Vector2Int cur, Vector2Int next) => { return Global.CalcTileDistance(next, user.Tile) - Global.CalcTileDistance(cur, user.Tile); });

        // validate each tile
        List<Vector2Int> navigableTiles = new List<Vector2Int>();
        foreach(Vector2Int option in inRange) {
            if(navigableTiles.Contains(option)) {
                continue;
            }

            List<Vector2Int> path = user.FindPath(option);
            if(path != null) {
                // add the tiles of this path until it intersects with a previously found path
                for(int i = path.Count - 1; i >= 0; i--) {
                    if(navigableTiles.Contains(path[i])) {
                        break;
                    }

                    if(user.CanMoveTo(path[i])) {
                        navigableTiles.Add(path[i]);
                    }
                }
            }
        }

        // put each tile in its own list
        return navigableTiles.Map((Vector2Int tile) => { return new List<Vector2Int>() { tile }; });
    }
}
