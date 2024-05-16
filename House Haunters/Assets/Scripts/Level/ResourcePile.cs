using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Ingredient
{
    Decay,
    Flora,
    Mineral,
    Swarm
}

public class ResourcePile : GridEntity
{
    [SerializeField] private Ingredient type;
    public Ingredient Type { get { return type; } }
    private bool cooldown;

    protected override void Start() {
        base.Start();
        GameManager.Instance.OnTurnEnd += TurnEndCheck;
        GameManager.Instance.AllResources.Add(this);
    }

    private void TurnEndCheck(Team turnEnder, Team nextTurn) {
        Team startController = Controller;
        CheckCapture();

        if(turnEnder != startController || Controller != startController) {
            return;
        }
        
        if(cooldown) {
            cooldown = false;
            return;
        }

        Controller.AddResource(type);
        cooldown = true;
    }

    private void CheckCapture() {
        LevelGrid level = LevelGrid.Instance;
        List<Monster> adjacentMonsters = level.GetTilesInRange(Tile, 1, true)
            .Map((Vector2Int tile) => { return level.GetMonster(tile); })
            .Filter((Monster monster) => { return monster != null; });

        if(adjacentMonsters.Count == 0) {
            return;
        }

        Dictionary<Team, int> teamCounts = new Dictionary<Team, int>();
        foreach(Monster monster in adjacentMonsters) {
            if(!teamCounts.ContainsKey(monster.Controller)) {
                teamCounts[monster.Controller] = 0;
            }
            teamCounts[monster.Controller]++;
        }

        Team capturer = null;
        int captureGoal = (Controller != null && teamCounts.ContainsKey(Controller) ? teamCounts[Controller] : 0);
        foreach(Team team in teamCounts.Keys) {
            if(team == Controller) {
                // look for another team with more monsters
                continue;
            }

            if(teamCounts[team] > captureGoal) {
                captureGoal = teamCounts[team];
                capturer = team;
            }
        }

        if(capturer != null) {
            Controller = capturer;
            Controller.AddResource(type);
            Controller.AddResource(type);
            cooldown = false;
        }
    }
}
