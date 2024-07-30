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
    public static GameMode Mode = GameMode.PVP;
    public static GameManager Instance { get; private set; }

    public List<ResourcePile> AllResources { get; private set; }
    public Team CurrentTurn { get; private set; }

    public Team Attacker { get; private set; }
    public Team Defender { get; private set; }

    public delegate void TurnEndEvent(Team lastTurn, Team nextTurn);
    public event TurnEndEvent OnTurnChange;
    public MonsterTrigger OnMonsterDefeated;

    void Awake() {
        Instance = this;
        Defender = new Team(Color.blue, true);
        Attacker = new Team(Color.red, Mode == GameMode.VSAI);
        AllResources = new List<ResourcePile>();

        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            Attacker.Resources[ingredient] = 9;
        }

        CurrentTurn = Attacker;
    }

    // runs after everything else because of script execution order
    void Start() {
        CurrentTurn.StartTurn();
        GameOverviewDisplayer.Instance.ShowTurnStart(CurrentTurn);
    }

    public Team OpponentOf(Team team) {
        return team == Attacker ? Defender : Attacker;
    }

    public void PassTurn(Team turnEnder) {
        // check for offense victory
        if(AllResources.Find((ResourcePile resource) => resource.Controller == Defender) == null) {
            GameOverviewDisplayer.Instance.ShowWinner(Attacker);
            return;
        }

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

        // check for defense victory
        if(Attacker.Teammates.Count == 0 && Attacker.TotalIngredients < 3) {
            GameOverviewDisplayer.Instance.ShowWinner(Defender);
        }
    }

}
