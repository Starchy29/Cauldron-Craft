using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAbility : Move
{
    // constructor for a default walk move
    public MovementAbility() : base(1, MoveType.Movement, Targets.Traversable, new RangeSelector((Monster user) => user.CurrentSpeed, false, false)) {}

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        LevelGrid.Instance.MoveEntity(user, tile);
    }
}
