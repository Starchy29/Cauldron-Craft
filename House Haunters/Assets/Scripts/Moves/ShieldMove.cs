using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldMove : Move
{
    public Shield AppliedShield { get; private set; }

    public delegate void BonusEffect(Monster user, Monster shielded);
    private BonusEffect OnUse;
    
    public ShieldMove(int cooldown, ISelector selection, Shield effect, BonusEffect bonusEffect = null) 
        : base(cooldown, MoveType.Shield, Targets.Allies, selection) 
    {
        AppliedShield = effect;
    }

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        Monster selectedMonster = (Monster)LevelGrid.Instance.GetEntity(tile);
        selectedMonster.ApplyShield(new Shield(AppliedShield.StrengthLevel, AppliedShield.Duration, AppliedShield.BlocksStatus, AppliedShield.BlocksOnce));
        OnUse(user, selectedMonster);
    }
}
