using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldMove : Move
{
    public Shield AppliedShield;

    private CombatTrigger OnUse;
    
    public ShieldMove(int cooldown, Selector selection) : base(cooldown, MoveType.Shield, Targets.Allies, selection) {

    }

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        Monster selectedMonster = (Monster)LevelGrid.Instance.GetEntity(tile);
        selectedMonster.ApplyShield(new Shield(AppliedShield.StrengthLevel, AppliedShield.Duration, AppliedShield.BlocksStatus, AppliedShield.BlocksOnce));
    }
}
