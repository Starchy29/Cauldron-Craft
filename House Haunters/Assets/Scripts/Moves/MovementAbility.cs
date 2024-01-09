using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAbility : Move
{
    public MovementAbility(int cost, int speedModifier) : base(cost, new MovementSelector(speedModifier)) { }

    public override void Use(Monster user, List<Vector2Int> tiles) {
        LevelGrid.Instance.MoveEntity(user, tiles[0]);
    }
}
