using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldMove : Move
{
    public Shield AppliedShield { get; private set; }

    private event EffectFunction OnUse;
    
    public ShieldMove(string name, int cooldown, ISelector selection, Shield effect, AnimationQueuer effectAnimation, string description = "", EffectFunction bonusEffect = null) 
        : base(name, cooldown, MoveType.Shield, Targets.Allies, selection, null, effectAnimation, description) 
    {
        AppliedShield = effect;
        ApplyEffect = ApplyShield;
        if(bonusEffect != null) {
            OnUse += bonusEffect;
        }
    }

    private void ApplyShield(Monster user, Vector2Int tile) {
        LevelGrid.Instance.GetMonster(tile).ApplyShield(AppliedShield);
        OnUse?.Invoke(user, tile);
    }
}
