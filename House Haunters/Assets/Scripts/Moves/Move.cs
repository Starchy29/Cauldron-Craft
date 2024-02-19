using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void AnimationQueuer(Monster user, List<Vector2Int> tiles);

public enum MoveType {
    Movement,
    Attack,
    Shield,
    Status,
    Zone
}

public abstract class Move
{
    public enum Targets {
        Allies,
        Enemies,
        UnaffectedFloor,
        StandableSpot,
        Traversable
    }

    private ISelector selection;

    public Targets TargetType { get; private set; }
    public MoveType Type { get; private set; }
    public int Cooldown { get; private set; }

    public string Name { get; private set; }
    public string Description { get; private set; }

    private AnimationQueuer effectAnimation;

    private delegate bool FilterCheck(Monster user, Vector2Int tile);
    private static Dictionary<Targets, FilterCheck> TargetFilters = new Dictionary<Targets, FilterCheck>() {
        { Targets.Allies, IsAllyOn },
        { Targets.Enemies, IsEnemyOn },
        { Targets.UnaffectedFloor, IsFloorAt },
        { Targets.StandableSpot, IsStandable },
        { Targets.Traversable, IsTraversable }
    };

    public Move(string name, int cooldown, MoveType type, Targets targetType, ISelector selection, AnimationQueuer effectAnimation, string description = "") {
        Cooldown = cooldown;
        this.selection = selection;
        TargetType = targetType;
        Type = type;
        Name = name == null? "" : name;
        Description = description;
        this.effectAnimation = effectAnimation;
    }

    // filters down the selection groups to make sure each option has at least one valid target
    public List<List<Vector2Int>> GetOptions(Monster user) {
        return selection.GetSelectionGroups(user)
            .Map((List<Vector2Int> group) => { return group.Filter((Vector2Int tile) => { return TargetFilters[TargetType](user, tile); }); })
            .Filter((List<Vector2Int> selectionGroup) => { return selectionGroup.Count > 0; });
    }

    // allows the UI to show the range of moves, duplicates are allowed
    public List<Vector2Int> GetCoveredArea(Monster user) {
        List<List<Vector2Int>> options = TargetType == Targets.Traversable ? GetOptions(user) : selection.GetSelectionGroups(user);
        List<Vector2Int> result = new List<Vector2Int>();
        foreach(List<Vector2Int> option in options) {
            result.AddRange(option);
        }
        return result;
    }

    public void Use(Monster user, List<Vector2Int> tiles) {
        if(effectAnimation != null) {
            effectAnimation(user, tiles);
        }

        foreach(Vector2Int tile in tiles) {
            ApplyEffect(user, tile);
        }
    }

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
        WorldTile spot = LevelGrid.Instance.GetTile(tile);
        return spot.Walkable && spot.CurrentEffect == null;
    }

    private static bool IsStandable(Monster user, Vector2Int tile) {
        return user.CanStandOn(tile);
    }

    private static bool IsTraversable(Monster user, Vector2Int tile) {
        return user.FindPath(tile) != null;
    }
    #endregion
}
