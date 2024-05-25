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
    [SerializeField] private GameObject floorCoverPrefab;
    public Ingredient Type { get { return type; } }
    private bool cooldown;

    protected override void Start() {
        base.Start();
        GameManager.Instance.OnTurnEnd += TurnEndCheck;
        GameManager.Instance.AllResources.Add(this);

        List<Vector2Int> openAdjTiles = LevelGrid.Instance.GetTilesInRange(Tile, 1, true).Filter((Vector2Int tile) => { return tile != this.Tile && LevelGrid.Instance.GetTile(tile).Walkable; });
        foreach(Vector2Int tile in openAdjTiles) {
            GameObject floorCover = Instantiate(floorCoverPrefab);
            floorCover.transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)tile);
            floorCover.transform.position = transform.position + 0.8f * (floorCover.transform.position - transform.position);

            floorCover.transform.localScale = new Vector3(Random.value < 0.5f ? 1f : 1f, Random.value < 0.5f ? 1f : 1f, 1f);
            floorCover.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 4) * 90f);
        }
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
