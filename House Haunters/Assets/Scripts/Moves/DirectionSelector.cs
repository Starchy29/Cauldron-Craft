using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an ability that moves straight forward and stops at the first enemy it hits unless it is piercing
public class DirectionSelector : Selector
{
    public int Range { get; private set; }
    public bool Piercing { get; private set; }

    public List<List<Vector2Int>> GetSelectionGroups(Monster user) {
        LevelGrid level = LevelGrid.Instance;
        List<List<Vector2Int>> groups = new List<List<Vector2Int>>();

        foreach(Vector2Int direction in Global.Cardinals) {
            List<Vector2Int> group  = new List<Vector2Int>();
            for(int i = 0; i < Range; i++) {
                Vector2Int testTile = user.Tile + i * direction;

                // stop at walls
                if(level.GetTile(testTile).IsWall) {
                    break;
                }

                if(Piercing) {
                    group.Add(testTile);
                    continue;
                }

                GridEntity entity = level.GetEntity(testTile);
                if(entity != null && entity is Monster && ((Monster)entity).Controller != user.Controller) {
                    group.Add(testTile);
                    break;
                }
            }

            if(group.Count > 0) {
                groups.Add(group);
            }
        }

        return groups;
    }
}
