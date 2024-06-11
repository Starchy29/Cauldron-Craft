using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController
{
    public enum Playstyle { // could be aggression value
        Offensive,
        Defensive
    }

    private Team team;

    private static Dictionary<ResourcePile, ResourceData> resourceData;
    private static Dictionary<Monster, MonsterTasks> tasks;

    public AIController(Team team) {
        this.team = team;
        team.OnTurnStart += PlanTurn;
        AnimationsManager.Instance.OnAnimationsEnd += ChooseMove;
    }

    public void PlanTurn() {
        // determine which resources to focus on
        resourceData = EvaluateResources();
        List<ResourcePile> pointsOfInterest = new List<ResourcePile>();
        foreach(ResourcePile resource in GameManager.Instance.AllResources) {
            if(resourceData[resource].controlValue > 0.5f) {
                pointsOfInterest.Add(resource);
                ResourceData data = resourceData[resource];
                data.idealAllocation = resourceData[resource].threatValue + resourceData[resource].Intensity / 4f;
                resourceData[resource] = data;
            }
        }


        // when attacking, evaluate whether this should retreat or invest more

            // order monsters

            // assign tasks
    }

    // chooses 1 move at a time
    public void ChooseMove(Team currentTurn) {
        if(currentTurn != team) {
            return;
        }

        Vector2Int targetPosition = FindTargetPosition();
        foreach(Monster monster in team.Teammates) {
            List<int> moveOptions = GetUsableMoveSlots(monster);
            if(moveOptions.Count == 0) {
                continue;
            }

            int chosenMoveSlot = moveOptions[Random.Range(0, moveOptions.Count)];

            // extra chance to choose an attack
            foreach(int moveSlot in moveOptions) {
                if(monster.Stats.Moves[moveSlot] is Attack && Random.value < 0.3f) {
                    chosenMoveSlot = moveSlot;
                    break;
                }
            }

            Move chosenMove = monster.Stats.Moves[chosenMoveSlot];

            List<List<Vector2Int>> targetOptions = monster.GetMoveOptions(chosenMoveSlot);

            int chosenTargets = Random.Range(0, targetOptions.Count);

            // when moving, bias towards the current objective
            if(chosenMove is MovementAbility) {
                targetOptions.Sort((List<Vector2Int> tile1, List<Vector2Int> tile2) => { return Global.CalcTileDistance(tile1[0], targetPosition) - Global.CalcTileDistance(tile2[0], targetPosition); });
                chosenTargets /= 4; // only choose from the better portion of options
            }

            monster.UseMove(chosenMoveSlot, targetOptions[chosenTargets]);
            return;
        }

        AttemptCraft();

        team.EndTurn();
    }

    private List<int> GetUsableMoveSlots(Monster monster) {
        List<int> moveOptions = new List<int>();
        for(int i = 0; i < monster.Stats.Moves.Length; i++) {
            if(monster.CanUse(i)) {
                moveOptions.Add(i);
            }
        }
        return moveOptions;
    }
    
    private Dictionary<ResourcePile, ResourceData> EvaluateResources() {
        Dictionary<ResourcePile, ResourceData> resourceData = new Dictionary<ResourcePile, ResourceData>();

        foreach(ResourcePile resource in GameManager.Instance.AllResources) {
            ResourceData data = new ResourceData();

            foreach(Monster ally in team.Teammates) {
                data.controlValue += CalculateInfluence(ally, resource);
            }

            foreach(Team opponent in GameManager.Instance.AllTeams) {
                if(opponent == team) {
                    continue;
                }

                foreach(Monster enemy in opponent.Teammates) {
                    data.threatValue += CalculateInfluence(enemy, resource);
                }
            }

            resourceData[resource] = data;
        }

        return resourceData;
    }

    private float CalculateInfluence(Monster monster, ResourcePile resource) {
        const int MAX_INFLUENCE_RANGE = 5;
        
        if(Global.CalcTileDistance(monster.Tile, resource.Tile) > MAX_INFLUENCE_RANGE + 2) {
            return 0;
        }

        int distance = monster.FindPath(resource.Tile, false).Count - 1; // distance to capture area
        if(monster.Tile.x != resource.Tile.x && monster.Tile.y != resource.Tile.y) {
            // make corners worth the same as orthogonally adjacent
            distance--;
        }

        if(distance > MAX_INFLUENCE_RANGE) {
            return 0f;
        }

        return (MAX_INFLUENCE_RANGE + 1 - distance) / (MAX_INFLUENCE_RANGE + 1f);
    }

    private Vector2Int FindTargetPosition() {
        ResourcePile closestUnclaimed = null;
        int closestDistance = 0;
        foreach(ResourcePile resource in GameManager.Instance.AllResources) {
            int distance = Global.CalcTileDistance(resource.Tile, team.Spawnpoint.Tile);
            if(resource.Controller != team && (closestUnclaimed == null || distance < closestDistance)) {
                closestUnclaimed = resource;
                closestDistance = distance;
            }
        }

        return closestUnclaimed == null ? Vector2Int.zero : closestUnclaimed.Tile;
    }

    private void AttemptCraft() {
        if(team.Spawnpoint.CookState != Cauldron.State.Ready) {
            return;
        }

        List<MonsterName> buyOptions = new List<MonsterName>();
        foreach(MonsterName monsterType in System.Enum.GetValues(typeof(MonsterName))) {
            if(team.CanBuy(monsterType)) {
                buyOptions.Add(monsterType);
            }
        }

        if(buyOptions.Count > 0) {
            team.BuyMonster(buyOptions[Random.Range(0, buyOptions.Count)]);
        }
    }

    private struct ResourceData {
        public float threatValue; // opponent's influence
        public float controlValue; // controller's influence

        public float idealAllocation; // the amount of influenece the AI would like to have with unlimited resources

        public float Intensity { get { return threatValue + controlValue; } } // amount of action at a control point
        public float Advantage { get { return controlValue - threatValue; } } // positive: winning, negative: losing
    }

    private struct MonsterTasks {
        public ResourcePile assignment;
    }
}
