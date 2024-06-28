using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveType {
    Movement,
    RangedAttack,
    MeleeAttack,
    Heal,
    Decay,
    Shield,
    Boost,
    Disrupt,
    Shift,
    Terrain
}

public class Move {
    public enum Targets {
        Allies,
        Enemies,
        ZonePlaceable,
        StandableSpot,
        Prefiltered
    }

    private ISelector selection;

    public Targets TargetType { get; private set; }
    public MoveType Type { get; private set; }
    public int Cooldown { get; private set; }

    public string Name { get; private set; }
    public string Description { get; private set; }

    private AnimationQueuer effectAnimation;

    public delegate void EffectFunction(Monster user, Vector2Int tile);
    protected EffectFunction ApplyEffect;

    public delegate bool FilterCheck(Monster user, Vector2Int tile);
    public static Dictionary<Targets, FilterCheck> TargetFilters { get; private set; } = new Dictionary<Targets, FilterCheck>() {
        { Targets.Allies, IsAllyOn },
        { Targets.Enemies, IsEnemyOn },
        { Targets.ZonePlaceable, CanPlaceZoneAt },
        { Targets.StandableSpot, IsStandable },
        { Targets.Prefiltered, IsFiltered }
    };

    public Move(string name, int cooldown, MoveType type, Targets targetType, ISelector selection, EffectFunction effect, AnimationQueuer effectAnimation, string description = "") {
        ApplyEffect = effect;
        Cooldown = cooldown;
        this.selection = selection;
        TargetType = targetType;
        Type = type;
        Name = name == null? "" : name;
        Description = description;
        this.effectAnimation = effectAnimation;
    }

    // has options to filter down to options with at least one target as well as filter out useless tiles
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
        List<Vector2Int> filteredTiles = tiles;
        filteredTiles = tiles.Filter((Vector2Int tile) => { return TargetFilters[TargetType](user, tile); });

        if(effectAnimation != null) {
            effectAnimation.QueueAnimation(user, effectAnimation.UseFilteredSelection ? filteredTiles : tiles);
        } else {
            // for moves that temporarily have no animation
            AnimationsManager.Instance.QueueAnimation(new PauseAnimator(0.2f));
        }

        foreach(Vector2Int tile in filteredTiles) {
            ApplyEffect(user, tile);
        }
    }

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

    public static bool CanPlaceZoneAt(Monster user, Vector2Int tile) {
        WorldTile spot = LevelGrid.Instance.GetTile(tile);
        return spot.Walkable && (spot.CurrentEffect == null || spot.CurrentEffect.Controller == user.Controller);
    }

    public static bool IsStandable(Monster user, Vector2Int tile) {
        return user.CanMoveTo(tile);
    }

    public static bool IsFiltered(Monster user, Vector2Int tile) {
        return true; // some moves, specifically movement, can use the selector to filter beforehand for better performance
    }
    #endregion
}
