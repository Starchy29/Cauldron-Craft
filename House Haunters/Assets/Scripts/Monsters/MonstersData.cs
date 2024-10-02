using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

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

        monsterTypes[(int)MonsterName.LostSoul] = new MonsterType(MonsterName.LostSoul, new List<Ingredient>() { Ingredient.Decay, Ingredient.Decay, Ingredient.Decay },
            25, 4,
            new Move("Revitalize", 1, MoveType.Heal, Move.Targets.Allies, new RangeSelector(2, false, true), (user, tile) => { LevelGrid.Instance.GetMonster(tile).Heal(6); }, AnimateGlow(2f, new Color(0.2f, 0.7f, 0.9f)), "Heals an ally for 6 health."),
            new StatusMove("Haunt", 2, new StatusAilment(StatusEffect.Haunt, 2, prefabs.spookHaunt), RangeSelector.MeleeSelector, 
                (user, targets) => { AnimationsManager.Instance.QueueAnimation(new ThrustAnimator(user.gameObject, Global.DetermineCenter(targets.Filtered), true)); }, "The target takes 1.5x damage for 2 turns.")
        );

        monsterTypes[(int)MonsterName.Demon] = new MonsterType(MonsterName.Demon, new List<Ingredient>() { Ingredient.Decay, Ingredient.Decay, Ingredient.Decay },
            25, 4,
            new Attack("Fireball", 1, 8, new RangeSelector(3, false, true), AnimateProjectile(prefabs.fireball, prefabs.fireballBlast, 10f), "Deals 8 damage to the target and 4 damage to enemies adjacent to the target.", (user, target, healthLost) => { DealSplashDamage(user, target.Tile, 4); }),
            new StatusMove("Ritual", 2, new StatusAilment(StatusEffect.Power, 2, prefabs.demonStrength), SelfSelector.Instance, AnimateGlow(1.5f, Color.red), "Lose 3 life to gain power next turn", (user, tile) => user.TakeDamage(3))
        );

        monsterTypes[(int)MonsterName.Cactus] = new MonsterType(MonsterName.Cactus, new List<Ingredient>() { Ingredient.Flora, Ingredient.Flora, Ingredient.Flora },
            28, 3,
            new Attack("Barb Bullet", 1, 7, new DirectionSelector(6, true), AnimateBarbBullet, "Deals 7 damage and pierces through enemies."),
            new ZoneMove("Thorn Trap", 2, new RangeSelector(3, false, true), TileAffector.CreateBlueprint(prefabs.spikeTrapPrefab, 5, 0, (lander) => { lander.TakeDamage(6); }, true, true), 
                AnimateLobber(prefabs.thornShot, 2f, 0.8f), "Places a trap that blocks enemies and deals 6 damage when stepped on.")
        );

        tangledStatus = new StatusAilment(StatusEffect.Slowness, 2, prefabs.tangleVines);
        monsterTypes[(int)MonsterName.Flytrap] = new MonsterType(MonsterName.Flytrap, new List<Ingredient>() { Ingredient.Flora, Ingredient.Flora, Ingredient.Flora },
            28, 3,
            new Attack("Chomp", 1, 12, RangeSelector.MeleeSelector, AnimateMeleeWithParticle(prefabs.chompTeeth), "Deals 12 damage."),
            new Move("Vine Grasp", 2, MoveType.Shift, Move.Targets.Enemies, new DirectionSelector(4, false), PullTarget, null, "Pulls the target to the user and slows it for 2 turns.")
        );

        monsterTypes[(int)MonsterName.Golem] = new MonsterType(MonsterName.Golem, new List<Ingredient>() { Ingredient.Mineral, Ingredient.Mineral, Ingredient.Mineral },
            22, 5,
            new StatusMove("Aura Boost", 1, new StatusAilment(StatusEffect.Power, 1, prefabs.auraStatus), new RangeSelector(2, false, true), AnimateGlow(1.5f, Color.cyan), "Increases an ally's damage by 1.5x for 1 turn."),
            new StatusMove("Crystallize", 2, new StatusAilment(StatusEffect.Sturdy, 2, prefabs.crystalShield), RangeSelector.MeleeSelector, meleeAnimation, "Halves the damage an ally receives for 2 turns.")
        );

        monsterTypes[(int)MonsterName.Automaton] = new MonsterType(MonsterName.Automaton, new List<Ingredient>() { Ingredient.Mineral, Ingredient.Mineral, Ingredient.Mineral },
            28, 2,
            new Attack("Flame Cannon", 1, 9, new ZoneSelector(5, 2, true), AnimateLobber(prefabs.fireball, 3f, 1f, prefabs.fireballBlast), "Deals 9 damage in a 2x2 square. Cannot be used if the user has moved this turn."),
            new StatusMove("Fortify", 2, new StatusAilment(StatusEffect.Sturdy, 1, prefabs.bastionShield), SelfSelector.Instance, null, "Receive half damage for a turn.")
        );
        monsterTypes[(int)MonsterName.Automaton].Moves[MonsterType.PRIMARY_INDEX].CantWalkFirst = true;

        monsterTypes[(int)MonsterName.Fungus] = new MonsterType(MonsterName.Fungus, new List<Ingredient>() { Ingredient.Flora, Ingredient.Flora, Ingredient.Decay },
            28, 3,
            new StatusMove("Infect", 1, new StatusAilment(StatusEffect.Poison, 3, prefabs.poisonSpores), new RangeSelector(2, false, true), AnimateLobber(prefabs.sporeShot, 0.8f, 0.7f), "Deals 5 damage for 3 turns."),
            new StatusMove("Psychic Spores", 1, new StatusAilment(StatusEffect.Fear, 2, prefabs.fearStatus), RangeSelector.MeleeSelector, meleeAnimation, "Halves the target's damage for 2 turns.")
        );

        monsterTypes[(int)MonsterName.Jackolantern] = new MonsterType(MonsterName.Jackolantern, new List<Ingredient>() { Ingredient.Flora, Ingredient.Flora, Ingredient.Mineral },
            22, 4,
            new StatusMove("Will o' Wisp", 1, new StatusAilment(StatusEffect.Haunt, 1, prefabs.willOWisps), new RangeSelector(3, false, true), AnimateProjectile(prefabs.willOWisps, null, 5f, false, false), "The target receives 1.5x damage this turn."),
            new StatusMove("Hex", 2, new StatusAilment(StatusEffect.Cursed, 3, prefabs.hexStatus), new RangeSelector(2, false, true), AnimateGlow(1.5f, new Color(0.5f, 0.1f, 0.8f)), "Makes an ally deal 4 revenge damage for 3 turns.")
        );

        monsterTypes[(int)MonsterName.Sludge] = new MonsterType(MonsterName.Sludge, new List<Ingredient>() { Ingredient.Decay, Ingredient.Decay, Ingredient.Flora },
            25, 4,
            new Attack("Blob Lob", 1, 8, new RangeSelector(4, false, true), AnimateLobber(prefabs.sludgeLob, 2.0f, 0.6f), "Deals 8 damage."),
            new ZoneMove("Toxic Puddle", 3, ZoneSelector.AdjacentSelector, TileAffector.CreateBlueprint(prefabs.sludgeZone, 3, 0, null, false, false, DealPuddleDamage),
                AnimateStretching(new List<StretchType> { StretchType.Horizontal }, 1.5f, 1f), "Places a zone that deals 5 damage to enemies that stand on it.")
        );

        monsterTypes[(int)MonsterName.Fossil] = new MonsterType(MonsterName.Fossil, new List<Ingredient>() { Ingredient.Mineral, Ingredient.Mineral, Ingredient.Decay },
            22, 3,
            new Attack("Bone Shot", 1, 10, new DirectionSelector(5, false), AnimateLinearShot(prefabs.boneShot, 16f), "Deals 10 damage."),
            new ZoneMove("Quicksand", 3, new ZoneSelector(2, 2), TileAffector.CreateBlueprint(prefabs.quicksand, 3, 1, null), meleeAnimation, "Places an area that slows enemies passing through.")
        );

        monsterTypes[(int)MonsterName.Phantom] = new MonsterType(MonsterName.Phantom, new List<Ingredient>() { Ingredient.Decay, Ingredient.Decay, Ingredient.Mineral },
            20, 5,
            new Attack("Slash", 1, 11, RangeSelector.MeleeSelector, AnimateMeleeWithParticle(prefabs.phantomSlash), "Deals 11 damage."),
            new Move("Pierce", 3, MoveType.Shift, Move.Targets.StandableSpot, new DirectionSelector(3, false, false), DashSlash, null, "Moves to the target tile and deals 9 damage to enemies passed through.")
        );

        monsterTypes[(int)MonsterName.Beast] = new MonsterType(MonsterName.Beast, new List<Ingredient>() { Ingredient.Mineral, Ingredient.Mineral, Ingredient.Flora },
            25, 4,
            new Attack("Claw", 1, 9, new ZoneSelector(1, 2), AnimateClaw, "Deals 9 damage in an arc."),
            new StatusMove("Battle Cry", 5, new StatusAilment(StatusEffect.Swift, 2, prefabs.beastSpeed), ZoneSelector.AOESelector, AnimateBattleCry, "Increases speed of all nearby allies for 2 turns.")
        );

        monsterTypes[(int)MonsterName.Amalgamation] = new MonsterType(MonsterName.Amalgamation, new List<Ingredient>() { Ingredient.Decay, Ingredient.Flora, Ingredient.Mineral },
            30, 3,
            new Attack("Lash Out", 1, 7, new ZoneSelector(1, 3), AnimateLashOut, "Deals 7 damage to all adjacent enemies."),
            new Move("Mend Flesh", 2, MoveType.Heal, Move.Targets.Allies, SelfSelector.Instance, (user, tile) => user.Heal(4), 
                AnimateStretching(new List<StretchType> { StretchType.Horizontal, StretchType.Vertical, StretchType.Horizontal, StretchType.Vertical  }, 0.5f, 0.3f), "Heal 4 health")
        );
    }

    public MonsterType GetMonsterData(MonsterName name) {
        return monsterTypes[(int)name];
    }

    private static Move.AnimationFunction meleeAnimation = (Monster user, Selection targets) => {
        AnimationsManager.Instance.QueueAnimation(new ThrustAnimator(user.gameObject, Global.DetermineCenter(targets.Filtered)));
    };

    #region Animation Helpers
    // these create objects that queue animations
    private static Move.AnimationFunction AnimateMeleeWithParticle(GameObject particlePrefab) {
        return (Monster user, Selection targets) => {
            GameObject particle = GameObject.Instantiate(particlePrefab);
            particle.transform.position = LevelGrid.Instance.GetMonster(targets.Filtered[0]).SpriteModel.transform.position;
            particle.SetActive(false);
            AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(particle, true));
            AnimationsManager.Instance.QueueAnimation(new ThrustAnimator(user.gameObject, Global.DetermineCenter(targets.Filtered)));

        };
    }

    private static Move.AnimationFunction AnimateParticle(GameObject particlePrefab) {
        return (Monster user, Selection targets) => {
            GameObject particle = GameObject.Instantiate(particlePrefab);
            particle.transform.position = Global.DetermineCenter(targets.Unfiltered);
            particle.SetActive(false);
            AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(particle, true));
        };
    }

    private static Move.AnimationFunction AnimateProjectile(GameObject projectilePrefab, GameObject destroyParticlePrefab, float speed, bool reversed = false, bool rotate = true) {
        return (Monster user, Selection targets) => {
            LevelGrid level = LevelGrid.Instance;
            Vector3 start = level.Tiles.GetCellCenterWorld((Vector3Int)user.Tile);
            Vector3 end = level.Tiles.GetCellCenterWorld((Vector3Int)targets.Filtered[0]);
            AnimationsManager.Instance.QueueAnimation(new ProjectileAnimator(projectilePrefab, reversed ? end : start, reversed ? start : end, speed, rotate));
            if(destroyParticlePrefab != null) {
                GameObject particle = GameObject.Instantiate(destroyParticlePrefab);
                particle.transform.position = end;
                particle.SetActive(false);
                AnimationsManager.Instance.QueueFunction(() => { particle.SetActive(true); });
            }
        };
    }

    private static Move.AnimationFunction AnimateLobber(GameObject prefab, float height, float duration, GameObject destroyParticle = null) {
        return (Monster user, Selection targets) => {
            LevelGrid level = LevelGrid.Instance;
            Vector3 start = level.Tiles.GetCellCenterWorld((Vector3Int)user.Tile);
            Vector3 end = Global.DetermineCenter(targets.Unfiltered);
            AnimationsManager.Instance.QueueAnimation(new LobAnimator(prefab, start, end, height, duration));
            if(destroyParticle != null) {
                GameObject particle = GameObject.Instantiate(destroyParticle);
                particle.transform.position = end;
                particle.SetActive(false);
                AnimationsManager.Instance.QueueFunction(() => { particle.SetActive(true); });
            }
        };
    }

    private static Move.AnimationFunction AnimateLinearShot(GameObject projectilePrefab, float speed) {
        return (Monster user, Selection targets) => {
            LevelGrid level = LevelGrid.Instance;
            Vector3 start = level.Tiles.GetCellCenterWorld((Vector3Int)user.Tile);
            Vector3 direction = ((Vector2)(targets.Unfiltered[0] - user.Tile)).normalized;
            Vector2Int furthestTile = targets.Unfiltered.Max((Vector2Int tile) => { return Global.CalcTileDistance(tile, user.Tile); });
            Vector3 end = level.Tiles.GetCellCenterWorld((Vector3Int)furthestTile);
            AnimationsManager.Instance.QueueAnimation(new ProjectileAnimator(projectilePrefab, start, end, speed));
        };
    }

    private static Move.AnimationFunction AnimateStretching(List<StretchType> sequence, float maxStretch, float elementDuration) {
        return (Monster user, Selection targets) => {
            AnimationsManager.Instance.QueueAnimation(new StretchAnimator(user.gameObject, sequence, maxStretch, elementDuration));
        };
    }

    private static Move.AnimationFunction AnimateGlow(float duration, Color color) {
        return (Monster user, Selection targets) => {
            AnimationsManager.Instance.QueueAnimation(new RadialAnimator(PrefabContainer.Instance.glowParticle, user, duration, true, color));
        };
    }
    #endregion

    #region Specific Move Animations
    private static void AnimateBattleCry(Monster user, Selection targets) {
        GameObject prefab = PrefabContainer.Instance.beastShout;
        AnimationsManager.Instance.QueueAnimation(new RadialAnimator(prefab, user, 0.25f, false));
        AnimationsManager.Instance.QueueAnimation(new RadialAnimator(prefab, user, 0.25f, false));
        AnimationsManager.Instance.QueueAnimation(new RadialAnimator(prefab, user, 0.25f, false));
    }

    private static void AnimateClaw(Monster user, Selection targets) {
        GameObject particle = GameObject.Instantiate(PrefabContainer.Instance.beastSlash);
        Vector3 target = Global.DetermineCenter(targets.Unfiltered);
        particle.transform.position = target;
        particle.SetActive(false);
        Vector3 direction = target - user.transform.position;
        if(direction.y < 0 && direction.x < 0) {
            particle.transform.rotation = Quaternion.Euler(0, 0, 90);
            particle.GetComponent<SpriteRenderer>().flipX = true;
        }
        else if(direction.y < 0) {
            particle.transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        else if(direction.x < 0) {
            particle.GetComponent<SpriteRenderer>().flipX = true;
        }
        AnimationsManager.Instance.QueueFunction(() => { particle.SetActive(true); });
        AnimationsManager.Instance.QueueAnimation(new ThrustAnimator(user.gameObject, target));
    }

    private static void AnimateLashOut(Monster user, Selection targets) {
        foreach(Vector2Int tile in targets.Filtered) {
            AnimationsManager.Instance.QueueAnimation(new ThrustAnimator(user.gameObject, LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)tile)));
        }
    }

    private static void AnimateBarbBullet(Monster user, Selection targets) {
        AnimateLinearShot(PrefabContainer.Instance.thornShot, 30f)(user, targets);
        foreach(Vector2Int tile in targets.Filtered) {
            AnimationsManager.Instance.QueueFunction(() => { SpawnPierceParticle(tile); });
        }
    }

    private static void SpawnPierceParticle(Vector2Int tile) {
        GameObject particle = GameObject.Instantiate(PrefabContainer.Instance.pierceParticle);
        particle.transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)tile) + new Vector3(0, 0.2f, 0);
    }
    #endregion

    #region bonus effects and special moves
    private static void DealSplashDamage(Monster attacker, Vector2Int center, int damage) {
        List<Monster> targets = LevelGrid.Instance.GetTilesInRange(center, 1, true)
            .FindAll((Vector2Int tile) => { return Move.IsEnemyOn(attacker, tile); })
            .ConvertAll((Vector2Int tile) => { return LevelGrid.Instance.GetMonster(tile); });

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
        AnimationsManager.Instance.QueueAnimation(new PathAnimator(user, new List<Vector3> { level.Tiles.GetCellCenterWorld((Vector3Int)target) }, 25f));
        level.MoveEntity(user, target);

        // damage all enemies passed through
        Vector2Int direction = target - start;
        if(direction.x == 0) {
            direction = new Vector2Int(0, direction.y > 0 ? 1 : -1);
        } else {
            direction = new Vector2Int(direction.x > 0 ? 1 : -1, 0);
        }

        List<Monster> hits = new List<Monster>();
        for(Vector2Int tile = start + direction; tile != target; tile += direction) {
            Monster hit = level.GetMonster(tile);
            if(hit != null && hit.Controller != user.Controller) {
                hits.Add(hit);
            }
        }

        foreach(Monster hit in hits) {
            SpawnPierceParticle(hit.Tile);
        }
        AnimationsManager.Instance.QueueAnimation(new PauseAnimator(0.3f));
        foreach(Monster hit in hits) {
            hit.TakeDamage(9, user);
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

        AnimationsManager.Instance.QueueAnimation(new VineAnimator(user, target, furthestPull));
        level.MoveEntity(target, furthestPull);

        // apply slowness effect as well
        target.ApplyStatus(tangledStatus);
    }

    private static void DealPuddleDamage(Monster occupant) {
        AnimationsManager.Instance.QueueFunction(() => {
            GameObject particle = GameObject.Instantiate(PrefabContainer.Instance.poisonParticle);
            particle.transform.position = occupant.SpriteModel.transform.position;
            particle = GameObject.Instantiate(PrefabContainer.Instance.poisonParticle);
            particle.transform.position = occupant.SpriteModel.transform.position;
        });
        occupant.TakeDamage(5);
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
