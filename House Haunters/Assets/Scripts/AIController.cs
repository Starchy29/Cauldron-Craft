using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIController
{
    // playstyle values, 0-1
    //private float greed = 0.5f; // how much this player likes to control multiple resources at once
    //private float aggression; // how much this player prioritizes eliminating enemy monsters
    //private float persistence;

    private Team controlTarget;

    private static Dictionary<ResourcePile, ResourceData> resourceData;

    public AIController(Team team) {
        this.controlTarget = team;
        //AnimationsManager.Instance.OnAnimationsEnd += ChooseMove;
    }

    // enacts every move in the turn immediately. The animation queuer makes the visuals play out in the correct order
    public void TakeTurn() {
        // determine which resources to focus on
        resourceData = EvaluateResources();

        foreach(Monster teammate in controlTarget.Teammates) {
            ChooseMoves(teammate);
        }

        AttemptCraft();

        controlTarget.EndTurn();
    }

    private void ChooseMoves(Monster monster) {
        LevelGrid level = LevelGrid.Instance;
        Vector2Int lastPositon = monster.Tile + new Vector2Int(1, 1); // being different from the monster's position causes end position evaluation 
        List<List<Vector2Int>> walkableSpots = null;
        Dictionary<Vector2Int, float> positionWeights = new Dictionary<Vector2Int, float>();

        ResourcePile targetResource = GameManager.Instance.AllResources.Max(
            (ResourcePile resource) => {
                float distanceWeight = (20f - resourceData[resource].allyPaths[monster].Count) / 20f;
                return resourceData[resource].desirability * distanceWeight;
            }
        );

        List<TurnOption> allOptions = new List<TurnOption>();
        while(monster.MovesLeft > 0) {
            List<int> usableMoves = monster.GetUsableMoveSlots();
            if(usableMoves.Count == 0) {
                return;
            }
            bool canWalk = usableMoves.Contains(MonsterType.WALK_INDEX);
            float currentTileWeight = canWalk ? DeterminePositionWeight(monster, monster.Tile, targetResource.Tile) : 0f;

            // update walk end position weights when necessary
            if(canWalk && (walkableSpots == null || monster.Tile != lastPositon)) {
                lastPositon = monster.Tile;
                positionWeights.Clear();
                walkableSpots = monster.GetMoveOptions(MonsterType.WALK_INDEX);

                foreach(List<Vector2Int> spot in walkableSpots) {
                    positionWeights[spot[0]] = DeterminePositionWeight(monster, spot[0], targetResource.Tile);
                    
                    TileAffector effect = level.GetTile(spot[0]).CurrentEffect;
                    if(effect != null && effect.Controller != monster.Controller) {
                        positionWeights[spot[0]] -= 0.2f; // discourage movement options that end in an enemy zone
                    }
                }
            }

            allOptions.Clear();

            if(canWalk) {
                // add best end position as an option
                List<List<Vector2Int>> bestTiles = walkableSpots.AllTiedMax((List<Vector2Int> tile) => { return positionWeights[tile[0]]; });
                Vector2Int bestTile = bestTiles[UnityEngine.Random.Range(0, bestTiles.Count)][0];
                allOptions.Add(new TurnOption() { 
                    walkDestination = bestTile,
                    abilitySlot = null,
                    abilityTargets = null,
                    effectiveness = positionWeights[bestTile]
                });
            }

            // find the best option for each move
            foreach(int moveSlot in usableMoves) {
                if(moveSlot == MonsterType.WALK_INDEX) {
                    continue;
                }

                allOptions.Add(DetermineBestOption(monster, moveSlot));
            }

            if(canWalk && monster.MovesLeft > 1) {
                // consider using an ability then doing something else
                if(usableMoves.Count > 1) {
                    int bestOptionIndex = 0;
                    int secondBestOptionIndex = 0;
                    for(int i = 1; i < allOptions.Count; i++) {
                        if(allOptions[i].effectiveness > allOptions[bestOptionIndex].effectiveness) {
                            secondBestOptionIndex = bestOptionIndex;
                            bestOptionIndex = i;
                        }
                        else if(allOptions[i].effectiveness > allOptions[secondBestOptionIndex].effectiveness) {
                            secondBestOptionIndex = i;
                        }
                    }

                    float bestOptionWeight = allOptions[bestOptionIndex].effectiveness;
                    float secondBestWeight = allOptions[secondBestOptionIndex].effectiveness;

                    for(int i = 1; i < allOptions.Count; i++) {
                        TurnOption option = allOptions[i];
                        option.effectiveness += i == bestOptionIndex ? secondBestWeight : bestOptionWeight;
                        allOptions[i] = option;
                    }
                }

                // consider moving then using an ability
                for(int i = 0; i < monster.Stats.Moves.Length; i++) {
                    if(i == MonsterType.WALK_INDEX || monster.Cooldowns[i] > 0) {
                        continue;
                    }

                    TurnOption? option = DetermineBestWalkOption(monster, i, walkableSpots, positionWeights);
                    if(option.HasValue) {
                        allOptions.Add(option.Value);
                    }
                }
            } else {
                // add the value of the current tile to each option
                for(int i = 1; i < allOptions.Count; i++) {
                    TurnOption option = allOptions[i];
                    option.effectiveness += currentTileWeight;
                }
            }

            // consider not using a move and staying still
            allOptions.Add(new TurnOption {
                walkDestination = null,
                abilitySlot = null,
                effectiveness = currentTileWeight
            });

            // execute the best option
            List<TurnOption> bestOptions = allOptions.AllTiedMax((TurnOption option) => { return option.effectiveness; });
            TurnOption chosenOption = bestOptions[UnityEngine.Random.Range(0, bestOptions.Count)];

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
        }
    }

    // returns a value 0-1 that represents how far this starting position is from the end goal
    private float DeterminePositionWeight(Monster monster, Vector2Int startPosition, Vector2Int goal) {
        // TODO: weight spaces on the capture point a decent amount more than the path to it

        float tileDistance = monster.FindPath(goal, false, startPosition).Count - 1f; // distance to an orthog/diag adjacent tile
        if(startPosition.x != goal.x && startPosition.y != goal.y) {
            tileDistance--; // make diagonal corners worth the same as orthogonally adjacent
        }

        const float MAX_DISTANCE = 20f;
        return Mathf.Max(0f, (MAX_DISTANCE - tileDistance) / MAX_DISTANCE);
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
    private TurnOption? DetermineBestWalkOption(Monster monster, int moveSlot, List<List<Vector2Int>> walkableTiles, Dictionary<Vector2Int, float> tileWeights) {
        Dictionary<Vector2Int, List<List<Vector2Int>>> options = monster.GetMoveOptionsAfterWalk(moveSlot, false, walkableTiles);

        List<TurnOption> converted = new List<TurnOption>();
        foreach(KeyValuePair<Vector2Int, List<List<Vector2Int>>> keyVal in options) {
            foreach(List<Vector2Int> targetGroup in keyVal.Value) {
                converted.Add(new TurnOption {
                    walkDestination = keyVal.Key,
                    abilitySlot = moveSlot,
                    abilityTargets = targetGroup,
                    effectiveness = 0.5f + tileWeights[keyVal.Key]
                });
            }
        }

        if(converted.Count == 0) {
            return null; // nothing can be targeted
        }

        // TODO: add move heuristics
        List<TurnOption> bestOptions = converted.AllTiedMax((TurnOption option) => { return option.effectiveness; });
        return bestOptions[UnityEngine.Random.Range(0, bestOptions.Count)];
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
