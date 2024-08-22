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
    Sludge,
    Fungus,
    Fossil,
    Phantom,
    Jackolantern,
    Beast,
    Amalgamation
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
            23, 4,
            new Move("Revitalize", 1, MoveType.Heal, Move.Targets.Allies, new RangeSelector(3, false, true), (user, tile) => { LevelGrid.Instance.GetMonster(tile).Heal(5); }, null, "Heals an ally for 4 health."),
            new StatusMove("Haunt", 3, false, new StatusAilment(StatusEffect.Haunted, 3, prefabs.spookHaunt), RangeSelector.MeleeSelector, null, "The target takes 1.5x damage for 3 turns.")
        );

        monsterTypes[(int)MonsterName.Demon] = new MonsterType(new List<Ingredient>() { Ingredient.Decay, Ingredient.Decay, Ingredient.Decay },
            25, 4,
            new Attack("Fireball", 1, 8, new RangeSelector(3, false, true), AnimateProjectile(prefabs.TempMonsterProjectile, prefabs.fireballBlast, 10f), "Deals 6 damage to the target and 4 damage to enemies adjacent to the target", (user, target, healthLost) => { DealSplashDamage(user, target.Tile, 4); }),
            null
        );

        monsterTypes[(int)MonsterName.Cactus] = new MonsterType(new List<Ingredient>() { Ingredient.Flora, Ingredient.Flora, Ingredient.Flora },
            27, 3,
            new ZoneMove("Spike Trap", 0, new RangeSelector(3, false, true), TileAffector.CreateBlueprint(prefabs.spikeTrapPrefab, 3, 0, (lander) => { lander.TakeDamage(4); }, true, true), null, "Places a trap that blocks enemies and deals 4 damage when they land in it."),
            new Attack("Barb Bullet", 1, 6, new DirectionSelector(6, true), AnimateLinearShot(prefabs.thornShot, null, 20f), "Deals 5 damage and pierces through enemies")
        );

        tangledStatus = new StatusAilment(StatusEffect.Slowness, 2, prefabs.tangleVines);
        monsterTypes[(int)MonsterName.Flytrap] = new MonsterType(new List<Ingredient>() { Ingredient.Flora, Ingredient.Flora, Ingredient.Flora },
            28, 3,
            new Move("Vine Grab", 1, MoveType.Shift, Move.Targets.Enemies, new DirectionSelector(4, false), PullTarget, null, "Pulls the target towards the user"),
            new Attack("Chomp", 1, 10, RangeSelector.MeleeSelector, AnimateParticle(prefabs.chompTeeth), "Deals 8 damage to the target")
        );

        monsterTypes[(int)MonsterName.Golem] = new MonsterType(new List<Ingredient>() { Ingredient.Mineral, Ingredient.Mineral, Ingredient.Mineral },
            25, 4,
            // "Crystals"
            new StatusMove("Aura Boost", 1, true, new StatusAilment(StatusEffect.Strength, 2, prefabs.auraStatus), new RangeSelector(2, false, true), null, ""),
            new Attack("Gem Bash", 1, 8, RangeSelector.MeleeSelector, AnimateParticle(prefabs.chompTeeth), "Deals 6 damage.")
        );

        monsterTypes[(int)MonsterName.Automaton] = new MonsterType(new List<Ingredient>() { Ingredient.Mineral, Ingredient.Mineral, Ingredient.Mineral },
            28, 2,
            new Attack("Flame Cannon", 1, 7, new RangeSelector(5, false, true), AnimateProjectile(prefabs.TempMonsterProjectile, prefabs.fireballBlast, 15f), "", (user, target, healthLost) => { DealSplashDamage(user, target.Tile, 3); }),
            null
            
        );

        monsterTypes[(int)MonsterName.Fungus] = new MonsterType(new List<Ingredient>() { Ingredient.Flora, Ingredient.Flora, Ingredient.Decay },
            27, 3,
            new StatusMove("Psychic Spores", 1, false, new StatusAilment(StatusEffect.Fear, 1, prefabs.fearStatus), new ZoneSelector(2, 2), AnimateParticle(prefabs.psychicBurst), "Halves the targets' damage for 1 turn"),
            new Move("Infect", 0, MoveType.Decay, Move.Targets.Enemies, new RangeSelector(2, false, true), LeechStatus.ApplyLeech, null, "Drains 2 health per turn for 3 turns")
        );

        monsterTypes[(int)MonsterName.Jackolantern] = new MonsterType(new List<Ingredient>() { Ingredient.Flora, Ingredient.Flora, Ingredient.Mineral },
            23, 4,
            new Move("Portal", 2, MoveType.Shift, Move.Targets.Allies, new RangeSelector(4, false, false), SwapPosition, null, "Swaps position with a nearby ally."),
            new Move("Illunminate", 2, MoveType.Heal, Move.Targets.Allies, new ZoneSelector(2, 5), (user, tile) => { if(tile != user.Tile) LevelGrid.Instance.GetMonster(tile).Heal(3); }, null, "")
        );

        monsterTypes[(int)MonsterName.Sludge] = new MonsterType(new List<Ingredient>() { Ingredient.Decay, Ingredient.Decay, Ingredient.Flora },
            27, 4,
            new ZoneMove("Toxic Puddle", 3, ZoneSelector.AOESelector, TileAffector.CreateBlueprint(prefabs.sludgeZone, 3, 0, null, false, false, (Monster occupant) => { occupant.TakeDamage(5); }), null, ""),
            new Attack("Blob Lob", 1, 5, new ZoneSelector(3, 2), AnimateLobber(prefabs.sludgeLob, 2.0f, 0.6f), "")
        );

        monsterTypes[(int)MonsterName.Fossil] = new MonsterType(new List<Ingredient>() { Ingredient.Mineral, Ingredient.Mineral, Ingredient.Decay },
            25, 3,
            new ZoneMove("Quicksand", 3, new ZoneSelector(2, 2), TileAffector.CreateBlueprint(prefabs.quicksand, 3, 1, null), null, ""),
            new Attack("Bone Shot", 1, 8, new DirectionSelector(5, false), AnimateLinearShot(prefabs.boneShot, null, 16f), "")
        );

        monsterTypes[(int)MonsterName.Phantom] = new MonsterType(new List<Ingredient>() { Ingredient.Decay, Ingredient.Decay, Ingredient.Mineral },
            21, 5,
            new Move("Pierce", 3, MoveType.Shift, Move.Targets.StandableSpot, new DirectionSelector(3, false, false), DashSlash, null, ""),
            new Attack("Slash", 1, 9, RangeSelector.MeleeSelector, null, "")
        );

        monsterTypes[(int)MonsterName.Beast] = new MonsterType(new List<Ingredient>() { Ingredient.Mineral, Ingredient.Mineral, Ingredient.Flora },
            27, 4,
            new StatusMove("Battle Cry", 5, true, new StatusAilment(StatusEffect.Swiftness, 2, prefabs.beastSpeed), new ZoneSelector(2, 5), null, ""),
            new Attack("Claw", 1, 7, new ZoneSelector(1, 2), null, "")
        );

        monsterTypes[(int)MonsterName.Amalgamation] = new MonsterType(new List<Ingredient>() { Ingredient.Decay, Ingredient.Flora, Ingredient.Mineral },
            30, 3,
            new StatusMove("Horrify", 3, false, new StatusAilment(StatusEffect.Fear, 1, prefabs.fearStatus), new ZoneSelector(2, 5), null, ""),
            new Attack("Lash Out", 1, 6, new ZoneSelector(1, 3), null, "")
        );
    }

    public MonsterType GetMonsterData(MonsterName name) {
        return monsterTypes[(int)name];
    }

    #region Animation Helpers
    // these create objects that queue animations
    private static AnimationQueuer AnimateParticle(GameObject particlePrefab) {
        return new AnimationQueuer(false, (Monster user, List<Vector2Int> tiles) => {
            GameObject particle = GameObject.Instantiate(particlePrefab);
            particle.transform.position = Global.DetermineCenter(tiles);
            particle.SetActive(false);
            AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(particle, true));
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

    private static AnimationQueuer AnimateLobber(GameObject prefab, float height, float duration) {
        return new AnimationQueuer(false, (Monster user, List<Vector2Int> tiles) => {
            LevelGrid level = LevelGrid.Instance;
            Vector3 start = level.Tiles.GetCellCenterWorld((Vector3Int)user.Tile);
            Vector3 end = Global.DetermineCenter(tiles);
            AnimationsManager.Instance.QueueAnimation(new LobAnimator(prefab, start, end, height, duration));
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
                target.TakeDamage(Mathf.FloorToInt(damage), attacker);
            }
        }
    }

    private static StatusAilment tangledStatus;
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

        // apply slowness effect as well
        target.ApplyStatus(tangledStatus, user);
    }

    private static void SwapPosition(Monster user, Vector2Int targetTile) {
        LevelGrid level = LevelGrid.Instance;
        Monster target = level.GetMonster(targetTile);

        Vector2Int userTile = user.Tile;
        level.ClearEntity(userTile);
        level.MoveEntity(target, userTile);
        level.PlaceEntity(user, targetTile);

        AnimationsManager.Instance.QueueAnimation(new FunctionAnimator(() => {
            Vector3 targetPosition = target.transform.position;
            target.transform.position = user.transform.position;
            user.transform.position = targetPosition;
            user.UpdateSortingOrder();
            target.UpdateSortingOrder();
        }));
    }

    private static void DashSlash(Monster user, Vector2Int target) {
        LevelGrid level = LevelGrid.Instance;

        // move to the end tile
        Vector2Int start = user.Tile;
        AnimationsManager.Instance.QueueAnimation(new PathAnimator(user, new List<Vector3> { level.Tiles.GetCellCenterWorld((Vector3Int)target) }, 20f));
        level.MoveEntity(user, target);

        // damage all enemies passed through
        Vector2Int direction = target - start;
        if(direction.x == 0) {
            direction = new Vector2Int(0, direction.y > 0 ? 1 : -1);
        } else {
            direction = new Vector2Int(direction.x > 0 ? 1 : -1, 0);
        }

        for(Vector2Int tile = start + direction; tile != target; tile += direction) {
            Monster hit = level.GetMonster(tile);
            if(hit != null && hit.Controller != user.Controller) {
                hit.TakeDamage(7, user);
            }
        }
    }

    public const int SHOVE_DIST = 3;
    private static void Shove(Monster user, Vector2Int targetTile) {
        LevelGrid level = LevelGrid.Instance;
        Monster enemy = level.GetMonster(targetTile);

        const float SPEED = 18f;
        const int DAMAGE = 6;

        Vector2Int direction = targetTile - user.Tile;
        Vector2Int endTile = targetTile + SHOVE_DIST * direction;
        for(int i = 0; i < SHOVE_DIST; i++) {
            Vector2Int tile = targetTile + (i + 1) * direction;
            if(!level.IsInGrid(tile)) {
                endTile = tile - direction;
                break;
            }

            WorldTile terrain = level.GetTile(tile);
            GridEntity occupant = level.GetEntity(tile);

            if(occupant != null || terrain.IsWall) {
                // hit into another monster or crash into a wall
                AnimationsManager.Instance.QueueAnimation(new PathAnimator(enemy, new List<Vector3> { level.Tiles.GetCellCenterWorld((Vector3Int)tile) }, SPEED));
                AnimationsManager.Instance.QueueAnimation(new PathAnimator(enemy, new List<Vector3> { level.Tiles.GetCellCenterWorld((Vector3Int)(tile - direction)) }, SPEED));
                level.MoveEntity(enemy, tile - direction);

                if(occupant != null && occupant is Monster) {
                    ((Monster)occupant).TakeDamage(DAMAGE);
                }
                enemy.TakeDamage(DAMAGE);
                return;
            }

            if(!terrain.Walkable && !terrain.IsWall) {
                // fall into pit
                AnimationsManager.Instance.QueueAnimation(new PathAnimator(enemy, new List<Vector3> { level.Tiles.GetCellCenterWorld((Vector3Int)tile) }, SPEED));
                AnimationsManager.Instance.QueueAnimation(new FallAnimator(enemy.gameObject, level.Tiles.GetCellCenterWorld((Vector3Int)(tile - direction))));
                level.MoveEntity(enemy, tile - direction);
                enemy.TakeDamage(DAMAGE);
                return;
            }
        }

        // travel full distance
        AnimationsManager.Instance.QueueAnimation(new PathAnimator(enemy, new List<Vector3> { level.Tiles.GetCellCenterWorld((Vector3Int)endTile) }, SPEED));
        level.MoveEntity(enemy, endTile);
    }
    #endregion
}
