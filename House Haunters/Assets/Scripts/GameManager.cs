using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public CapturePoint Objective;

    public List<ResourcePile> AllResources { get; private set; }
    public Team CurrentTurn { get; private set; }
    public Team[] AllTeams { get; private set; }

    public delegate void TurnEndEvent(Team lastTurn, Team nextTurn);
    public event TurnEndEvent OnTurnEnd;

    private int currentTurnIndex;
    private AIController enemyAI;
    private AnimationsManager animator;

    void Awake() {
        Instance = this;
        AllTeams = new Team[2];
        AllTeams[0] = new Team(Color.blue, true);
        AllTeams[1] = new Team(Color.red, false);
        enemyAI = new AIController();
        AllResources = new List<ResourcePile>();

        CurrentTurn = AllTeams[currentTurnIndex];
    }

    // runs after everything else because of script execution order
    void Start() {
        animator = AnimationsManager.Instance;
        if(!CurrentTurn.IsAI) {
            MenuManager.Instance.StartPlayerTurn(CurrentTurn);
        }
    }

    void Update() {
        if(CurrentTurn.IsAI && !animator.Animating) {
            enemyAI.ChooseMove(CurrentTurn);
        }
    }

    public void PassTurn(Team turnEnder) {
        currentTurnIndex++;
        if(currentTurnIndex >= AllTeams.Length) {
            currentTurnIndex = 0;
        }
        CurrentTurn = AllTeams[currentTurnIndex];

        OnTurnEnd?.Invoke(turnEnder, CurrentTurn);
        CurrentTurn.StartTurn();
        if(!CurrentTurn.IsAI) {
            MenuManager.Instance.StartPlayerTurn(CurrentTurn);
        }
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
