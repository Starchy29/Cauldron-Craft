using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Team PlayerTeam { get; private set; }
    public Team EnemyTeam { get; private set; }
    public Team CurrentTurn { get; private set; }

    void Awake() {
        Instance = this;
        PlayerTeam = new Team();
        EnemyTeam = new Team();
        CurrentTurn = PlayerTeam;
    }

    void Start() {
        SpawnMonster(MonsterName.Temporary, Vector2Int.zero, PlayerTeam);
    }

    public void EndTurn() {
        if(CurrentTurn == PlayerTeam) {
            CurrentTurn = EnemyTeam;
        } else {
            CurrentTurn = PlayerTeam;
        }
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
