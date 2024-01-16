using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAbility : Move
{
    // default constructor for a character's regular walk ability
    public MovementAbility() : base(1, MoveType.Movement, Targets.SpecificTile, new WalkSelector()) {}

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        LevelGrid.Instance.MoveEntity(user, tile);
    }
}
