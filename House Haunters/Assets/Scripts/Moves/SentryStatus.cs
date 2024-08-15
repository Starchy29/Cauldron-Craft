using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentryStatus : UniqueStatus
{
    private const int RANGE = 5;
    
    private SentryStatus(Monster target) : base(target, 2, PrefabContainer.Instance.auraStatus, UniqueStatuses.Sentry) {
        LevelGrid.Instance.OnMonsterMove += CheckMovement;
        target.Controller.OnTurnStart += Remove;
    }

    public static void BecomeSentry(Monster user, Vector2Int target) {
        SentryStatus status = new SentryStatus(LevelGrid.Instance.GetMonster(target));
    }

    private void CheckMovement(Monster mover) {
        if(mover.Controller == target.Controller || 
            Global.CalcTileDistance(mover.Tile, target.Tile) > RANGE ||
            !RangeSelector.HasLineOfSight(mover.Tile, target.Tile)
        ) {
            return;
        }

        // attack the enemy that moved within vision
        // queue animation
        mover.ReceiveAttack(5, target);
    }

    protected override void Remove() {
        base.Remove();
        LevelGrid.Instance.OnMonsterMove -= CheckMovement;
        target.Controller.OnTurnStart -= Remove;
    }
}
