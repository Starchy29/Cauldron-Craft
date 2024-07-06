using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIController
{
    // playstyle values, 0-1
    private float greed = 0.5f; // how much this player likes to control multiple resources at once
    private float aggression; // how much this player prioritizes eliminating enemy monsters

    private Team controlTarget;
    //private float aggression; // how much this AI favors offense over defense
    //private float persistence; // how much this AI commits to its strategy

    private static Dictionary<ResourcePile, ResourceData> resourceData;

    public AIController(Team team) {
        this.controlTarget = team;
        //AnimationsManager.Instance.OnAnimationsEnd += ChooseMove;
    }

    // enacts every move in the turn immediately. The animation queuer makes the visuals play out in the correct order
    public void TakeTurn() {
        // determine which resources to focus on
        resourceData = EvaluateResources();

        //controlTarget.EndTurn();
        //return;

        foreach(Monster teammate in controlTarget.Teammates) {
            ChooseMoves(teammate);
        }

        //AttemptCraft();

        controlTarget.EndTurn();
    }

    private void ChooseMoves(Monster monster) {
        // find all tiles that this could move to this turn
        List<List<Vector2Int>> standableSpots = monster.GetMoveOptions(MonsterType.WALK_INDEX);

        // discourage movement options that end in an enemy zone

        List<TurnOption> allOptions = new List<TurnOption>();
        while(monster.MovesLeft > 0) {
            List<int> usableMoves = monster.GetUsableMoveSlots();
            if(usableMoves.Count == 0) {
                return;
            }

            allOptions.Clear();

            // find the best option for each move (without moving first)
            foreach(int moveSlot in usableMoves) {
                if(moveSlot == MonsterType.WALK_INDEX) {
                    continue;
                }

                allOptions.Add(DetermineBestOption(monster, moveSlot));
            }

            bool canWalk = usableMoves.Contains(MonsterType.WALK_INDEX);

            if(canWalk) {
                // add best end position as an option
                allOptions.Add(new TurnOption() { 
                    walkDestination = standableSpots[UnityEngine.Random.Range(0, standableSpots.Count)][0],
                    abilitySlot = null,
                    abilityTargets = null,
                    effectiveness = 0.2f
                });
            }

            if(canWalk && monster.MovesLeft > 1) {
                // consider moving then using an ability
                foreach(int moveSlot in usableMoves) {
                    if(moveSlot == MonsterType.WALK_INDEX) {
                        continue;
                    }

                    allOptions.Add(DetermineBestWalkOption(monster, moveSlot, standableSpots));
                }
            }

            // consider not using a move
            allOptions.Add(new TurnOption {
                walkDestination = null,
                abilitySlot = null,
                abilityTargets = null,
                effectiveness = 0f
            });

            // execute the best option
            TurnOption chosenOption = allOptions.Max((TurnOption option) => { return option.effectiveness; });

            if(!chosenOption.walkDestination.HasValue && !chosenOption.abilitySlot.HasValue) {
                return; // choose to end turn with abilities left over
            }

            if(chosenOption.walkDestination.HasValue) {
                monster.UseMove(MonsterType.WALK_INDEX, new List<Vector2Int>(){ chosenOption.walkDestination.Value });
            }

            if(chosenOption.abilitySlot.HasValue &&
                monster.GetMoveOptions(chosenOption.abilitySlot.Value).Find((List<Vector2Int> match) => { return match.AreContentsEqual(chosenOption.abilityTargets); }) != null
            ) {
                monster.UseMove(chosenOption.abilitySlot.Value, chosenOption.abilityTargets);
            }
            
            // if there would be leftover moves, consider the best possible move after this
            // monster.UseMove();

            // reevaluate movement options if the last move changed position
        }
    }

    // returns the best way of using this moveslot without moving to another tile
    private TurnOption DetermineBestOption(Monster monster, int moveSlot) {
        List<List<Vector2Int>> targetGroups = monster.GetMoveOptions(moveSlot);

        // TODO: add move heuristics

        // for now, random
        return new TurnOption {
            walkDestination = null,
            abilitySlot = moveSlot,
            abilityTargets = targetGroups[UnityEngine.Random.Range(0, targetGroups.Count)],
            effectiveness = 0.5f
        };
    }

    // returns the best way of using this moveslot after moving to another tile
    private TurnOption DetermineBestWalkOption(Monster monster, int moveSlot, List<List<Vector2Int>> walkableTiles) {
        Dictionary<Vector2Int, List<List<Vector2Int>>> options = monster.GetMoveOptionsAfterWalk(moveSlot, false, walkableTiles);

        // TODO: add move heuristics

        // for now, random
        Vector2Int moveTile = walkableTiles[UnityEngine.Random.Range(0, walkableTiles.Count)][0];
        return new TurnOption {
            walkDestination = moveTile,
            abilitySlot = moveSlot,
            abilityTargets = options[moveTile][UnityEngine.Random.Range(0, options[moveTile].Count)],
            effectiveness = 0.8f
        };
    }

    private void DEPRECATEDChooseMove() {
        foreach(Monster monster in controlTarget.Teammates) {
            List<int> moveOptions = monster.GetUsableMoveSlots();
            if(moveOptions.Count == 0) {
                continue;
            }

            int chosenMoveSlot = moveOptions[UnityEngine.Random.Range(0, moveOptions.Count)];

            // extra chance to choose an attack
            foreach(int moveSlot in moveOptions) {
                if(monster.Stats.Moves[moveSlot] is Attack && UnityEngine.Random.value < 0.3f) {
                    chosenMoveSlot = moveSlot;
                    break;
                }
            }

            Move chosenMove = monster.Stats.Moves[chosenMoveSlot];

            List<List<Vector2Int>> targetOptions = monster.GetMoveOptions(chosenMoveSlot);

            // when moving, bias towards the current objective
            if(chosenMove is MovementAbility) {
                //targetOptions.Sort((List<Vector2Int> tile1, List<Vector2Int> tile2) => { return Global.CalcTileDistance(tile1[0], targetPosition) - Global.CalcTileDistance(tile2[0], targetPosition); });
                //chosenTargets /= 4; // only choose from the better portion of options

                // choose which resource to move towards
                ResourcePile targetResource = GameManager.Instance.AllResources.Max(
                    (ResourcePile resource) => {
                        float distanceWeight = (20f - resourceData[resource].allyPaths[monster].Count) / 20f;
                        return resourceData[resource].desirability * distanceWeight; 
                    }
                );

                // if within range of the point, only move onto tiles that are in capture range
                List<Vector2Int> moveSpot;
                List<List<Vector2Int>> onCapture = targetOptions.FindAll((List<Vector2Int> tileCont) => { return targetResource.IsInCaptureRange(tileCont[0]);});
                if(onCapture.Count > 0) {
                    moveSpot = onCapture[UnityEngine.Random.Range(0, onCapture.Count)];
                } else {
                    // follow the path to the desired resource
                    moveSpot = targetOptions.Max((List<Vector2Int> tileGroup) => { return resourceData[targetResource].allyPaths[monster].IndexOf(tileGroup[0]); });
                }
                monster.UseMove(chosenMoveSlot, moveSpot);
                return;
            }

            int chosenTargets = UnityEngine.Random.Range(0, targetOptions.Count);
            monster.UseMove(chosenMoveSlot, targetOptions[chosenTargets]);
            return;
        }

        //AttemptCraft();

        controlTarget.EndTurn();
    }
    
    private Dictionary<ResourcePile, ResourceData> EvaluateResources() {
        Dictionary<ResourcePile, ResourceData> resourceData = new Dictionary<ResourcePile, ResourceData>();

        Dictionary<Team, Dictionary<Ingredient, float>> teamNeededResources = new Dictionary<Team, Dictionary<Ingredient, float>>();
        foreach(Team team in GameManager.Instance.AllTeams) {
            teamNeededResources[team] = GetIngredientPriorities(team);
        }

        foreach(ResourcePile resource in GameManager.Instance.AllResources) {
            ResourceData data = new ResourceData();

            // determine how much influence each team has on this resource
            data.allyPaths = new Dictionary<Monster, List<Vector2Int>>();
            foreach(Monster ally in controlTarget.Teammates) {
                data.allyPaths[ally] = ally.FindPath(resource.Tile, false);
                data.controlValue += CalculateInfluence(ally, resource, data.allyPaths[ally]);
            }

            foreach(Team opponent in GameManager.Instance.AllTeams) {
                if(opponent == controlTarget) {
                    continue;
                }

                foreach(Monster enemy in opponent.Teammates) {
                    data.threatValue += CalculateInfluence(enemy, resource);
                }
            }

            // determine how much each team needs this resource
            foreach(Team team in GameManager.Instance.AllTeams) {
                data.desirability += teamNeededResources[team][resource.Type] * (team == controlTarget ? 1f : 0.5f);
            }

            resourceData[resource] = data;
        }

        return resourceData;
    }

    private float CalculateInfluence(Monster monster, ResourcePile resource, List<Vector2Int> existingPath = null) {
        const int MAX_INFLUENCE_RANGE = 5;
        
        if(Global.CalcTileDistance(monster.Tile, resource.Tile) > MAX_INFLUENCE_RANGE + 2) { // influence range may be 2 greater than the actual distance
            return 0;
        }

        // find the distance to the capture zone
        int distance = (existingPath == null ? monster.FindPath(resource.Tile, false) : existingPath).Count - 1;
        if(monster.Tile.x != resource.Tile.x && monster.Tile.y != resource.Tile.y) {
            // make corners worth the same as orthogonally adjacent
            distance--;
        }

        if(distance > MAX_INFLUENCE_RANGE) {
            return 0f;
        }

        return (MAX_INFLUENCE_RANGE + 1 - distance) / (MAX_INFLUENCE_RANGE + 1f);
    }

    // determines how much this monster is encouraged to move to this tile
    private float DetermineTileForce(Monster mover, Vector2Int tile) {
        const float MAX_FORCE_RANGE = 15f;
        float force = 0f;
        foreach(ResourcePile resource in GameManager.Instance.AllResources) {
            int pathIndex = resourceData[resource].allyPaths[mover].IndexOf(tile);
            if(pathIndex >= 0) {
                int pathDistance = resourceData[resource].allyPaths[mover].Count - pathIndex;
                float distanceScale = 1f - pathDistance / MAX_FORCE_RANGE;
                if(distanceScale < 0f) {
                    continue;
                }
            }
        }
        DebugHelp.Instance.MarkTile(tile, force.ToString("F2"));
        return force;
    }

    private void AttemptCraft() {
        if(controlTarget.Spawnpoint.CookState != Cauldron.State.Ready) {
            return;
        }

        List<MonsterName> buyOptions = new List<MonsterName>();
        List<MonsterName> newOptions = new List<MonsterName>();
        foreach (MonsterName monsterType in System.Enum.GetValues(typeof(MonsterName))) {
            if(controlTarget.CanAfford(monsterType)) {
                buyOptions.Add(monsterType);
                if(!controlTarget.CraftedMonsters[monsterType]) {
                    newOptions.Add(monsterType);
                }
            }
        }

        if(newOptions.Count > 0) {
            controlTarget.BuyMonster(newOptions[UnityEngine.Random.Range(0, newOptions.Count)]);
        }
        else if(buyOptions.Count > 0) {
            controlTarget.BuyMonster(buyOptions[UnityEngine.Random.Range(0, buyOptions.Count)]);
        }
    }

    // returns a dictionary where each ingredient has an entry from 0-1 which indicates the ratio of how many are needed relative to the others
    private Dictionary<Ingredient, float> GetIngredientPriorities(Team team) {
        Dictionary<Ingredient, float> result = new Dictionary<Ingredient, float>();
        
        // reduce by the amount of ingredients in the inventory
        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            result[ingredient] = -team.Resources[ingredient];
        }

        // add up the recipes of all monster types
        Dictionary<MonsterName, bool> crafted = team.CraftedMonsters;
        foreach(MonsterName monster in Enum.GetValues(typeof(MonsterName))) {
            if(!crafted[monster]) {
                MonsterType data = MonstersData.Instance.GetMonsterData(monster);
                foreach(Ingredient ingredient in data.Recipe) {
                    result[ingredient]++;
                }
            }
        }

        // remove neagtive amounts and find total
        float total = 0;
        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            result[ingredient] = Mathf.Max(result[ingredient], 0f);
            total += result[ingredient];
        }

        if(total == 0) {
            return result;
        }

        // change counts to weights
        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            result[ingredient] = result[ingredient] / total;
        }

        return result;
    }

    private struct ResourceData {
        public Dictionary<Monster, List<Vector2Int>> allyPaths;
        public float threatValue; // opponent's influence
        public float controlValue; // controller's influence
        public float desirability;

        public float Intensity { get { return threatValue + controlValue; } } // amount of action at a control point
        public float Advantage { get { return controlValue - threatValue; } } // positive: winning, negative: losing
    }

    private struct TurnOption {
        public Vector2Int? walkDestination;
        public int? abilitySlot;
        public List<Vector2Int> abilityTargets;
        public float effectiveness;
    }
}
