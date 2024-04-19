using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum MonsterName {
    LostSoul,
    Demon,
    ThornBush,
    Flytrap,
    Fungus,
    Jackolantern
}

public class MonstersData
{
    private static MonstersData instance;
    public static MonstersData Instance { get {
        if(instance == null) {
            instance = new MonstersData();
        }
        return instance;
    } }

    private MonsterType[] monsterTypes; // index is name enum cast to an int

    // define the stats and abilities of all monster types
    private MonstersData() {
        PrefabContainer prefabs = PrefabContainer.Instance;

        monsterTypes = new MonsterType[Enum.GetValues(typeof(MonsterName)).Length];

        monsterTypes[(int)MonsterName.LostSoul] = new MonsterType(Ingredient.Decay, Ingredient.Decay, Ingredient.Decay,
            18, 4,
            new List<Move>() {
                new UniqueMove("Revitalize", 1, MoveType.Support, Move.Targets.Allies, new RangeSelector(2, false, true), (user, tile) => { LevelGrid.Instance.GetMonster(tile).Heal(4); }, null),
                new StatusMove("Haunt", 3, false, new StatusAilment(StatusEffect.Haunted, 3, prefabs.spookHaunt), new RangeSelector(1, false, false), null),
                new Attack("Soul Drain", 1, 3, new RangeSelector(2, false, false), null, "Steals the target's health.", StealHealth)
            }
        );

        monsterTypes[(int)MonsterName.Demon] = new MonsterType(Ingredient.Decay, Ingredient.Decay, Ingredient.Decay,
            20, 4,
            new List<Move>() {
                new StatusMove("Ritual", 5, true, new StatusAilment(new List<StatusEffect>() { StatusEffect.Strength, StatusEffect.Haunted }, 3, prefabs.demonStrength), new SelfSelector(), null, "do later."),
                new Attack("Void Grasp", 2, 8, new RangeSelector(1, false, false), null, "Curses the target for 2 turns.", ApplyStatusOnHit(new StatusAilment(StatusEffect.Cursed, 2, prefabs.demonCurse))),
                new Attack("Fireball", 1, 6, new RangeSelector(3, false, true), AnimateProjectile(prefabs.TempMonsterProjectile, null, 10f), "Deals 4 damage to enemies adjacent to the target.", (user, target, healthLost) => { DealSplashDamage(user, target.Tile, 4); })
            }
        );

        monsterTypes[(int)MonsterName.ThornBush] = new MonsterType(Ingredient.Flora, Ingredient.Flora, Ingredient.Flora,
            22, 4,
            new List<Move>() {
                new ShieldMove("Thorn Guard", 1, new SelfSelector(), new Shield(Shield.Strength.Weak, 1, false, false, prefabs.thornShieldPrefab, DamageMeleeAttacker), null, "Deals 6 damage to enemies that attack this within melee range."),
                new ZoneMove("Spike Trap", 0, new RangeSelector(3, false, true), new TileEffect(null, 0, 4, prefabs.thornTrapPrefab, (lander) => { lander.TakeDamage(5, null); }, true), null, "Places a trap that deals 5 damage to an enemy that lands on it."),
                new Attack("Barb Bullet", 0, 6, new DirectionSelector(6, true), AnimateLinearShot(prefabs.thornShot, null, 20f, 6), "Pierces through enemies.")
            }
        );

        monsterTypes[(int)MonsterName.Flytrap] = new MonsterType(Ingredient.Flora, Ingredient.Flora, Ingredient.Flora,
            24, 3,
            new List<Move>() {
                new StatusMove("Sweet Nectar", 4, true, new StatusAilment(StatusEffect.Regeneration, 3, prefabs.nectarRegen), new RangeSelector(2, false, true), null),
                new UniqueMove("Vine Grab", 2, MoveType.Movement, Move.Targets.Enemies, new DirectionSelector(4, false), PullTarget, null, "Pulls the target towards the user."),
                new Attack("Chomp", 1, 8, new RangeSelector(1, false, false), null)
            }
        );

        monsterTypes[(int)MonsterName.Fungus] = new MonsterType(Ingredient.Decay, Ingredient.Decay, Ingredient.Flora,
            18, 3,
            new List<Move>() {
                new StatusMove("Sleepy Spores", 2, false, new StatusAilment(StatusEffect.Drowsiness, 2, prefabs.drowsySpores), new RangeSelector(1, false, false), null),
                new StatusMove("Psychedelic Spores", 1, false, new StatusAilment(StatusEffect.Fear, 1, prefabs.fearSpores), new ZoneSelector(2, 2), null),
                new UniqueMove("Infect", 0, MoveType.Disrupt, Move.Targets.Enemies, new RangeSelector(2, false, true), LeechStatus.ApplyLeech, null)
            }
        );

        monsterTypes[(int)MonsterName.Jackolantern] = new MonsterType(Ingredient.Decay, Ingredient.Flora, Ingredient.Flora,
            20, 4,
            new List<Move>() {
                new ShieldMove("Illuminate", 3, new ZoneSelector(1, 3), new Shield(Shield.Strength.None, 2, true, true, prefabs.illuminateShield), null),
                new ZoneMove("Will o' Wisps", 4, new ZoneSelector(3, 3), new TileEffect(StatusEffect.Haunted, 0, 3, prefabs.ExampleZone, null), null),
                new Attack("Hex", 1, 6, new RangeSelector(4, false, true), null, "Curses the target for one turn.", ApplyStatusOnHit(new StatusAilment(StatusEffect.Cursed, 1, prefabs.demonCurse)))
            }
        );
    }

