using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveType {
    Movement,
    Attack,
    Heal,
    Decay,
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
        StandableSpot
    }

    private ISelector selection;
    public int Range { get { return selection.Range; } }

    public Targets TargetType { get; private set; }
    public MoveType Type { get; private set; }
    public int Cooldown { get; private set; }
    public bool CantWalkFirst { get; set; }

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
        { Targets.StandableSpot, IsStandable }
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
    public List<Selection> GetOptions(Monster user, bool ignoreUseless = true) {
        List<List<Vector2Int>> group = selection.GetSelectionGroups(user);

        List<Selection> options = group.ConvertAll((List<Vector2Int> unfiltered) => { 
            return new Selection(unfiltered, unfiltered.FindAll((Vector2Int tile) => TargetFilters[TargetType](user, tile)));
        });

        if(ignoreUseless) {
            options = options.FindAll((Selection group) => group.Filtered.Count > 0);
        }

        return options;
    }

    public void Use(Monster user, Selection targets) {
        if(effectAnimation != null) {
            effectAnimation.QueueAnimation(user, effectAnimation.UseFilteredSelection ? targets.Filtered : targets.Unfiltered);
        }

        foreach(Vector2Int tile in targets.Filtered) {
            ApplyEffect(user, tile);
        }
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
        return LevelGrid.Instance.IsOpenTile(tile);
    }
    #endregion
}
