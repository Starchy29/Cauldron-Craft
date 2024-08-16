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

    public AIController(Team team) {
        this.controlTarget = team;
        AnimationsManager.Instance.OnAnimationsEnd += EndTurn;
    }

    // enacts every move in the turn immediately. The animation queuer makes the visuals play out in the correct order
    public void TakeTurn() {
        // focus attention on the resource that the attacking team is approaching
        resourceData = EvaluateResources();
        targetResource = GameManager.Instance.AllResources.Max((ResourcePile resource) => resourceData[resource].threatValue + (resourceData[resource].captured ? -9999f : 0f));

        // cache where the monsters can move to avoid repeated pathfinding
        pathedPositions.Clear();
        walkOptions.Clear();
        foreach(Monster monster in controlTarget.Teammates) {
            pathedPositions[monster] = monster.Tile;
            if(monster.CanUse(MonsterType.WALK_INDEX)) {
                walkOptions[monster] = monster.GetMoveOptions(MonsterType.WALK_INDEX, false).CollapseList();
            }
        }

        // choose abilities
        while(true) {
            // find avaialble non-walk abilities
            List<TurnOption> options = controlTarget.Teammates.ConvertAll((Monster monster) => ChooseAction(monster))
                .FindAll((TurnOption option) => option.Effectiveness > 0);
    
            if(options.Count == 0) {
                break;
            }

            // order based on dependencies

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

        List<TurnOption> bestOptions = usableMoveSlots.ConvertAll((int slot) => DetermineBestOption(monster, slot, canWalk && monster.MovesLeft > 1))
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
                allOptions.Add(new TurnOption {
                    user = monster,
                    walkDestination = positionTargets.Key == monster.Tile ? null : positionTargets.Key,
                    abilitySlot = moveSlot,
                    abilityTargets = targetGroup,
                    actionValue = DetermineOptionValue(monster, moveSlot, targetGroup),
                    positionDelta = posDelta
                });
            }
        }

        // choose the best option
        allOptions = allOptions.AllTiedMax((TurnOption option) => option.Effectiveness);
        return allOptions[UnityEngine.Random.Range(0, allOptions.Count)];
    }

    // returns a value that represents how valuable the usage of the input move on the input targets would be
    private float DetermineOptionValue(Monster monster, int moveSlot, List<Vector2Int> targets) {
        // TODO: add move heuristics
        LevelGrid level = LevelGrid.Instance;
        Move move = monster.Stats.Moves[moveSlot];

        if(move is Attack) {
            int totalDamage = 0;
            foreach(Monster hit in targets.ConvertAll((Vector2Int tile) => level.GetMonster(tile))) {
                totalDamage += hit.DetermineDamage(((Attack)move).Damage, monster);
            }
            return totalDamage / 10.0f;
        }

        if(move is ShieldMove) {
            // all shield moves currently have one target each
            Monster shielded = level.GetMonster(targets[0]);
            if(shielded.CurrentShield != null) {
                return -0.2f; // replacing a shield is bad
            }
        }

        if(move is StatusMove) {

        }

        if(move is ZoneMove) {

        }

        if(move.Type == MoveType.Heal) {
            // prioritize healing low health teammates
        }

        return 0.5f;
    }

    private Dictionary<ResourcePile, ResourceData> EvaluateResources() {
        Dictionary<ResourcePile, ResourceData> resourceData = new Dictionary<ResourcePile, ResourceData>();

        foreach(ResourcePile resource in GameManager.Instance.AllResources) {
            ResourceData data = new ResourceData();
            data.captured = resource.Controller == GameManager.Instance.Attacker;

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
        public bool captured;
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
