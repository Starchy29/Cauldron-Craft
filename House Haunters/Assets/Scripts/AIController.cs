using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController
{
    public AIController() {}

    // chooses 1 move at a time
    public void ChooseMove(Team team) {
        //team.EndTurn(); return;

        foreach(Monster monster in team.Teammates) {
            List<int> moveOptions = GetUsableMoveSlots(monster);
            if(moveOptions.Count == 0) {
                continue;
            }

            int chosenMoveSlot = moveOptions[Random.Range(0, moveOptions.Count)];
            Move chosenMove = monster.Stats.Moves[chosenMoveSlot];
            List<List<Vector2Int>> targetOptions = monster.GetMoveOptions(chosenMoveSlot);

            int chosenTargets = Random.Range(0, targetOptions.Count);
            if(chosenMove is MovementAbility) {
                Vector2Int targetPosition = new Vector2Int(LevelGrid.Instance.Width / 2, LevelGrid.Instance.Height / 2);
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
}
