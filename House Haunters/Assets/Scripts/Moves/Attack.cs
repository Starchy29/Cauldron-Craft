using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : Move
{
    public int Damage { get; private set; }

    public delegate void HitTrigger(Monster user, Monster target, int healthLost);
    private HitTrigger OnHit;

    public Attack(int cooldown, int damage, ISelector selection, HitTrigger hitEffect = null) : base(cooldown, MoveType.Attack, Targets.Enemies, selection) {
        Damage = damage;
        OnHit = hitEffect;
    }

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        Monster hitMonster = (Monster)LevelGrid.Instance.GetEntity(tile);
        int startHealth = hitMonster.Health;
        hitMonster.TakeDamage(Mathf.FloorToInt(Damage * user.DamageMultiplier), user);
        OnHit(user, hitMonster, hitMonster.Health < startHealth ? startHealth - hitMonster.Health : 0);
    }
}
