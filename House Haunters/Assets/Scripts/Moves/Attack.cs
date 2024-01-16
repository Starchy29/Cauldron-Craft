using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : Move
{
    public int Damage { get; private set; }

    public delegate void CombatTrigger(Monster attacker, Monster hitMonster);
    private CombatTrigger OnHit;

    public Attack(int cooldown, int damage, Selector selection) : base(cooldown, MoveType.Attack, Targets.Enemies, selection) {
        Damage = damage;
    }

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        Monster hitMonster = (Monster)LevelGrid.Instance.GetEntity(tile);
        hitMonster.TakeDamage(Mathf.FloorToInt(Damage * user.DamageMultiplier));
        OnHit(user, hitMonster);
    }
}
