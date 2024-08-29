using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIController
{
    private Team controlTarget;

    private ResourcePile targetResource;
    private static Dictionary<ResourcePile, ResourceData> resourceData;
    private static Dictionary<Monster, List<Vector2Int>> walkOptions = new Dictionary<Monster, List<Vector2Int>>(); // cache to prevent repeated pathfinding
    private static Dictionary<Monster, Vector2Int> pathedPositions = new Dictionary<Monster, Vector2Int>();
    private static Vector2 idealZoneMiddle;

    public AIController(Team team) {
        this.controlTarget = team;
    }

    // enacts every move in the turn immediately. The animation queuer makes the visuals play out in the correct order
    public void TakeTurn() {
        // focus attention on the resource that the attacking team is approaching
        resourceData = EvaluateResources();
        targetResource = GameManager.Instance.AllResources.Max((ResourcePile resource) => resourceData[resource].threatValue);

        // cache where the monsters can move to avoid repeated pathfinding
        pathedPositions.Clear();
        walkOptions.Clear();
        foreach(Monster monster in controlTarget.Teammates) {
            pathedPositions[monster] = monster.Tile;
            if(monster.WalkAvailable) {
                walkOptions[monster] = monster.GetMoveOptions(MonsterType.WALK_INDEX, false)
                    .ConvertAll((Selection tileContainter) => tileContainter.Unfiltered[0]);
            }
        }

        // find center of conflict for zone placement
        Vector2 allyCenter = controlTarget.Teammates
            .ConvertAll(monster => (Vector2)monster.transform.position)
            .Collapse((Vector2 cur, Vector2 next) => cur + next)
            / controlTarget.Teammates.Count;

        Vector2 enemyCenter = GameManager.Instance.OpponentOf(controlTarget).Teammates
            .ConvertAll(monster => (Vector2)monster.transform.position)
            .Collapse((Vector2 cur, Vector2 next) => cur + next)
            / GameManager.Instance.OpponentOf(controlTarget).Teammates.Count;

        idealZoneMiddle = .25f * allyCenter + .75f * enemyCenter;

        // order abilities
        Dictionary<Monster, int> movePriorities = new Dictionary<Monster, int>();
        foreach(Monster teammate in controlTarget.Teammates) {
            // -2: targets an ally
            // -1 targets an ally and targeted by an ally
            // 0: no priority
            // 1: targeted by an ally
            movePriorities[teammate] = 0;
        }

        List<TurnOption> monsterPlans = controlTarget.Teammates.ConvertAll((Monster monster) => ChooseAction(monster));
        foreach(TurnOption plan in monsterPlans) {
            if(!plan.UsesAbility) {
                continue;
            }

            Move plannedMove = plan.user.Stats.Moves[plan.abilitySlot];
            if(plannedMove is StatusMove && (plannedMove as StatusMove).Condition.effect == StatusEffect.Haunt) {
                movePriorities[plan.user] = -2; // always haunt the enemy first
            }
            else if(plannedMove.TargetType == Move.Targets.Allies) {
                movePriorities[plan.user] = movePriorities[plan.user] == 1 || movePriorities[plan.user] == -1 ? -1 : -2;
                foreach(Vector2Int tile in plan.abilityTargets.Filtered) {
                    if(tile == plan.walkDestination) {
                        continue; // targeting yourself has no dependencies
                    }
                    Monster ally = LevelGrid.Instance.GetMonster(tile);
                    movePriorities[ally] = movePriorities[ally] == -2 || movePriorities[plan.user] == -1 ? -1 : 1;
                }
            }
        }

        // choose abilities
        List<Monster> orderedTeammates = new List<Monster>(controlTarget.Teammates);
        orderedTeammates.Sort((Monster cur, Monster next) => movePriorities[cur] - movePriorities[next]);
        foreach(Monster teammate in orderedTeammates) {
            TurnOption choice = ChooseAction(teammate);
            foreach(TurnOption.MoveOrdering action in choice.GetSequence()) {
                if(action == TurnOption.MoveOrdering.WalkOnly) {
                    choice.user.UseMove(MonsterType.WALK_INDEX, new Selection(choice.walkDestination));
                }
                else if(action == TurnOption.MoveOrdering.AbilityOnly) {
                    choice.user.UseMove(choice.abilitySlot, choice.abilityTargets);
                }
            }
        }

        AttemptCraft();

        // end turn after animations play out
        AnimationsManager.Instance.QueueFunction(() => { controlTarget.EndTurn(); });
    }

    // determines the best sequence of walk and ability to use this turn
    private TurnOption ChooseAction(Monster monster) {
        List<TurnOption> options = new List<TurnOption>();

        // consider if doing nothing is optimal
        options.Add(new TurnOption { 
            user = monster,
            ordering = TurnOption.MoveOrdering.None,
            abilityValue = 0f,
            positionValue = 0f
        });

        // find the best walk destination
        List<Vector2Int> walkableTiles = walkOptions[monster].FindAll((Vector2Int tile) => LevelGrid.Instance.IsOpenTile(tile));
        Vector2Int idealEndTile = walkableTiles.Count > 0 ? walkableTiles.Max((Vector2Int tile) => DeterminePositionValue(tile, targetResource.Tile)) : monster.Tile;
        float startPosValue = DeterminePositionValue(monster.Tile, targetResource.Tile);
        float idealEndValue = DeterminePositionValue(idealEndTile, targetResource.Tile);

        // consider simply walking
        if(walkableTiles.Count > 0) {
            options.Add(new TurnOption { 
                user = monster,
                ordering = TurnOption.MoveOrdering.WalkOnly,
                walkDestination = idealEndTile,
                abilityValue = 0f,
                positionValue = idealEndValue
            });
        }

        // for each ability, find the best option
        for(int i = 0; i < monster.Stats.Moves.Length; i++) {
            if(i == MonsterType.WALK_INDEX || monster.Cooldowns[i] > 0) {
                continue;
            }

            // consider moving then using the ability
            if(walkableTiles.Count > 0) {
                TurnOption walkFirst = ChooseWalkedOption(monster, i, startPosValue, walkableTiles);
                if(walkFirst.ordering != TurnOption.MoveOrdering.None) {
                    options.Add(walkFirst);
                }
            }

            // consider using the ability then moving
            TurnOption abilityFirst = ChooseStillOption(monster, i);
            if(abilityFirst.ordering == TurnOption.MoveOrdering.None) {
                // there might be no targets
                continue;
            }

            if(walkableTiles.Count == 0 || monster.Stats.Moves[i].Name == "Pierce") {
                abilityFirst.ordering = TurnOption.MoveOrdering.AbilityOnly;
                abilityFirst.positionValue = startPosValue;
                options.Add(abilityFirst);
                continue;
            }

            abilityFirst.ordering = idealEndTile == monster.Tile ? TurnOption.MoveOrdering.AbilityOnly : TurnOption.MoveOrdering.AbilityThenWalk;
            abilityFirst.walkDestination = idealEndTile;
            abilityFirst.positionValue = idealEndValue;
            options.Add(abilityFirst);
        }
       
        return options.Max((TurnOption option) => option.Effectiveness);
    }

    // best way of using an ability without moving
    private TurnOption ChooseStillOption(Monster monster, int moveSlot) {
        List<Selection> targetGroups = monster.GetMoveOptions(moveSlot);
        if(targetGroups.Count == 0) {
            return new TurnOption {
                user = monster,
                ordering = TurnOption.MoveOrdering.None
            };
        }

        List<Selection> bestTargets = targetGroups.AllTiedMax((Selection targetGroup) => DetermineOptionValue(monster, moveSlot, targetGroup.Filtered, monster.Tile));
        Selection chosenTarget = bestTargets[UnityEngine.Random.Range(0, bestTargets.Count)];
        return new TurnOption {
            user = monster,
            ordering = TurnOption.MoveOrdering.AbilityOnly,
            abilitySlot = moveSlot,
            abilityTargets = chosenTarget
        };
    }

    // best way of moving to another tile then using an ability
    private TurnOption ChooseWalkedOption(Monster monster, int moveSlot, float startTileValue, List<Vector2Int> walkableTiles) {
        Dictionary<Vector2Int, List<Selection>> positionTargetOptions = monster.GetMoveOptionsAfterWalk(moveSlot, false, walkableTiles);

        List<TurnOption> allOptions = new List<TurnOption>();
        foreach(KeyValuePair<Vector2Int, List<Selection>> positionTargets in positionTargetOptions) {
            Vector2Int walkDestination = positionTargets.Key;
            List<Selection> targetGroups = positionTargets.Value;

            float posVal = DeterminePositionValue(walkDestination, targetResource.Tile);
            foreach(Selection targetGroup in targetGroups) {
                if(targetGroup.Filtered.Count == 0) {
                    continue;
                }

                allOptions.Add(new TurnOption {
                    user = monster,
                    ordering = TurnOption.MoveOrdering.WalkThenAbility,
                    walkDestination = walkDestination,
                    abilitySlot = moveSlot,
                    abilityTargets = targetGroup,
                    abilityValue = DetermineOptionValue(monster, moveSlot, targetGroup.Filtered, walkDestination),
                    positionValue = posVal
                });
            }
        }

        if(allOptions.Count == 0) {
            return new TurnOption {
                user = monster,
                ordering = TurnOption.MoveOrdering.None
            };
        }

        return allOptions.Max((TurnOption option) => option.Effectiveness);
    }

    // returns a value that represents how valuable the usage of the input move on the input targets would be
    // userPosition is the tile that the user has theoretically moved to, which may be different from its current tile
    private float DetermineOptionValue(Monster user, int moveSlot, List<Vector2Int> targets, Vector2Int userPosition) {
        // TODO: add move heuristics
        LevelGrid level = LevelGrid.Instance;
        Move move = user.Stats.Moves[moveSlot];

        if(move.Type == MoveType.Heal) {
            // prioritize healing lower health allies
            float value = -0.1f;
            foreach(Vector2Int tile in targets) {
                Monster ally = tile == userPosition ? user : level.GetMonster(tile);
                value += 0.7f * (1f - (float)ally.Health / ally.Stats.Health);
            }
            return value;
        }

        if(move is Attack) {
            int totalDamage = 0;
            foreach(Monster hit in targets.ConvertAll((Vector2Int tile) => level.GetMonster(tile))) {
                totalDamage += hit.DetermineDamage(((Attack)move).Damage, user);
            }
            return totalDamage / 10.0f;
        }

        if(move is StatusMove) {
            StatusAilment effect = ((StatusMove)move).Condition;
            Monster hit = targets[0] == userPosition ? user : level.GetMonster(targets[0]);
            if(move.TargetType == Move.Targets.Enemies) {
                if(effect.effect == StatusEffect.Fear && !(hit.Stats.Moves[MonsterType.PRIMARY_INDEX] is Attack)) {
                    return -1f; // dont weaken an enemy that has no attacks
                }

                return 0.8f * (hit.Health / (float)hit.Stats.Health);
            } else {
                if(effect.effect == StatusEffect.Power && !(hit.Stats.Moves[MonsterType.PRIMARY_INDEX] is Attack)) {
                    return -1f; // dont strengthen an ally that has no attacks
                }

                float value = -0.3f;
                foreach(Monster enemy in GameManager.Instance.OpponentOf(controlTarget).Teammates) {
                    value += 0.4f * Mathf.Max(0f, Global.CalcTileDistance(hit.Tile, enemy.Tile) / 6f);
                }
                return value;
            }
        }

        if(move is ZoneMove) {
            if(GameManager.Instance.OpponentOf(controlTarget).Teammates.Count == 0) {
                return -1f;
            }

            float value = -0.4f;
            foreach(Vector2Int tile in targets) {
                if(level.GetTile(tile).CurrentEffect != null) {
                    // discourage replacing an existing tile effect
                    value -= 0.2f;
                    continue;
                }

                float distance = Vector2.Distance(level.Tiles.GetCellCenterWorld((Vector3Int)tile), idealZoneMiddle);
                value += 0.6f * (1f - distance / 4f);
            }
            return value;
        }

        if(move.Type == MoveType.Shift) {
            switch(move.Name) {
                case "Pierce":
                    // dash
                    Vector2Int dir = targets[0] - userPosition;
                    if(dir.x == 0) {
                        dir = new Vector2Int(0, dir.y > 0 ? 1 : -1);
                    } else {
                        dir = new Vector2Int(dir.x > 0 ? 1 : -1, 0);
                    }

                    float enemiesHit = 0;
                    for(Vector2Int tile = userPosition + dir; tile != targets[0]; tile += dir) {
                        Monster occupant = level.GetMonster(tile);
                        if(occupant != null && occupant.Controller != user.Controller) {
                            enemiesHit++;
                        }
                    }
                    return 0.4f * DeterminePositionValue(targets[0], targetResource.Tile) + 0.5f * enemiesHit;

                case "Vine Grasp":
                    // pull
                    return 0.3f + 0.1f * (Global.CalcTileDistance(userPosition, targets[0]) - 1);
            }
        }

        return 0.3f;
    }

    // returns a value 0-1 that represents how far this starting position is from the end goal
    private float DeterminePositionValue(Vector2Int startPosition, Vector2Int goal) {
        float tileDistance = Monster.FindPath(startPosition, goal).Count - 1f; // distance to an orthog/diag adjacent tile
        if(startPosition.x != goal.x && startPosition.y != goal.y) {
            tileDistance--; // make diagonal corners worth the same as orthogonally adjacent
        }

        const float MAX_DISTANCE = 20f;
        return Mathf.Max(0f, (MAX_DISTANCE - tileDistance) / MAX_DISTANCE);
    }

    private float CalculateInfluence(Monster monster, ResourcePile resource, List<Vector2Int> existingPath = null) {
        const int MAX_INFLUENCE_RANGE = 5;
        
        if(Global.CalcTileDistance(monster.Tile, resource.Tile) > MAX_INFLUENCE_RANGE + 2) { // influence range may be 2 greater than the actual distance
            return 0;
        }

        // find the distance to the capture zone
        int distance = (existingPath == null ? Monster.FindPath(monster.Tile, resource.Tile) : existingPath).Count - 1;
        if(monster.Tile.x != resource.Tile.x && monster.Tile.y != resource.Tile.y) {
            // make corners worth the same as orthogonally adjacent
            distance--;
        }

        if(distance > MAX_INFLUENCE_RANGE) {
            return 0f;
        }

        return (MAX_INFLUENCE_RANGE + 1 - distance) / (MAX_INFLUENCE_RANGE + 1f);
    }

    private Dictionary<ResourcePile, ResourceData> EvaluateResources() {
        Dictionary<ResourcePile, ResourceData> resourceData = new Dictionary<ResourcePile, ResourceData>();

        foreach(ResourcePile resource in GameManager.Instance.AllResources) {
            ResourceData data = new ResourceData();
            // determine how much influence each team has on this resource
            data.allyPaths = new Dictionary<Monster, List<Vector2Int>>();
            foreach(Monster ally in controlTarget.Teammates) {
                data.allyPaths[ally] = Monster.FindPath(ally.Tile, resource.Tile);
                data.controlValue += CalculateInfluence(ally, resource, data.allyPaths[ally]);
            }

            foreach(Monster enemy in GameManager.Instance.OpponentOf(controlTarget).Teammates) {
                data.threatValue += CalculateInfluence(enemy, resource);
            }

            resourceData[resource] = data;
        }

        return resourceData;
    }

    private void AttemptCraft() {
        if(controlTarget.Spawnpoint.CookState != Cauldron.State.Ready) {
            return;
        }

        List<MonsterName> buyOptions = new List<MonsterName>();
        foreach(MonsterName monsterType in System.Enum.GetValues(typeof(MonsterName))) {
            if(controlTarget.CanAfford(monsterType)) {
                buyOptions.Add(monsterType);
            }
        }
        if(buyOptions.Count > 0) {
            controlTarget.BuyMonster(buyOptions[UnityEngine.Random.Range(0, buyOptions.Count)]);
        }
    }

    private struct ResourceData {
        public Dictionary<Monster, List<Vector2Int>> allyPaths;
        public float threatValue; // attacker's
        public float controlValue; // defender's influence
    }

    private struct TurnOption {
        public enum MoveOrdering {
            None,
            WalkOnly,
            AbilityOnly,
            WalkThenAbility,
            AbilityThenWalk
        }

        public Monster user;
        public MoveOrdering ordering;
        public Vector2Int walkDestination;
        public int abilitySlot;
        public Selection abilityTargets;
        public float abilityValue; // how valuable the used ability with the given targets is
        public float positionValue; // how close the end position is to the target position

        public float Effectiveness { get { return abilityValue + positionValue; } }

        private static List<MoveOrdering> abilityOrders = new List<MoveOrdering> { MoveOrdering.AbilityOnly, MoveOrdering.WalkThenAbility, MoveOrdering.AbilityThenWalk };
        public bool UsesAbility { get { return abilityOrders.Contains(ordering); } }

        public MoveOrdering[] GetSequence() {
            switch(ordering) {
                default:
                case MoveOrdering.None:
                    return new MoveOrdering[0];
                case MoveOrdering.WalkOnly:
                    return new MoveOrdering[] { MoveOrdering.WalkOnly };
                case MoveOrdering.AbilityOnly:
                    return new MoveOrdering[] { MoveOrdering.AbilityOnly }; ;
                case MoveOrdering.WalkThenAbility:
                    return new MoveOrdering[] { MoveOrdering.WalkOnly, MoveOrdering.AbilityOnly }; ;
                case MoveOrdering.AbilityThenWalk:
                    return new MoveOrdering[] { MoveOrdering.AbilityOnly, MoveOrdering.WalkOnly }; ;
            }
        }
    }
}
