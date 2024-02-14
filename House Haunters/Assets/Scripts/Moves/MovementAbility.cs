using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAbility : Move
{
    // constructor for a default walk move
    public MovementAbility() : base("Move", 1, MoveType.Movement, Targets.Traversable, new RangeSelector((Monster user) => user.CurrentSpeed, false, false)) {}

    // teleport ability
    public MovementAbility(string name, string description, int cooldown, ISelector selection) : base(name, cooldown, MoveType.Movement, Targets.StandableSpot, selection, description) {}

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        List<Vector3> pathWorldLocations = user.FindPath(tile).Map((Vector2Int tile) => { return LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)tile); });
        AnimationsManager.Instance.QueueAnimation(new PathAnimator(user.gameObject, pathWorldLocations, 2f * user.Stats.Speed));
        LevelGrid.Instance.MoveEntity(user, tile);
    }
}
