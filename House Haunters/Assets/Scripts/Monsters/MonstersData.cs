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

    private static StatusAilment tangledStatus;
    private static StatusAilment hexedStatus;

    // define the stats and abilities of all monster types
    private MonstersData() {
        PrefabContainer prefabs = PrefabContainer.Instance;

        monsterTypes = new MonsterType[Enum.GetValues(typeof(MonsterName)).Length];

        monsterTypes[(int)MonsterName.LostSoul] = new MonsterType(new List<Ingredient>() { Ingredient.Decay, Ingredient.Decay, Ingredient.Decay },
            26, 4,
            new Move("Revitalize", 1, MoveType.Heal, Move.Targets.Allies, new RangeSelector(2, false, true), (user, tile) => { LevelGrid.Instance.GetMonster(tile).Heal(6); }, AnimateLobber(prefabs.soulDrop, 1.5f, 0.5f), "Heals an ally for 6 health."),
            new StatusMove("Haunt", 2, new StatusAilment(StatusEffect.Haunt, 2, prefabs.spookHaunt), RangeSelector.MeleeSelector, null, "The target takes 1.5x damage for 2 turns.")
        );

        monsterTypes[(int)MonsterName.Demon] = new MonsterType(new List<Ingredient>() { Ingredient.Decay, Ingredient.Decay, Ingredient.Decay },
            24, 4,
            new Attack("Fireball", 1, 8, new RangeSelector(3, false, true), AnimateProjectile(prefabs.TempMonsterProjectile, prefabs.fireballBlast, 10f), "Deals 8 damage to the target and 5 damage to enemies adjacent to the target.", (user, target, healthLost) => { DealSplashDamage(user, target.Tile, 5); }),
            new Attack("Soul Drain", 3, 7, RangeSelector.MeleeSelector, AnimateProjectile(prefabs.soulDrop, null, 10f, true), "Deals 7 damage and heals equal to the health lost.", StealHealth)
        );

        monsterTypes[(int)MonsterName.Cactus] = new MonsterType(new List<Ingredient>() { Ingredient.Flora, Ingredient.Flora, Ingredient.Flora },
            28, 3,
            new Attack("Barb Bullet", 1, 7, new DirectionSelector(6, true), AnimateLinearShot(prefabs.thornShot, null, 20f), "Deals 7 damage and pierces through enemies."),
            new ZoneMove("Thorn Trap", 2, new RangeSelector(3, false, true), TileAffector.CreateBlueprint(prefabs.spikeTrapPrefab, 5, 0, (lander) => { lander.TakeDamage(6); }, true, true), null, "Places a trap that blocks enemies and deals 6 damage when stepped on.")
        );

        tangledStatus = new StatusAilment(StatusEffect.Slowness, 2, prefabs.tangleVines);
        monsterTypes[(int)MonsterName.Flytrap] = new MonsterType(new List<Ingredient>() { Ingredient.Flora, Ingredient.Flora, Ingredient.Flora },
            28, 3,
            new Attack("Chomp", 1, 12, RangeSelector.MeleeSelector, AnimateParticle(prefabs.chompTeeth), "Deals 12 damage to the target."),
            new Move("Vine Grasp", 2, MoveType.Shift, Move.Targets.Enemies, new DirectionSelector(4, false), PullTarget, null, "Pulls the target to the user and slows it for 2 turns.")
        );

        monsterTypes[(int)MonsterName.Golem] = new MonsterType(new List<Ingredient>() { Ingredient.Mineral, Ingredient.Mineral, Ingredient.Mineral },
            26, 5,
            new StatusMove("Aura Boost", 1, new StatusAilment(StatusEffect.Power, 1, prefabs.crystalShield), new RangeSelector(2, false, true), null, "Increases damage by 1.5x for 1 turn."),
            new StatusMove("Crystals", 2, new StatusAilment(StatusEffect.Sturdy, 2, prefabs.spikeShieldPrefab), new RangeSelector(1, false, true), null, "Halves damage received for 2 turns.")
        );

        monsterTypes[(int)MonsterName.Automaton] = new MonsterType(new List<Ingredient>() { Ingredient.Mineral, Ingredient.Mineral, Ingredient.Mineral },
            28, 2,
            new Attack("Flame Cannon", 1, 9, new ZoneSelector(4, 2), AnimateLobber(prefabs.TempMonsterProjectile, 4f, 1f), "Deals 9 damage."),
            new Move("Repair", 2, MoveType.Heal, Move.Targets.Allies, SelfSelector.Instance, (user, tile) => { LevelGrid.Instance.GetMonster(tile).Heal(5); }, null, "Heals 5 health.")
        );
        monsterTypes[(int)MonsterName.Automaton].Moves[MonsterType.PRIMARY_INDEX].CantWalkFirst = true;

        monsterTypes[(int)MonsterName.Fungus] = new MonsterType(new List<Ingredient>() { Ingredient.Flora, Ingredient.Flora, Ingredient.Decay },
            28, 3,
            new StatusMove("Infect", 1, new StatusAilment(StatusEffect.Poison, 3, prefabs.leechSeed), new RangeSelector(2, false, true), null, "Deals 5 damage for 3 turns."),
            new StatusMove("Psychic Spores", 1, new StatusAilment(StatusEffect.Fear, 2, prefabs.fearStatus), RangeSelector.MeleeSelector, AnimateParticle(prefabs.psychicBurst), "Halves the target's damage for 2 turns.")
        );

        hexedStatus = new StatusAilment(StatusEffect.Slowness, 1, prefabs.hexBlast);
        monsterTypes[(int)MonsterName.Jackolantern] = new MonsterType(new List<Ingredient>() { Ingredient.Flora, Ingredient.Flora, Ingredient.Mineral },
            24, 4,
            new Attack("Hex", 1, 6, new RangeSelector(3, false, true), null, "Deals 6 damage and slows the target for 1 turn.", (user, target, healthLost) => { target.ApplyStatus(hexedStatus); }),
            new Move("Illuminate", 3, MoveType.Heal, Move.Targets.Allies, ZoneSelector.AOESelector, (user, tile) => { if(tile != user.Tile) LevelGrid.Instance.GetMonster(tile).Heal(4); }, null, "Heals other allies for 4 health each.")
        );

        monsterTypes[(int)MonsterName.Sludge] = new MonsterType(new List<Ingredient>() { Ingredient.Decay, Ingredient.Decay, Ingredient.Flora },
            26, 4,
            new Attack("Blob Lob", 1, 8, new RangeSelector(4, false, true), AnimateLobber(prefabs.sludgeLob, 2.0f, 0.6f), "Deals 8 damage."),
            new ZoneMove("Toxic Puddle", 3, ZoneSelector.AdjacentSelector, TileAffector.CreateBlueprint(prefabs.sludgeZone, 3, 0, null, false, false, (Monster occupant) => { occupant.TakeDamage(5); }), null, "Places a zone that deals 5 damage to enemies that stand on it.")
        );

        monsterTypes[(int)MonsterName.Fossil] = new MonsterType(new List<Ingredient>() { Ingredient.Mineral, Ingredient.Mineral, Ingredient.Decay },
            24, 3,
            new Attack("Bone Shot", 1, 10, new DirectionSelector(5, false), AnimateLinearShot(prefabs.boneShot, null, 16f), ""),
            new ZoneMove("Quicksand", 3, new ZoneSelector(2, 2), TileAffector.CreateBlueprint(prefabs.quicksand, 3, 1, null), null, "Places an area that slows enemies passing through.")
        );

        monsterTypes[(int)MonsterName.Phantom] = new MonsterType(new List<Ingredient>() { Ingredient.Decay, Ingredient.Decay, Ingredient.Mineral },
            22, 5,
            new Attack("Slash", 1, 12, RangeSelector.MeleeSelector, null, "Deals 12 damage."),
            new Move("Pierce", 3, MoveType.Shift, Move.Targets.StandableSpot, new DirectionSelector(3, false, false), DashSlash, null, "Shifts forward and deals 9 damage to enemies passed through.")
        );

        monsterTypes[(int)MonsterName.Beast] = new MonsterType(new List<Ingredient>() { Ingredient.Mineral, Ingredient.Mineral, Ingredient.Flora },
            26, 4,
            new Attack("Claw", 1, 9, new ZoneSelector(1, 2), null, "Deals 9 damage."),
            new StatusMove("Battle Cry", 5, new StatusAilment(StatusEffect.Swift, 2, prefabs.beastSpeed), ZoneSelector.AOESelector, null, "Increases speed for 2 turns.")
        );

        monsterTypes[(int)MonsterName.Amalgamation] = new MonsterType(new List<Ingredient>() { Ingredient.Decay, Ingredient.Flora, Ingredient.Mineral },
            30, 3,
            new Attack("Lash Out", 1, 8, new ZoneSelector(1, 3), null, "Deals 8 damage."),
            new StatusMove("Horrify", 3, new StatusAilment(StatusEffect.Fear, 1, prefabs.fearStatus), ZoneSelector.AOESelector, null, "Halves damage dealt for 1 turn")
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
                hit.TakeDamage(9, user);
            }
        }
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

        // apply slowness effect as well
        target.ApplyStatus(tangledStatus);
    }

    private static void StealHealth(Monster user, Monster target, int healthLost) {
        user.Heal(healthLost);
    }

    /* deprecated :(
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
    }*/
    #endregion
}
