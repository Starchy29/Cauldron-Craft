using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode {
    VSAI,
    PVP
}

public class GameManager : MonoBehaviour
{
    public static GameMode GameMode = GameMode.PVP;
    public static GameManager Instance { get; private set; }

    public List<ResourcePile> AllResources { get; private set; }
    public Team CurrentTurn { get; private set; }
    public Team[] AllTeams { get; private set; }

    public delegate void TurnEndEvent(Team lastTurn, Team nextTurn);
    public event TurnEndEvent OnTurnEnd;

    private int currentTurnIndex;
    private AnimationsManager animator;

    void Awake() {
        Instance = this;
        AllTeams = new Team[2];
        AllTeams[0] = new Team(Color.blue, false, Ingredient.Flora);
        AllTeams[1] = new Team(Color.red, GameMode == GameMode.VSAI, Ingredient.Decay);
        AllResources = new List<ResourcePile>();

        CurrentTurn = AllTeams[currentTurnIndex];
    }

    // runs after everything else because of script execution order
    void Start() {
        animator = AnimationsManager.Instance;
        CurrentTurn.StartTurn();
        if(CurrentTurn.AI == null) {
            MenuManager.Instance.StartPlayerTurn(CurrentTurn);
        }
    }

    public void PassTurn(Team turnEnder) {
        // end the current turn
        currentTurnIndex++;
        if(currentTurnIndex >= AllTeams.Length) {
            currentTurnIndex = 0;
        }
        CurrentTurn = AllTeams[currentTurnIndex];

        OnTurnEnd?.Invoke(turnEnder, CurrentTurn);

        // check for victory
        Team winner = AllResources[0].Controller;
        foreach(ResourcePile dispenser in AllResources) {
            if(dispenser.Controller == null || dispenser.Controller != winner) {
                winner = null;
                break;
            }
        }
        if(winner != null) {
            GameOverviewDisplayer.Instance.ShowWinner(winner);
            return;
        }

        // start next turn
        CurrentTurn.StartTurn();
        GameOverviewDisplayer.Instance.ShowTurnStart(currentTurnIndex);
    }

    public void SpawnMonster(MonsterName monsterType, Vector2Int startTile, Team controller) {
        Monster spawned = Instantiate(PrefabContainer.Instance.BaseMonsterPrefab).GetComponent<Monster>();
        spawned.Setup(monsterType, controller);
        LevelGrid.Instance.PlaceEntity(spawned, startTile);
        spawned.transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)startTile);
    }

    // removes the monster from the game state. Destroying the game object is handled by the DeathAnimator
    public void DefeatMonster(Monster defeated) {
        LevelGrid.Instance.ClearEntity(defeated.Tile);
        defeated.Controller.Remove(defeated);
    }
}
