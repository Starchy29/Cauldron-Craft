using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusMove : Move
{
    private StatusAilment condition;

    public delegate void ExtraEffect(Monster user, Vector2Int tile);
    private ExtraEffect extraEffect;

    public StatusMove(string name, int cooldown, bool forAllies, StatusAilment condition, ISelector selection, AnimationQueuer animation, string description = "", ExtraEffect extraEffect = null)
        : base(name, cooldown, forAllies ? MoveType.Support : MoveType.Disrupt, forAllies ? Targets.Allies : Targets.Enemies, selection, animation, description)
    {
        this.condition = condition;
        this.extraEffect = extraEffect;
    }

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        LevelGrid.Instance.GetMonster(tile).ApplyStatus(condition, user);
        if(extraEffect != null) {
            extraEffect(user, tile);
        }
    }
}
