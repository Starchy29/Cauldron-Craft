using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Team PlayerTeam { get; private set; }
    public Team EnemyTeam { get; private set; }
    public Team CurrentTurn { get; private set; }

    private AIController enemyAI;
    private AnimationsManager animator;

    void Awake() {
        Instance = this;
        PlayerTeam = new Team();
        EnemyTeam = new Team();
        enemyAI = new AIController();
    }

    void Start() {
        animator = AnimationsManager.Instance;
        SpawnMonster(MonsterName.Temporary, Vector2Int.zero, PlayerTeam);
        SpawnMonster(MonsterName.Temporary, new Vector2Int(4, 4), EnemyTeam);

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
