using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapturePoint : Capturable
{
    [SerializeField] private ScoreboardScript scoreboard;
    [SerializeField] private int winAmount;
    public Dictionary<Team, int> Points { get; private set; } = new Dictionary<Team, int>();

    protected override void Start() {
        base.Start();
        GameManager.Instance.OnTurnEnd += GrantPoint;
        scoreboard.Setup(winAmount);

        foreach(Team team in GameManager.Instance.AllTeams) {
            Points[team] = 0;
        }
    }

    private void GrantPoint(Team turnEnder, Team nextTurn) {
        if(turnEnder == Controller) {
            Points[Controller]++;
            scoreboard.UpdateDisplay(Points);

            if(Points[Controller] >= winAmount) {
                // win game
            }
        }
    }
}
