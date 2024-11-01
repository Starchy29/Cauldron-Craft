using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : Move
{
    public int Damage { get; private set; }

    public delegate void HitTrigger(Monster user, Monster target, int healthLost);
    private HitTrigger OnHit;

    public Attack(string name, int cooldown, int damage, ISelector selection, AnimationFunction effectAnimation, Sounds soundEffect, string description = "", HitTrigger hitEffect = null) 
        : base(name, cooldown, selection.Range > 1 ? MoveType.Attack : MoveType.Attack, Targets.Enemies, selection, null, effectAnimation, soundEffect, description)
    {
        Damage = damage;
        OnHit = hitEffect;
        ApplyEffect = DealDamage;
    }

    private void DealDamage(Monster user, Vector2Int tile) {
        Monster hitMonster = LevelGrid.Instance.GetMonster(tile);
        int startHealth = hitMonster.Health;
        hitMonster.TakeDamage(Damage, user);

        if(OnHit != null) {
            OnHit(user, hitMonster, hitMonster.Health < startHealth ? startHealth - hitMonster.Health : 0);
        }
    }
}
