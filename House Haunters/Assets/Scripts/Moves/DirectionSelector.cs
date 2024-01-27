using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// an ability that moves straight forward and stops at the first enemy it hits unless it is piercing
public class DirectionSelector : ISelector
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

                group.Add(testTile);
                if(Piercing) {
                    continue;
                }

                groups.Add(group);
                group = new List<Vector2Int>();

                Monster monster = level.GetMonster(testTile);
                if(monster != null && monster.Controller != user.Controller) {
                    // stop when reaching an enemy
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
