using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController
{
    public AIController() {}

    // chooses 1 move at a time
    public void ChooseMove(Team team) {
        //team.EndTurn(); return;
        List<MonsterName> buyOptions = new List<MonsterName>();
        foreach(MonsterName monsterType in System.Enum.GetValues(typeof(MonsterName))) {
            if(team.CanBuy(monsterType)) {
                buyOptions.Add(monsterType);
            }
        }
        if(buyOptions.Count > 0) {
            team.BuyMonster(buyOptions[Random.Range(0, buyOptions.Count)]);
        }

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
                Vector2Int targetPosition = FindTargetPosition(team);
                targetOptions.Sort((List<Vector2Int> tile1, List<Vector2Int> tile2) => { return Global.CalcTileDistance(tile1[0], targetPosition) - Global.CalcTileDistance(tile2[0], targetPosition); });
                chosenTargets /= 2; // only choose from the better half of options
            }

            monster.UseMove(chosenMoveSlot, targetOptions[chosenTargets]);
            return;
        }

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

    private Vector2Int FindTargetPosition(Team controller) {
        if(GameManager.Instance.Objective.Controller != controller) {
            return GameManager.Instance.Objective.Tile;
        }

        ResourcePile closestUnclaimed = null;
        int closestDistance = 0;
        foreach(ResourcePile resource in GameManager.Instance.AllResources) {
            int distance = Global.CalcTileDistance(resource.Tile, controller.Spawnpoint.Tile);
            if(resource.Controller != controller && (closestUnclaimed == null || distance < closestDistance)) {
                closestUnclaimed = resource;
                closestDistance = distance;
            }
        }

        if(closestUnclaimed == null) {
            return GameManager.Instance.Objective.Tile;
        }

        return closestUnclaimed.Tile;
    }
}
