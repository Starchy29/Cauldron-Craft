using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum GameMode {
    VSAI,
    PVP,
    Auto
}

public class GameManager : MonoBehaviour
{
    public static GameMode Mode = GameMode.PVP;
    public static GameManager Instance { get; private set; }

    public List<ResourcePile> AllResources { get; private set; }
    public Team CurrentTurn { get; private set; }

    private Team leftTeam;
    private Team rightTeam;
    public Team[] AllTeams { get { return new Team[] { leftTeam, rightTeam }; } }

    public delegate void TurnEndEvent(Team lastTurn, Team nextTurn);
    public event TurnEndEvent OnTurnChange;
    public MonsterTrigger OnMonsterDefeated;

    void Awake() {
        Instance = this;
        leftTeam = new Team(Team.Alchemists, Mode == GameMode.Auto);
        rightTeam = new Team(Team.Occultists, Mode != GameMode.PVP);
        AllResources = new List<ResourcePile>();
        CurrentTurn = leftTeam;
    }

    // runs after everything else because of script execution order
    void Start() {
        leftTeam.SpawnStartTeam();
        rightTeam.SpawnStartTeam();
        foreach(ResourcePile resource in AllResources) {
            LevelHighlighter.Instance.UpdateCapture(resource);
        }

        //QueueIntro();
        AnimationsManager.Instance.QueueAnimation(new CameraAnimator(CurrentTurn.Spawnpoint.transform.position));
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
        AnimationsManager.Instance.QueueAnimation(new CameraAnimator(CurrentTurn.Spawnpoint.transform.position));
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
        // check for loss due to no remaining monsters
        if(defeated.Controller.Teammates.Count == 1 && defeated.Controller.TotalIngredients < 3 && defeated.Controller.Spawnpoint.CookState == Cauldron.State.Ready) {
            GameOverviewDisplayer.Instance.ShowWinner(OpponentOf(defeated.Controller));
            return;
        }

        LevelGrid.Instance.ClearEntity(defeated.Tile);
        defeated.Controller.Teammates.Remove(defeated);
        if(defeated.Controller.AI != null) {
            defeated.Controller.AI.RemoveMonster(defeated);
        }
        OnMonsterDefeated?.Invoke(defeated); // during event, has no tile but retains team
    }

    private void QueueIntro() {
        GameOverviewDisplayer.Instance.ShowObjective();
        AllResources.Sort((ResourcePile cur, ResourcePile next) => (int)(-cur.transform.position.y + next.transform.position.y)); // sort top to bottom
        foreach(ResourcePile resource in AllResources) {
            AnimationsManager.Instance.QueueAnimation(new CameraAnimator(resource.transform.position));
            AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(resource.productionIndicator, true));
            AnimationsManager.Instance.QueueAnimation(new PauseAnimator(1.5f));
            AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(resource.productionIndicator, false));
        }
    }
}
