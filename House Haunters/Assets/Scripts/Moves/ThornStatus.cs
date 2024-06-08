using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornStatus : UniqueStatus
{
    private ThornStatus(Monster target) : base(target, 3, PrefabContainer.Instance.spikeShieldPrefab, UniqueStatuses.Thorns) {
        target.OnAttacked += DamageMeleeAttacker;
    }

    public static void ApplyThorns(Monster user, Vector2Int tile) {
        ThornStatus leech = new ThornStatus(LevelGrid.Instance.GetMonster(tile));
    }

    protected override void Remove() {
        target.OnAttacked -= DamageMeleeAttacker;
        base.Remove();
    }

    private static void DamageMeleeAttacker(Monster attacker, bool isMelee) {
        if(isMelee) {
            attacker.TakeDamage(6);
        }
    }
}
