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

    void Awake() {
        Instance = this;
        PlayerTeam = new Team(Color.blue);
        EnemyTeam = new Team(Color.red);
        enemyAI = new AIController();
    }

    void Start() {
        animator = AnimationsManager.Instance;
        SpawnMonster(MonsterName.Demon, Vector2Int.zero, PlayerTeam);
        SpawnMonster(MonsterName.LostSoul, new Vector2Int(0, 1), PlayerTeam);

        SpawnMonster(MonsterName.Demon, new Vector2Int(4, 4), EnemyTeam);
        SpawnMonster(MonsterName.LostSoul, new Vector2Int(4, 5), EnemyTeam);

        CurrentTurn = PlayerTeam;
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
        CurrentTurn.StartTurn();
    }

    public void SpawnMonster(MonsterName monsterType, Vector2Int startTile, Team controller) {
        Monster spawned = Instantiate(PrefabContainer.Instance.BaseMonsterPrefab).GetComponent<Monster>();
        spawned.MonsterType = monsterType;
        LevelGrid.Instance.PlaceEntity(spawned.GetComponent<GridEntity>(), startTile);
        spawned.transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)startTile);
        controller.Join(spawned.GetComponent<Monster>());
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
}
