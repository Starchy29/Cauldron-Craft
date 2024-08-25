using System.Collections;
using System.Collections.Generic;
using System;
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

    private Team leftTeam;
    private Team rightTeam;

    public delegate void TurnEndEvent(Team lastTurn, Team nextTurn);
    public event TurnEndEvent OnTurnChange;
    public MonsterTrigger OnMonsterDefeated;

    void Awake() {
        Instance = this;
        leftTeam = new Team("Alchemists", new Color(0.1f, 0.5f, 0.9f), false);
        rightTeam = new Team("Witchcrafters", new Color(0.5f, 0.8f, 0.1f), Mode == GameMode.VSAI);
        AllResources = new List<ResourcePile>();
        CurrentTurn = leftTeam;
    }

    // runs after everything else because of script execution order
    void Start() {
        GameOverviewDisplayer.Instance.ShowTurnStart(CurrentTurn);
        CurrentTurn.StartTurn();
    }

    public Team GetTeam(bool onLeft) {
        return onLeft ? leftTeam : rightTeam;
    }

    public Team OpponentOf(Team team) {
        return team == leftTeam ? rightTeam : leftTeam;
    }

    public void PassTurn(Team turnEnder) {
        CurrentTurn = OpponentOf(CurrentTurn);
        GameOverviewDisplayer.Instance.ShowTurnStart(CurrentTurn);
        OnTurnChange?.Invoke(turnEnder, CurrentTurn);
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
        defeated.Controller.Teammates.Remove(defeated);
        OnMonsterDefeated?.Invoke(defeated); // during event, has no tile but retains team
    }

}
