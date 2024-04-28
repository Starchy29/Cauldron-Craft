using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Capturable : GridEntity
{
    protected override void Start() {
        base.Start();
        GameManager.Instance.OnTurnEnd += CheckCapture;
    }

    private void CheckCapture(Team turnEnder, Team nextTurn) {
        LevelGrid level = LevelGrid.Instance;
        List<Monster> adjacentMonsters = level.GetTilesInRange(Tile, 1, true)
            .Map((Vector2Int tile) => { return level.GetMonster(tile); })
            .Filter((Monster monster) => { return monster != null; });

        Team adjacentTeam = null;
        foreach(Monster monster in adjacentMonsters) {
            if(adjacentTeam == null) {
                adjacentTeam = monster.Controller;
            }
            else if(monster.Controller != adjacentTeam) {
                Controller = null; // no team controls when it is contested
                return;
            }
        }

        if(adjacentTeam != null) {
            // don't lose control if there are no nearby monsters
            Controller = adjacentTeam;
        }
    }
}
