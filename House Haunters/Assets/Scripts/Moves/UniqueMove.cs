using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniqueMove : Move
{
    public delegate void SpecialMoveEffect(Monster user, Vector2Int tile);
    private SpecialMoveEffect effect;

    public UniqueMove(string name, int cooldown, MoveType type, Targets targetType, ISelector selection, SpecialMoveEffect effect, AnimationQueuer effectAnimation, string description = "")
        : base(name, cooldown, type, targetType, selection, effectAnimation, description)
    {
        this.effect = effect;
    }

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        effect(user, tile);
    }
}
