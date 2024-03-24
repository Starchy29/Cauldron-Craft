using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum MonsterName {
    LostSoul,
    Demon,
    ThornBush,
    Flytrap,
    //Mushroom,
    // Jackolantern
    //Temporary,
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

        //monsterTypes[(int)MonsterName.Temporary] = new MonsterType(Ingredient.Decay, Ingredient.Decay, Ingredient.Decay,
        //    10, 3,
        //    new List<Move>() {
        //        new Attack("Attack", 0, 1, new RangeSelector(4, false, true), AnimateProjectile(prefabs.TempMonsterProjectile, null, 8.0f)),
        //        new ZoneMove("Poison Zone", 5, new ZoneSelector(2, 3), new TileEffect(StatusEffect.Poison, 0, 3, prefabs.ExampleZone, null), null, ""),
        //        new ShieldMove("Block", 2, new SelfSelector(), new Shield(Shield.Strength.Medium, 1, false, false, prefabs.ExampleShield), null)
        //    }
        //);

        monsterTypes[(int)MonsterName.LostSoul] = new MonsterType(Ingredient.Decay, Ingredient.Decay, Ingredient.Decay,
            18, 4,
            new List<Move>() {
                new UniqueMove("Revitalize", 2, MoveType.Support, Move.Targets.Allies, new RangeSelector(2, false, true), (user, tile) => { LevelGrid.Instance.GetMonster(tile).Heal(3); }, null),
                new StatusMove("Spook", 3, StatusEffect.Haunted, 3, true, new RangeSelector(1, false, false), AnimateStatus(prefabs.spookHaunt, 3)),
                new Attack("Spirit Drain", 1, 2, new RangeSelector(2, false, false), null, "Steals the target's health.", StealHealth)
            }
        );

        monsterTypes[(int)MonsterName.Demon] = new MonsterType(Ingredient.Decay, Ingredient.Decay, Ingredient.Decay,
            20, 4,
            new List<Move>() {
                new StatusMove("Sacrifice", 5, StatusEffect.Strength, 3, false, new SelfSelector(), AnimateStatus(prefabs.demonStrength, 3), "Pay 3 life to gain strength.", (user, tile) => { user.TakeDamage(3, null); }),
                new StatusMove("Ritual", 2, StatusEffect.Cursed, 2, true, new ZoneSelector(2, 2), AnimateStatus(prefabs.demonCurse, 2)),
                new Attack("Fireball", 1, 4, new RangeSelector(3, false, true), AnimateProjectile(prefabs.TempMonsterProjectile, null, 10f), "Deals 2 damage to enemies adjacent to the target.", (user, target, healthLost) => { DealSplashDamage(user, target.Tile, 2); })
            }
        );

        monsterTypes[(int)MonsterName.ThornBush] = new MonsterType(Ingredient.Plant, Ingredient.Plant, Ingredient.Plant,
            22, 3,
            new List<Move>() {
                new ShieldMove("Thorn Guard", 1, new SelfSelector(), new Shield(Shield.Strength.Weak, 1, false, false, prefabs.thornShieldPrefab, DamageMeleeAttacker), null, "Deals 4 damage to enemies that attack this within melee range."),
                new ZoneMove("Spike Trap", 0, new RangeSelector(3, false, true), new TileEffect(null, 0, 4, prefabs.thornTrapPrefab, (lander) => { lander.TakeDamage(5, null); }, true), null, "Places a trap that deals 5 damage to an enemy that lands on it."),
                new Attack("Barb Bullet", 0, 4, new DirectionSelector(6, true), null, "Pierces enemies.")
            }
        );

        monsterTypes[(int)MonsterName.Flytrap] = new MonsterType(Ingredient.Plant, Ingredient.Plant, Ingredient.Plant,
            24, 4,
            new List<Move>() {
                new StatusMove("Sweet Nectar", 4, StatusEffect.Regeneration, 3, false, new RangeSelector(2, false, true), AnimateStatus(prefabs.nectarRegen, 3)),
                new StatusMove("Entangle", 1, StatusEffect.Slowness, 2, true, new RangeSelector(2, false, true), AnimateStatus(prefabs.tangleVines, 2)),
                new Attack("Chomp", 0, 6, new RangeSelector(1, false, false), null)
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

    private static AnimationQueuer AnimateStatus(GameObject effectParticlePrefab, int duration) {
        LevelGrid level = LevelGrid.Instance;
        return (Monster user, List<Vector2Int> tiles) => {
            foreach(Vector2Int tile in tiles) {
                Monster target = level.GetMonster(tile);
                if(target != null) {
                    AnimationsManager.Instance.QueueAnimation(new StatusApplicationAnimator(target, effectParticlePrefab, duration));
                }
            }
        };
    }
    #endregion

    private static void StealHealth(Monster user, Monster target, int healthLost) {
        user.Heal(healthLost);
    }

    private static void DealSplashDamage(Monster attacker, Vector2Int center, int damage) {
        List<Monster> targets = LevelGrid.Instance.GetTilesInRange(center, 1, true)
            .Filter((Vector2Int tile) => { return Move.IsEnemyOn(attacker, tile); })
            .Map((Vector2Int tile) => { return LevelGrid.Instance.GetMonster(tile); });

        foreach(Monster target in targets) {
            if(target.Tile != center) {
                target.TakeDamage(damage, attacker);
            }
        }
    }

    private static void DamageMeleeAttacker(Monster attacker, Monster defender) {
        if(Global.IsAdjacent(attacker.Tile, defender.Tile)) {
            attacker.TakeDamage(4, null);
        }
    }
}
