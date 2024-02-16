using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : Move
{
    public int Damage { get; private set; }

    public delegate void HitTrigger(Monster user, Monster target, int healthLost);
    private HitTrigger OnHit;

    public Attack(string name, int cooldown, int damage, ISelector selection, string description = "", HitTrigger hitEffect = null) 
        : base(name, cooldown, MoveType.Attack, Targets.Enemies, selection, description)
    {
        Damage = damage;
        OnHit = hitEffect;
    }

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        Monster hitMonster = LevelGrid.Instance.GetMonster(tile);
        int startHealth = hitMonster.Health;
        hitMonster.TakeDamage(Mathf.FloorToInt(Damage * user.DamageMultiplier), user);

        if(OnHit != null) {
            OnHit(user, hitMonster, hitMonster.Health < startHealth ? startHealth - hitMonster.Health : 0);
        }
    }
}
