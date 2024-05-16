using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAbility : Move
{
    // constructor for a default walk move
    public MovementAbility() : base("Move", 1, MoveType.Movement, Targets.Traversable, new RangeSelector((Monster user) => user.CurrentSpeed, false, false), null, AnimateWalk, "Reposition to a nearby tile") {
        ApplyEffect = MoveMonster;
    }

    // teleport ability
    public MovementAbility(string name, string description, int cooldown, ISelector selection) : base(name, cooldown, MoveType.Movement, Targets.StandableSpot, selection, null, null, description) {
        ApplyEffect = MoveMonster;
    }

    private void MoveMonster(Monster user, Vector2Int tile) {
        LevelGrid.Instance.MoveEntity(user, tile);
    }

    private static void AnimateWalk(Monster user, List<Vector2Int> tiles) {
        List<Vector3> pathWorldLocations = user.FindPath(tiles[0]).Map((Vector2Int tile) => { return LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)tile); });
        AnimationsManager.Instance.QueueAnimation(new PathAnimator(user.gameObject, pathWorldLocations, 2f * user.Stats.Speed));
    }
}
