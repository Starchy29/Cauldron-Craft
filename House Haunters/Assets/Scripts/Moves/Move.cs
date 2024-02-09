using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Move
{
    public enum Targets {
        Allies,
        Enemies,
        Floor,
        StandableSpot,
        Traversable
    }

    public enum MoveType {
        Movement,
        Attack,
        Shield,
        Status,
        Zone
    }

    private ISelector selection;

    public Targets TargetType { get; private set; }
    public MoveType Type { get; private set; }
    public int Cooldown { get; private set; }

    public string Name { get; private set; }
    public string Description { get; private set; }

    private delegate bool FilterCheck(Monster user, Vector2Int tile);
    private static Dictionary<Targets, FilterCheck> TargetFilters = new Dictionary<Targets, FilterCheck>() {
        { Targets.Allies, IsAllyOn },
        { Targets.Enemies, IsEnemyOn },
        { Targets.Floor, IsFloorAt },
        { Targets.StandableSpot, IsStandable },
        { Targets.Traversable, IsTraversable }
    };

    public Move(string name, int cooldown, MoveType type, Targets targetType, ISelector selection, string description = "") {
        Cooldown = cooldown;
        this.selection = selection;
        TargetType = targetType;
        Type = type;
        Name = name;
        Description = description;
    }

    // filters down the selection groups to make sure each option has at least one valid target
    public List<List<Vector2Int>> GetOptions(Monster user) {
        return selection.GetSelectionGroups(user).Filter((List<Vector2Int> selectionGroup) => { return HasValidTarget(user, selectionGroup); });
    }

    // allows the UI to show the range of moves, duplicates are allowed
    public List<Vector2Int> GetCoveredArea(Monster user) {
        List<List<Vector2Int>> options = selection.GetSelectionGroups(user);
        List<Vector2Int> result = new List<Vector2Int>();
        foreach(List<Vector2Int> option in options) {
            result.AddRange(option);
        }
        return result;
    }

    private bool HasValidTarget(Monster user, List<Vector2Int> selectionGroup) {
        foreach(Vector2Int tile in selectionGroup) {
            if(TargetFilters[TargetType](user, tile)) {
                return true;
            }
        }
        return false;
    }

    public void Use(Monster user, List<Vector2Int> tiles) {
        if(TargetType != Targets.Traversable) { // avoid pathfinding for an already validated path
            tiles = tiles.Filter((Vector2Int tile) => { return TargetFilters[TargetType](user, tile); });
        }

        QueueAnimations(user, tiles);

        foreach(Vector2Int tile in tiles) {
            ApplyEffect(user, tile);
        }
    }

    protected virtual void QueueAnimations(Monster user, List<Vector2Int> tiles) { }
    protected abstract void ApplyEffect(Monster user, Vector2Int tile);

    #region filter functions
    private static bool IsAllyOn(Monster user, Vector2Int tile) {
        Monster monster = LevelGrid.Instance.GetMonster(tile);
        return monster != null && monster.Controller == user.Controller;
    }

    private static bool IsEnemyOn(Monster user, Vector2Int tile) {
        Monster monster = LevelGrid.Instance.GetMonster(tile);
        return monster != null && monster.Controller != user.Controller;
    }

    private static bool IsFloorAt(Monster user, Vector2Int tile) {
        return LevelGrid.Instance.GetTile(tile).Walkable;
    }

    private static bool IsStandable(Monster user, Vector2Int tile) {
        return user.CanStandOn(tile);
    }

    private static bool IsTraversable(Monster user, Vector2Int tile) {
        return user.FindPath(tile) != null;
    }
    #endregion
}
