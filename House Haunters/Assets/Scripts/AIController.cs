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
        AnimationsManager.Instance.OnAnimationsEnd += EndTurn;
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
            if(monster.CanUse(MonsterType.WALK_INDEX)) {
                walkOptions[monster] = monster.GetMoveOptions(MonsterType.WALK_INDEX, false).CollapseList();
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

        // choose abilities
        while(true) {
            // find avaialble non-walk abilities
            List<TurnOption> options = controlTarget.Teammates.ConvertAll((Monster monster) => ChooseAction(monster))
                .FindAll((TurnOption option) => option.actionValue > 0);
    
            if(options.Count == 0) {
                break;
            }

            // TODO: order based on dependencies

            // execute a move
            TurnOption chosenOption = options[0];
            if(chosenOption.walkDestination.HasValue) {
                chosenOption.user.UseMove(MonsterType.WALK_INDEX, new List<Vector2Int> { chosenOption.walkDestination.Value });
            }
            if(chosenOption.abilitySlot.HasValue) {
                chosenOption.user.UseMove(chosenOption.abilitySlot.Value, chosenOption.abilityTargets);
            }
        }

        // when only moving is left, move to best positions
        foreach(Monster monster in controlTarget.Teammates) {
            if(monster.CanUse(MonsterType.WALK_INDEX)) {
                if(pathedPositions[monster] != monster.Tile) {
                    // update pathfinding if this monster has been moved
                    walkOptions[monster] = monster.GetMoveOptions(MonsterType.WALK_INDEX).CollapseList();
                }

                List<Vector2Int> bestTiles = walkOptions[monster].FindAll((Vector2Int tile) => LevelGrid.Instance.IsOpenTile(tile))
                    .AllTiedMax((Vector2Int tile) => DeterminePositionWeight(tile, targetResource.Tile));
                if(bestTiles.Count > 0) {
                    monster.UseMove(MonsterType.WALK_INDEX, new List<Vector2Int> { bestTiles[UnityEngine.Random.Range(0, bestTiles.Count)] });
                }
            }
        }

        AttemptCraft();

        // end turn after animations play out using event
    }

    private void EndTurn(Team currentTurn) {
        if(currentTurn == controlTarget) {
            controlTarget.EndTurn();
        }
    }

    // determines which non-walk ability to use, walking first if necessary. Returns TurnOption.None if there are no good options available
    private TurnOption ChooseAction(Monster monster) {
        List<int> usableMoveSlots = monster.GetUsableMoveSlots();
        bool canWalk = usableMoveSlots.Contains(MonsterType.WALK_INDEX);
        usableMoveSlots.Remove(MonsterType.WALK_INDEX); // only consider actual abilities
        if(usableMoveSlots.Count <= 0) {
            return TurnOption.None;
        }

        if(canWalk && pathedPositions[monster] != monster.Tile) {
            // if moved, find new end positions
            pathedPositions[monster] = monster.Tile;
            walkOptions[monster] = monster.GetMoveOptions(MonsterType.WALK_INDEX, false).CollapseList();
        }

        List<TurnOption> bestOptions = usableMoveSlots.ConvertAll((int slot) => DetermineBestOption(monster, slot, canWalk))
            .AllTiedMax((TurnOption option) => option.Effectiveness);
        return bestOptions.Exists((TurnOption option) => option.Effectiveness > 0) ? bestOptions[UnityEngine.Random.Range(0, bestOptions.Count)] : TurnOption.None;
    }

    // find the best way of using this move
    private TurnOption DetermineBestOption(Monster monster, int moveSlot, bool canMoveFirst) {
        // find all possible targets, considering a walk first if able
        Dictionary<Vector2Int, List<List<Vector2Int>>> positionTargetOptions = canMoveFirst ? 
            monster.GetMoveOptionsAfterWalk(moveSlot, false, walkOptions[monster].FindAll((Vector2Int tile) => LevelGrid.Instance.IsOpenTile(tile)))
            : new Dictionary<Vector2Int, List<List<Vector2Int>>>();

        positionTargetOptions[monster.Tile] = monster.GetMoveOptions(moveSlot);

        // determine the values of all options
        float startPositionValue = DeterminePositionWeight(monster.Tile, targetResource.Tile);
        List<TurnOption> allOptions = new List<TurnOption>();
        foreach(KeyValuePair<Vector2Int, List<List<Vector2Int>>> positionTargets in positionTargetOptions) {
            float posDelta = DeterminePositionWeight(positionTargets.Key, targetResource.Tile) - startPositionValue;
            foreach(List<Vector2Int> targetGroup in positionTargets.Value) {
                if(targetGroup.Count == 0) {
                    continue;
                }

                allOptions.Add(new TurnOption {
                    user = monster,
                    walkDestination = positionTargets.Key == monster.Tile ? null : positionTargets.Key,
                    abilitySlot = moveSlot,
                    abilityTargets = targetGroup,
                    actionValue = DetermineOptionValue(monster, moveSlot, targetGroup, positionTargets.Key),
                    positionDelta = posDelta
                });
            }
        }

        // choose the best option
        allOptions = allOptions.AllTiedMax((TurnOption option) => option.actionValue);
        TurnOption stillOption = allOptions.Find((TurnOption option) => option.walkDestination == null);
        return stillOption.user == null ? allOptions[UnityEngine.Random.Range(0, allOptions.Count)] : stillOption;
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
            if(move.TargetType == Move.Targets.Enemies) {
                StatusAilment effect = ((StatusMove)move).Condition;
                return 0.2f * (targets.Count * effect.duration);
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

        if(move.Type == MoveType.Decay) {
            // leech, wither, hex
            Monster target = level.GetMonster(targets[0]);
            float value = target.Health / 8f;
            return Mathf.Clamp(value, 0.2f, 0.7f);
        }

        /*if(move.Type == MoveType.Shift) {
            switch(user.Stats.Type) {
                case MonsterName.Phantom:
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
                    return 0.1f * Global.CalcTileDistance(userPosition, targets[0]) + 0.5f * enemiesHit;

                case MonsterName.Jackolantern:
                    // swap
                    return Global.CalcTileDistance(userPosition, targets[0]) * 0.1f;

                case MonsterName.Flytrap:
                    // pull
                    return 0.3f + 0.1f * (Global.CalcTileDistance(userPosition, targets[0]) - 1);

                case MonsterName.Beast:
                    // push
                    Vector2Int direction = targets[0] - userPosition;
                    for(int i = 1; i <= MonstersData.SHOVE_DIST; i++) {
                        Vector2Int tile = targets[0] + direction * i;
                        if(!level.IsInGrid(tile)) {
                            return 0f;
                        }

                        GridEntity occupant = level.GetEntity(tile);
                        WorldTile terrain = level.GetTile(tile);
                        if(occupant != null || !terrain.Walkable) {
                            if(occupant != null && occupant is Monster) {
                                return ((Monster)occupant).Controller == user.Controller ? 0f : 1.2f;
                            }
                            return 0.6f;
                        }
                    }
                    return 0.3f;
            }
        }*/

        // at this point the only moves not accounted for are sentry and thorns
        float result = -0.2f;
        foreach(Monster enemy in GameManager.Instance.OpponentOf(controlTarget).Teammates) {
            result += Mathf.Max(1f - Monster.FindPath(enemy.Tile, targets[0]).Count / 5f, 0f);
        }
        return result;
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

    // returns a value 0-1 that represents how far this starting position is from the end goal
    private float DeterminePositionWeight(Vector2Int startPosition, Vector2Int goal) {
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

    private void AttemptCraft() {
        if(controlTarget.Spawnpoint.CookState != Cauldron.State.Ready) {
            return;
        }

        List<MonsterName> buyOptions = new List<MonsterName>();
        foreach (MonsterName monsterType in System.Enum.GetValues(typeof(MonsterName))) {
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
        public Monster user;
        public Vector2Int? walkDestination;
        public int? abilitySlot;
        public List<Vector2Int> abilityTargets;
        public float actionValue; // how valuable the used attacks etc. are
        public float positionDelta; // how much better (or worse) the end position is

        public float Effectiveness { get { return actionValue + positionDelta; } }

        public static TurnOption None = new TurnOption {
            user = null,
            walkDestination = null,
            abilitySlot = null,
            abilityTargets = null,
            actionValue = 0f,
            positionDelta = 0f
        };
    }
}
