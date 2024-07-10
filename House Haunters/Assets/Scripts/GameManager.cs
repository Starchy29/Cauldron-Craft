using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode {
    VSAI,
    PVP
}

public class GameManager : MonoBehaviour
{
    public static GameMode Mode = GameMode.VSAI;
    public static GameManager Instance { get; private set; }

    public List<ResourcePile> AllResources { get; private set; }
    public Team CurrentTurn { get; private set; }
    public Team[] AllTeams { get; private set; }

    public delegate void TurnEndEvent(Team lastTurn, Team nextTurn);
    public event TurnEndEvent OnTurnChange;

    private int currentTurnIndex;

    void Awake() {
        Instance = this;
        AllTeams = new Team[2];
        AllTeams[0] = new Team(Color.blue, false, Ingredient.Mineral);
        AllTeams[1] = new Team(Color.red, Mode == GameMode.VSAI, Ingredient.Flora);
        AllResources = new List<ResourcePile>();

        CurrentTurn = AllTeams[currentTurnIndex];
    }

    // runs after everything else because of script execution order
    void Start() {
        CurrentTurn.StartTurn();
        GameOverviewDisplayer.Instance.ShowTurnStart(currentTurnIndex);
    }

    public void PassTurn(Team turnEnder) {
        // end the current turn
        currentTurnIndex++;
        if(currentTurnIndex >= AllTeams.Length) {
            currentTurnIndex = 0;
        }
        CurrentTurn = AllTeams[currentTurnIndex];

        OnTurnChange?.Invoke(turnEnder, CurrentTurn);

        // start next turn
        GameOverviewDisplayer.Instance.ShowTurnStart(currentTurnIndex);
        CurrentTurn.StartTurn();
    }

    public Monster SpawnMonster(MonsterName monsterType, Vector2Int startTile, Team controller) {
        Monster spawned = Instantiate(PrefabContainer.Instance.BaseMonsterPrefab).GetComponent<Monster>();
        spawned.Setup(monsterType, controller);
        LevelGrid.Instance.PlaceEntity(spawned, startTile);
        spawned.transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)startTile);
        spawned.SetSpriteFlip(startTile.x > LevelGrid.Instance.Width / 2);
        spawned.UpdateSortingOrder();
        return spawned;
    }

    // removes the monster from the game state. Destroying the game object is handled by the DeathAnimator
    public void DefeatMonster(Monster defeated) {
        LevelGrid.Instance.ClearEntity(defeated.Tile);
        defeated.Controller.Remove(defeated);
    }
}
