using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAbility : Move
{
    // default constructor for a character's regular walk ability
    public MovementAbility() : base(1, Move.Targets.SpecificTile, new WalkSelector()) {}

    public MovementAbility(int cooldown, Selector selection) : base(cooldown, Move.Targets.SpecificTile, selection) {}

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        LevelGrid.Instance.MoveEntity(user, tile);
    }
}
