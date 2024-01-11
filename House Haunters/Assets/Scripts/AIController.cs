using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController
{
    public Team TeamData { get; private set; }

    public AIController(Team team) {
        TeamData = team;
    }

    public void ChooseMove() {
        foreach(Monster monster in TeamData.Teammates) {
            List<List<Vector2Int>> options = monster.GetMoveOptions(0);
            monster.UseMove(0, options[Random.Range(0, options.Count)]);
        }
        GameManager.Instance.EndTurn(TeamData);
    }
}
