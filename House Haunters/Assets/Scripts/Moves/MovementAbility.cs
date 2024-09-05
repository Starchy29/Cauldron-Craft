using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MovementAbility : Move
{
    // constructor for a default walk move
    public MovementAbility(string name) : base(name, 1, MoveType.Movement, Targets.StandableSpot, PathSelector.Singleton, null, AnimateWalk, "Reposition to a nearby tile.") {
        ApplyEffect = MoveMonster;
    }

    private void MoveMonster(Monster user, Vector2Int tile) {
        LevelGrid.Instance.MoveEntity(user, tile);
    }

    private static void AnimateWalk(Monster user, Selection targets) {
        List<Vector3> pathWorldLocations = user.FindPath(targets.Filtered[0]).ConvertAll((Vector2Int tile) => { return LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)tile); });
        AnimationsManager.Instance.QueueAnimation(new PathAnimator(user, pathWorldLocations, 2f * user.CurrentSpeed));
    }
}
