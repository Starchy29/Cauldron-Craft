using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Move
{
    public enum Targets {
        Allies,
        Enemies,
        Floor,
        SpecificTile
    }

    public Selector Selection { get; private set; }
    public Targets TargetType { get; private set; }
    public int Cooldown { get; private set; }

    public Move(int cooldown, Targets targetType, Selector selection) {
        Cooldown = cooldown;
        Selection = selection;
        TargetType = targetType;
    }

    public void Use(Monster user, List<Vector2Int> tiles) {
        List<Vector2Int> affectedTiles = null;
        switch(TargetType) {
            case Targets.Allies:
                affectedTiles = tiles.Filter((Vector2Int tile) => {
                    GridEntity entity = LevelGrid.Instance.GetEntity(tile);
                    return entity != null && entity is Monster && ((Monster)entity).OnPlayerTeam == user.OnPlayerTeam;
                });
                break;
            case Targets.Enemies:
                affectedTiles = tiles.Filter((Vector2Int tile) => {
                    GridEntity entity = LevelGrid.Instance.GetEntity(tile);
                    return entity != null && entity is Monster && ((Monster)entity).OnPlayerTeam != user.OnPlayerTeam;
                });
                break;
            case Targets.Floor:
                affectedTiles = tiles.Filter((Vector2Int tile) => {
                    return LevelGrid.Instance.GetTile(tile).Walkable;
                });
                break;
            case Targets.SpecificTile:
                // used for moves like movements which validate their choices through selection
                affectedTiles = tiles;
                break;
        }

        foreach(Vector2Int tile in affectedTiles) {
            ApplyEffect(user, tile);
        }
    }

    protected abstract void ApplyEffect(Monster user, Vector2Int tile);
}