    public MonsterType GetMonsterData(MonsterName name) {
        return monsterTypes[(int)name];
    }

    #region Animation Helpers
    // creates the function that queues the animation of a projectile
    private static AnimationQueuer AnimateProjectile(GameObject projectilePrefab, GameObject destroyParticlePrefab, float speed) {
        return (Monster user, List<Vector2Int> tiles) => {
            LevelGrid level = LevelGrid.Instance;
            Vector3 start = level.Tiles.GetCellCenterWorld((Vector3Int)user.Tile);
            Vector3 end = level.Tiles.GetCellCenterWorld((Vector3Int)tiles[0]);
            AnimationsManager.Instance.QueueAnimation(new ProjectileAnimator(projectilePrefab, destroyParticlePrefab, start, end, speed));
        };
    }

    private static AnimationQueuer AnimateLinearShot(GameObject projectilePrefab, GameObject destroyParticlePrefab, float speed, int tileRange) {
        return (Monster user, List<Vector2Int> tiles) => {
            LevelGrid level = LevelGrid.Instance;
            Vector3 start = level.Tiles.GetCellCenterWorld((Vector3Int)user.Tile);
            Vector3 direction = ((Vector2)(tiles[0] - user.Tile)).normalized;
            Vector3 end = level.Tiles.GetCellCenterWorld((Vector3Int)user.Tile) + tileRange * direction;
            AnimationsManager.Instance.QueueAnimation(new ProjectileAnimator(projectilePrefab, destroyParticlePrefab, start, end, speed));
        };
    }

    //private static AnimationQueuer AnimateStatus(GameObject effectParticlePrefab, int duration) {
    //    LevelGrid level = LevelGrid.Instance;
    //    return (Monster user, List<Vector2Int> tiles) => {
    //        foreach(Vector2Int tile in tiles) {
    //            Monster target = level.GetMonster(tile);
    //            if(target != null) {
    //                AnimationsManager.Instance.QueueAnimation(new StatusApplicationAnimator(target, effectParticlePrefab, duration));
    //            }
    //        }
    //    };
    //}
    #endregion

    #region bonus effects and special moves
    private static void StealHealth(Monster user, Monster target, int healthLost) {
        user.Heal(healthLost);
    }

    private static void DealSplashDamage(Monster attacker, Vector2Int center, int damage) {
        List<Monster> targets = LevelGrid.Instance.GetTilesInRange(center, 1, true)
            .Filter((Vector2Int tile) => { return Move.IsEnemyOn(attacker, tile); })
            .Map((Vector2Int tile) => { return LevelGrid.Instance.GetMonster(tile); });

        foreach(Monster target in targets) {
            if(target.Tile != center) {
                target.TakeDamage(Mathf.FloorToInt(damage * attacker.DamageMultiplier), attacker);
            }
        }
    }

    private static void DamageMeleeAttacker(Monster attacker, Monster defender) {
        if(Global.IsAdjacent(attacker.Tile, defender.Tile)) {
            attacker.TakeDamage(6, null);
        }
    }

    private static Attack.HitTrigger ApplyStatusOnHit(StatusAilment status) {
        return (Monster user, Monster target, int healthLost) => {
            target.ApplyStatus(status, user);
        };
    }

    private static void PullTarget(Monster user, Vector2Int tile) {
        LevelGrid level = LevelGrid.Instance;
        Monster target = level.GetMonster(tile);
        Vector2Int pullDirection = user.Tile - target.Tile;
        pullDirection /= (int)pullDirection.magnitude;
        Vector2Int furthestPull = target.Tile;
        for(Vector2Int testTile = target.Tile + pullDirection; testTile != user.Tile; testTile += pullDirection) {
            if(level.GetTile(testTile).IsWall) {
                break; // cannot pull through walls
            }

            if(target.CanMoveTo(testTile)) {
                furthestPull = testTile;
            }
        }

        level.MoveEntity(target, furthestPull);
        AnimationsManager.Instance.QueueAnimation(new PathAnimator(target.gameObject, new List<Vector3>() { level.Tiles.GetCellCenterWorld((Vector3Int)furthestPull) }, 15f));
    }
    #endregion
}
