using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController
{
    public AIController() {}

    public void ChooseMove(Team team) {
        foreach(Monster monster in team.Teammates) {
            List<List<Vector2Int>> options = monster.GetMoveOptions(0);
            monster.UseMove(0, options[Random.Range(0, options.Count)]);
        }
        GameManager.Instance.PassTurn(team);
    }
}
