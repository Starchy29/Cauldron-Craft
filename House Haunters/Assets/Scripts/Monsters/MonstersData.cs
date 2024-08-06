using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum MonsterName {
    LostSoul,
    Demon,
    Flytrap,
    Cactus,
    Golem,
    Automaton,
    Fungus,
    Jackolantern,
    Sludge,
    Fossil,
    // Beast,
    // Smog,
    // Slasher
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

        monsterTypes[(int)MonsterName.LostSoul] = new MonsterType(new List<Ingredient>() { Ingredient.Decay, Ingredient.Decay, Ingredient.Decay },
            18, 4,
            new List<Move>() {
                new Move("Revitalize", 1, MoveType.Heal, Move.Targets.Allies, new RangeSelector(2, false, true), (user, tile) => { LevelGrid.Instance.GetMonster(tile).Heal(4); }, null, "Heals an ally for 4 health."),
                new StatusMove("Haunt", 3, false, new StatusAilment(StatusEffect.Haunted, 3, prefabs.spookHaunt), RangeSelector.MeleeSelector, null, "The target takes 1.5x damage for 3 turns."),
                new Attack("Soul Drain", 1, 3, new RangeSelector(2, false, true), AnimateProjectile(prefabs.soulDrop, null, 6f, true), "Steals 3 health from the target.", StealHealth)
            }
        );

        monsterTypes[(int)MonsterName.Demon] = new MonsterType(new List<Ingredient>() { Ingredient.Decay, Ingredient.Decay, Ingredient.Decay },
            20, 4,
            new List<Move>() {
                new StatusMove("Ritual", 5, true, new StatusAilment(new List<StatusEffect>() { StatusEffect.Strength, StatusEffect.Haunted }, 3, prefabs.demonStrength), SelfSelector.Instance, null, "Increases damage dealt and taken by 1.5x for 3 turns"),
                new Move("Wither", 4, MoveType.Decay, Move.Targets.Enemies, RangeSelector.MeleeSelector, WitherStatus.Apply, null, "Deals 4 damage for 3 turns."),
                new Attack("Fireball", 1, 6, new RangeSelector(3, false, true), AnimateProjectile(prefabs.TempMonsterProjectile, prefabs.fireballBlast, 10f), "Deals 6 damage to the target and 4 damage to enemies adjacent to the target", (user, target, healthLost) => { DealSplashDamage(user, target.Tile, 4); })
            }
        );

        monsterTypes[(int)MonsterName.Cactus] = new MonsterType(new List<Ingredient>() { Ingredient.Flora, Ingredient.Flora, Ingredient.Flora },
            21, 3,
            new List<Move>() {
                new Move("Thorns", 2, MoveType.Boost, Move.Targets.Allies, new RangeSelector(4, true, true), ThornStatus.ApplyThorns, null, "For 3 turns, deal 6 damage to any enemy that strikes the target with a melee attack."),
                new ZoneMove("Spike Trap", 0, new RangeSelector(3, false, true), TileAffector.CreateBlueprint(prefabs.spikeTrapPrefab, 3, null, 0, (lander) => { lander.TakeDamage(4); }, true, true), null, "Places a trap that blocks enemies and deals 4 damage when they land in it."),
                new Attack("Barb Bullet", 1, 5, new DirectionSelector(6, true), AnimateLinearShot(prefabs.thornShot, null, 20f), "Deals 5 damage and pierces through enemies")
            }
        );

        monsterTypes[(int)MonsterName.Flytrap] = new MonsterType(new List<Ingredient>() { Ingredient.Flora, Ingredient.Flora, Ingredient.Flora },
            25, 3,
            new List<Move>() {
                new StatusMove("Nectar", 5, true, new StatusAilment(StatusEffect.Regeneration, 3, prefabs.nectarRegen), new RangeSelector(2, false, true), null, "Applies regeneration for 3 turns"),
                new Move("Vine Grab", 1, MoveType.Shift, Move.Targets.Enemies, new DirectionSelector(5, false), PullTarget, null, "Pulls the target towards the user"),
                new Attack("Chomp", 1, 8, RangeSelector.MeleeSelector, AnimateParticle(prefabs.chompTeeth), "Deals 8 damage to the target")
            }
        );

        monsterTypes[(int)MonsterName.Golem] = new MonsterType(new List<Ingredient>() { Ingredient.Mineral, Ingredient.Mineral, Ingredient.Mineral },
            20, 4,
            new List<Move>() {
                new ShieldMove("Crystal Casing", 2, new RangeSelector(3, true, true), new Shield(Shield.Strength.Medium, 1, false, prefabs.crystalShield), null, ""),
                new StatusMove("Aura Boost", 1, true, new StatusAilment(StatusEffect.Strength, 2, prefabs.auraStatus), new RangeSelector(2, false, true), null, ""),
                new Attack("Gem Bash", 1, 6, RangeSelector.MeleeSelector, AnimateParticle(prefabs.chompTeeth), "Deals 6 damage.")
            }
        );

        monsterTypes[(int)MonsterName.Automaton] = new MonsterType(new List<Ingredient>() { Ingredient.Mineral, Ingredient.Mineral, Ingredient.Mineral },
            22, 2,
            new List<Move>() {
                new StatusMove("Overdrive", 6, true, new StatusAilment(new List<StatusEffect>() { StatusEffect.Swiftness, StatusEffect.Energy }, 3, prefabs.overdriveStatus), SelfSelector.Instance, null, ""),
                new ShieldMove("Bastion", 1, ZoneSelector.AOESelector, new Shield(Shield.Strength.Weak, 1, false, prefabs.bastionShield), null, ""),
                new Attack("Flame Cannon", 1, 6, new RangeSelector(5, false, true), AnimateProjectile(prefabs.TempMonsterProjectile, prefabs.fireballBlast, 15f), "", (user, target, healthLost) => { DealSplashDamage(user, target.Tile, 4); })
            }
        );

        monsterTypes[(int)MonsterName.Fungus] = new MonsterType(new List<Ingredient>() { Ingredient.Decay, Ingredient.Decay, Ingredient.Flora },
            22, 3,
            new List<Move>() {
                new StatusMove("Sleepy Spores", 2, false, new StatusAilment(StatusEffect.Drowsiness, 2, prefabs.drowsySpores), RangeSelector.MeleeSelector, null, "The target is reduced to one action for 2 turns"),
                new StatusMove("Psychic Spores", 1, false, new StatusAilment(StatusEffect.Fear, 1, prefabs.fearSpores), new ZoneSelector(2, 2), AnimateParticle(prefabs.psychicBurst), "Halves the targets' damage for 1 turn"),
                new Move("Infect", 0, MoveType.Decay, Move.Targets.Enemies, new RangeSelector(2, false, true), LeechStatus.ApplyLeech, null, "Drains 2 health per turn for 3 turns")
            }
        );

        monsterTypes[(int)MonsterName.Jackolantern] = new MonsterType(new List<Ingredient>() { Ingredient.Decay, Ingredient.Flora, Ingredient.Flora },
            20, 4,
            new List<Move>() {
                new Move("Portal", 2, MoveType.Shift, Move.Targets.Allies, new RangeSelector(4, false, false), SwapPosition, null, "Swaps position with a nearby ally."),
                new ZoneMove("Will o' Wisps", 4, new ZoneSelector(2, 3), TileAffector.CreateBlueprint(prefabs.ExampleZone, 3, StatusEffect.Haunted, 0, null), null, "Creates a zone for three turns in which enemies take 1.5x damage"),
                new Attack("Hex", 1, 5, new RangeSelector(4, false, true), AnimateParticle(prefabs.hexBlast), "Deals 5 damage")
            }
        );

        monsterTypes[(int)MonsterName.Sludge] = new MonsterType(new List<Ingredient>() { Ingredient.Decay, Ingredient.Decay, Ingredient.Mineral },
            20, 4,
            new List<Move>() {
                new ZoneMove("Toxic Coating", 3, ZoneSelector.AOESelector, TileAffector.CreateBlueprint(prefabs.sludgeZone, 3, StatusEffect.Poison, 0, null), null, ""),
                new ShieldMove("Bubble", 1, new RangeSelector(3, false, true), new Shield(Shield.Strength.Weak, 3, true, prefabs.sludgeBubble), null, ""),
                new Attack("Blob Lob", 1, 4, new ZoneSelector(3, 2), null, "")
            }
        );

        monsterTypes[(int)MonsterName.Fossil] = new MonsterType(new List<Ingredient>() { Ingredient.Decay, Ingredient.Mineral, Ingredient.Mineral },
            22, 3,
            new List<Move>() {
                new ShieldMove("Rib Cage", 2, SelfSelector.Instance, new Shield(Shield.Strength.Strong, 1, true, prefabs.boneShield), null, ""),
                new ZoneMove("Quicksand", 3, new ZoneSelector(2, 2), TileAffector.CreateBlueprint(prefabs.quicksand, 3, null, 1, null), null, ""),
                new Attack("Bone Shot", 1, 6, new DirectionSelector(5, false), AnimateLinearShot(prefabs.boneShot, null, 16f), "")
            }
        );
    }

    public MonsterType GetMonsterData(MonsterName name) {
        return monsterTypes[(int)name];
    }

    #region Animation Helpers
    // these create functions that queue animations
    private static AnimationQueuer AnimateParticle(GameObject particlePrefab) {
        return new AnimationQueuer(false, (Monster user, List<Vector2Int> tiles) => {
            GameObject particle = GameObject.Instantiate(particlePrefab);
            particle.transform.position = Global.DetermineCenter(tiles);
        });
    }

    private static AnimationQueuer AnimateProjectile(GameObject projectilePrefab, GameObject destroyParticlePrefab, float speed, bool reversed = false) {
        return new AnimationQueuer(true, (Monster user, List<Vector2Int> tiles) => {
            LevelGrid level = LevelGrid.Instance;
            Vector3 start = level.Tiles.GetCellCenterWorld((Vector3Int)user.Tile);
            Vector3 end = level.Tiles.GetCellCenterWorld((Vector3Int)tiles[0]);
            AnimationsManager.Instance.QueueAnimation(new ProjectileAnimator(projectilePrefab, destroyParticlePrefab, reversed ? end : start, reversed ? start : end, speed));
        });
    }

    private static AnimationQueuer AnimateLinearShot(GameObject projectilePrefab, GameObject destroyParticlePrefab, float speed) {
        return new AnimationQueuer(false, (Monster user, List<Vector2Int> tiles) => {
            LevelGrid level = LevelGrid.Instance;
            Vector3 start = level.Tiles.GetCellCenterWorld((Vector3Int)user.Tile);
            Vector3 direction = ((Vector2)(tiles[0] - user.Tile)).normalized;
            Vector2Int furthestTile = tiles.Max((Vector2Int tile) => { return Global.CalcTileDistance(tile, user.Tile); });
            Vector3 end = level.Tiles.GetCellCenterWorld((Vector3Int)furthestTile);
            AnimationsManager.Instance.QueueAnimation(new ProjectileAnimator(projectilePrefab, destroyParticlePrefab, start, end, speed));
        });
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
                target.ReceiveAttack(Mathf.FloorToInt(damage), attacker);
            }
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

            if(level.IsOpenTile(testTile)) {
                furthestPull = testTile;
            }
        }

        AnimationsManager.Instance.QueueAnimation(new PathAnimator(target, new List<Vector3>() { level.Tiles.GetCellCenterWorld((Vector3Int)furthestPull) }, 15f));
        level.MoveEntity(target, furthestPull);
    }

    private static void SwapPosition(Monster user, Vector2Int targetTile) {
        LevelGrid level = LevelGrid.Instance;
        Monster target = level.GetMonster(targetTile);

        Vector2Int userTile = user.Tile;
        level.ClearEntity(userTile);
        level.MoveEntity(target, userTile);
        level.PlaceEntity(user, targetTile);

        user.transform.position = level.Tiles.GetCellCenterWorld((Vector3Int)user.Tile);
        target.transform.position = level.Tiles.GetCellCenterWorld((Vector3Int)target.Tile);
    }
    #endregion
}
