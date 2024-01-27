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
            // TO DO: wait for animations to end before choosing the next move
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
        GameObject spawned = Instantiate(MonstersData.Instance.GetMonsterData(monsterType).Prefab);
        LevelGrid.Instance.PlaceEntity(spawned.GetComponent<GridEntity>(), startTile);
        controller.Join(spawned.GetComponent<Monster>());
    }

    public void DefeatMonster(Monster defeated) {
        LevelGrid.Instance.ClearEntity(defeated.Tile);

        if(defeated.Controller == PlayerTeam) {
            PlayerTeam.Remove(defeated);
        }
        else if(defeated.Controller == EnemyTeam) {
            EnemyTeam.Remove(defeated);
        }

        Destroy(defeated);
    }
}
