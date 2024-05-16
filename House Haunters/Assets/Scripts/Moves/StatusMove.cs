using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusMove : Move
{
    private StatusAilment condition;

    private event EffectFunction extraEffect;

    public StatusMove(string name, int cooldown, bool forAllies, StatusAilment condition, ISelector selection, AnimationQueuer animation, string description = "", EffectFunction extraEffect = null)
        : base(name, cooldown, forAllies ? MoveType.Support : MoveType.Disrupt, forAllies ? Targets.Allies : Targets.Enemies, selection, null, animation, description)
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
}
