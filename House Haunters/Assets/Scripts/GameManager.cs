using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Team PlayerTeam { get; private set; }
    public Team EnemyTeam { get; private set; }
    public Team CurrentTurn { get; private set; }

    public delegate void TurnEndEvent(Team lastTurn, Team nextTurn);
    public event TurnEndEvent OnTurnEnd;

    private AIController enemyAI;
    private AnimationsManager animator;
    
    public List<ResourcePile> AllResources { get; private set; }

    void Awake() {
        Instance = this;
        PlayerTeam = new Team(Color.blue);
        EnemyTeam = new Team(Color.red);
        enemyAI = new AIController();
        AllResources = new List<ResourcePile>();

        CurrentTurn = PlayerTeam;
    }

    void Start() {
        animator = AnimationsManager.Instance;
    }

    void Update() {
        if(CurrentTurn == EnemyTeam && !animator.Animating) {
            enemyAI.ChooseMove(EnemyTeam);
        }
    }

    public void PassTurn(Team turnEnder) {
        if(turnEnder == PlayerTeam) {
            CurrentTurn = EnemyTeam;
        } else {
            CurrentTurn = PlayerTeam;
        }

        OnTurnEnd?.Invoke(turnEnder, CurrentTurn);
        MenuManager.Instance.UpdateResources();

        Team winner = null;//DetermineWinner();
        if(winner == null) {
            CurrentTurn.StartTurn();
        } else {
            Debug.Log(winner.TeamColor + " team wins.");
        }
    }

    public void SpawnMonster(MonsterName monsterType, Vector2Int startTile, Team controller) {
        Monster spawned = Instantiate(PrefabContainer.Instance.BaseMonsterPrefab).GetComponent<Monster>();
        spawned.Controller = controller;
        spawned.MonsterType = monsterType;
        spawned.transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)startTile);
        // grid placement and team joining handled by GridEntity.Start() and Monster.Start()
    }

    // removes the monster from the game state. Destroy the game object is handled by the DeathAnimator
    public void DefeatMonster(Monster defeated) {
        LevelGrid.Instance.ClearEntity(defeated.Tile);

        if(defeated.Controller == PlayerTeam) {
            PlayerTeam.Remove(defeated);
        }
        else if(defeated.Controller == EnemyTeam) {
            EnemyTeam.Remove(defeated);
        }
    }

    // declare a winner if one team controls all of the resources
    private Team DetermineWinner() {
        Team winner = null;
        foreach(ResourcePile resource in AllResources) {
            if(resource.Controller != null && winner == null) {
                winner = resource.Controller;
            }
            else if(resource.Controller != winner) {
                return null;
            }
        }

        return winner;
    }
}
