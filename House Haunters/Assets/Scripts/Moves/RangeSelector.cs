using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// selects a single item within range
public class RangeSelector : ISelector
{
    public bool SelfSelectable { get; private set; }
    public bool NeedsLineOfSight { get; private set; }
    public int Range { get; private set; }

    public RangeSelector(int range, bool selfSelectable, bool needsLineOfSight) {
        Range = range;
        SelfSelectable = selfSelectable;
        NeedsLineOfSight = needsLineOfSight;
    }

    public List<List<Vector2Int>> GetSelectionGroups(Monster user) {
        return LevelGrid.Instance.GetTilesInRange(user.Tile, Range, false)
            .Filter((Vector2Int tile) => { return (SelfSelectable || tile != user.Tile) && (!NeedsLineOfSight || HasLineOfSight(user.Tile, tile)); })
            .Map((Vector2Int tile) => { return new List<Vector2Int>() { tile }; }); // put each tile in its own list
    }

    private static bool HasLineOfSight(Vector2Int startTile, Vector2Int endTile) {
        LevelGrid level = LevelGrid.Instance;
        Vector2 startPoint = level.Tiles.GetCellCenterWorld((Vector3Int)startTile);
        Vector2 direction = (Vector2)level.Tiles.GetCellCenterWorld((Vector3Int)endTile) - startPoint;
        for(int x = Mathf.Min(startTile.x, endTile.x); x <= Mathf.Max(startTile.x, endTile.x); x++) {
            for(int y = Mathf.Min(startTile.y, endTile.y); y <= Mathf.Max(startTile.y, endTile.y); y++) {
                Vector3Int testCell = new Vector3Int(x, y, 0);
                Vector2 cellCenter = level.Tiles.GetCellCenterWorld(testCell);
                Vector2 startToCellCenter = cellCenter - startPoint;
                Vector2 projection = Vector3.Project(startToCellCenter, direction);
                Vector2 perpendicular = startToCellCenter - projection;

                Vector2 closestPoint = cellCenter - perpendicular;
                if(Mathf.Abs(closestPoint.x - cellCenter.x) < LevelGrid.TileWordSize / 2f && Mathf.Abs(closestPoint.y - cellCenter.y) < LevelGrid.TileWordSize / 2f && level.GetTile((Vector2Int)testCell).IsWall) {
                    return false;
                }
            }
        }

        return true;
    }
}
