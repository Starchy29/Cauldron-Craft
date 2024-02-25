using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusMove : Move
{
    private StatusEffect status;
    private int duration;

    public delegate void ExtraEffect(Monster user, Vector2Int tile);
    private ExtraEffect extraEffect;

    public StatusMove(string name, int cooldown, StatusEffect status, int duration, bool isNegative, ISelector selection, AnimationQueuer animation, string description = "", ExtraEffect extraEffect = null)
        : base(name, cooldown, isNegative ? MoveType.Disrupt : MoveType.Support, isNegative ? Targets.Enemies : Targets.Allies, selection, animation, description)
    {
        this.status = status;
        this.duration = duration;
        this.extraEffect = extraEffect;
    }

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        LevelGrid.Instance.GetMonster(tile).ApplyStatus(status, duration);
        if(extraEffect != null) {
            extraEffect(user, tile);
        }
    }
}
