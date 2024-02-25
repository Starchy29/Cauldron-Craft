using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void AnimationQueuer(Monster user, List<Vector2Int> tiles);

public enum MoveType {
    Movement,
    Attack,
    Shield,
    Support,
    Disrupt,
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

    // filters down the selection groups to be only tiles that pass the filter
    public List<List<Vector2Int>> GetOptions(Monster user, bool filtered = true, bool ignoreUseless = true) {
        List<List<Vector2Int>> group = selection.GetSelectionGroups(user);

        if(ignoreUseless) {
            group = group.Filter((List<Vector2Int> group) => { return HasValidOption(user, group); });
        }

        if(filtered) {
            group = group.Map((List<Vector2Int> group) => { 
                return group.Filter((Vector2Int tile) => { 
                    return TargetFilters[TargetType](user, tile); 
                }); 
            });
        }

        return group;
    }

    public void Use(Monster user, List<Vector2Int> tiles) {
        if(effectAnimation != null) {
            effectAnimation(user, tiles);
        }

        if(TargetType != Targets.Traversable) { // avoid pathfinding extra times
            tiles = tiles.Filter((Vector2Int tile) => { return TargetFilters[TargetType](user, tile); });
        }

        foreach(Vector2Int tile in tiles) {
            ApplyEffect(user, tile);
        }
    }

    protected abstract void ApplyEffect(Monster user, Vector2Int tile);

    private bool HasValidOption(Monster user, List<Vector2Int> tileGroup) {
        foreach(Vector2Int tile in tileGroup) {
            if(TargetFilters[TargetType](user, tile)) {
                return true;
            }
        }

        return false;
    }

    #region filter functions
    public static bool IsAllyOn(Monster user, Vector2Int tile) {
        Monster monster = LevelGrid.Instance.GetMonster(tile);
        return monster != null && monster.Controller == user.Controller;
    }

    public static bool IsEnemyOn(Monster user, Vector2Int tile) {
        Monster monster = LevelGrid.Instance.GetMonster(tile);
        return monster != null && monster.Controller != user.Controller;
    }

    public static bool IsFloorAt(Monster user, Vector2Int tile) {
        WorldTile spot = LevelGrid.Instance.GetTile(tile);
        return spot.Walkable && spot.CurrentEffect == null;
    }

    public static bool IsStandable(Monster user, Vector2Int tile) {
        return user.CanMoveTo(tile);
    }

    public static bool IsTraversable(Monster user, Vector2Int tile) {
        return user.FindPath(tile) != null;
    }
    #endregion
}
