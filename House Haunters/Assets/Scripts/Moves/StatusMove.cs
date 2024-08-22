using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusMove : Move
{
    private StatusAilment condition;
    public StatusAilment Condition { get { return condition; } }

    private event EffectFunction extraEffect;

    public StatusMove(string name, int cooldown, bool forAllies, StatusAilment condition, ISelector selection, AnimationQueuer animation, string description = "", EffectFunction extraEffect = null)
        : base(name, cooldown, DetermineMoveType(condition, forAllies), forAllies ? Targets.Allies : Targets.Enemies, selection, null, animation, description)
    {
        this.condition = condition;
        ApplyEffect = ApplyStatus;
        if(extraEffect != null) {
            this.extraEffect += extraEffect;
        }
    }

    private void ApplyStatus(Monster user, Vector2Int tile) {
        LevelGrid.Instance.GetMonster(tile).ApplyStatus(condition, user);
        extraEffect?.Invoke(user, tile);
    }

    private static MoveType DetermineMoveType(StatusAilment condition, bool forAllies) {
        if(condition.effects.Contains(StatusEffect.Wither)) {
            return MoveType.Decay;
        }

        return forAllies ? MoveType.Boost : MoveType.Disrupt;
    }
}
