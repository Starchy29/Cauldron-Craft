using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum MonsterName {
    Temporary,
    LostSoul,
    Demon,
    Flytrap,
    ThornBush,
    Mushroom,
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
    public Dictionary<MonsterName, Sprite> monsterToSprite;

    // define the stats and abilities of all monster types
    private MonstersData() {
        PrefabContainer prefabs = PrefabContainer.Instance;

        monsterToSprite = new Dictionary<MonsterName, Sprite>() {
            { MonsterName.Temporary, prefabs.tempMonsterSprite },
            { MonsterName.Demon, prefabs.demonSprite },
            { MonsterName.LostSoul, prefabs.soulSprite }
        };

        monsterTypes = new MonsterType[Enum.GetValues(typeof(MonsterName)).Length];

        monsterTypes[(int)MonsterName.Temporary] = new MonsterType(Ingredient.Decay, Ingredient.Decay, Ingredient.Decay,
            10, 3,
            new List<Move>() {
                new Attack("Attack", 0, 1, new RangeSelector(4, false, true), AnimateProjectile(prefabs.TempMonsterProjectile, null, 8.0f)),
                new ZoneMove("Poison Zone", 5, new ZoneSelector(2, 3), new TileEffect(StatusEffect.Poison, 0, 3, prefabs.ExampleZone, null), null, ""),
                new ShieldMove("Block", 2, new SelfSelector(), new Shield(Shield.Strength.Medium, 1, false, false, prefabs.ExampleShield), null)
            }
        );

        monsterTypes[(int)MonsterName.LostSoul] = new MonsterType(Ingredient.Decay, Ingredient.Decay, Ingredient.Decay,
            18, 4,
            new List<Move>() {
                new UniqueMove("Revitalize", 2, MoveType.Support, Move.Targets.Allies, new RangeSelector(2, false, true), (user, tile) => { LevelGrid.Instance.GetMonster(tile).Heal(3); }, null),
                new StatusMove("Spook", 3, StatusEffect.Haunted, 3, true, new RangeSelector(1, false, false), null),
                new Attack("Spirit Drain", 0, 2, new RangeSelector(1, false, false), null, "Steals the target's health.", StealHealth)
            }
        );

        monsterTypes[(int)MonsterName.Demon] = new MonsterType(Ingredient.Decay, Ingredient.Decay, Ingredient.Decay,
            20, 4,
            new List<Move>() {
                new StatusMove("Sacrifice", 5, StatusEffect.Strength, 3, false, new SelfSelector(), null, "Pay 3 life to gain strength.", (user, tile) => { user.TakeDamage(3, null); }),
                new StatusMove("Ritual", 2, StatusEffect.Cursed, 2, true, new ZoneSelector(2, 2), null),
                new Attack("Fireball", 0, 4, new RangeSelector(3, false, true), AnimateProjectile(prefabs.TempMonsterProjectile, null, 10f), "Deals 2 splash damage to nearby enemies.", (user, target, healthLost) => { DealSplashDamage(user, target.Tile, 2); })
            }
        );
    }

    public MonsterType GetMonsterData(MonsterName name) {
        return monsterTypes[(int)name];
    }

    // creates the function that queues the animation of a projectile
    private static AnimationQueuer AnimateProjectile(GameObject projectilePrefab, GameObject destroyParticlePrefab, float speed) {
        return (Monster user, List<Vector2Int> tiles) => {
            LevelGrid level = LevelGrid.Instance;
            Vector3 start = level.Tiles.GetCellCenterWorld((Vector3Int)user.Tile);
            Vector3 end = level.Tiles.GetCellCenterWorld((Vector3Int)tiles[0]);
            AnimationsManager.Instance.QueueAnimation(new ProjectileAnimator(projectilePrefab, destroyParticlePrefab, start, end, speed));
        };
    }

    private void StealHealth(Monster user, Monster target, int healthLost) {
        user.Heal(healthLost);
    }

    private void DealSplashDamage(Monster attacker, Vector2Int center, int damage) {
        List<Monster> targets = LevelGrid.Instance.GetTilesInRange(center, 1, true)
            .Filter((Vector2Int tile) => { return Move.IsEnemyOn(attacker, tile); })
            .Map((Vector2Int tile) => { return LevelGrid.Instance.GetMonster(tile); });

        foreach(Monster target in targets) {
            if(target.Tile != center) {
                target.TakeDamage(damage, attacker);
            }
        }
    }
}
