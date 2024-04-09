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
            List<int> moveOptions = monster.GetUsableMoveSlots();
            if(moveOptions.Count == 0) {
                continue;
            }

            int chosenMove = moveOptions[Random.Range(0, moveOptions.Count)];
            List<List<Vector2Int>> targets = monster.GetMoveOptions(chosenMove);
            monster.UseMove(chosenMove, targets[Random.Range(0, targets.Count)]);
            return;
        }

        team.EndTurn();
    }
}
