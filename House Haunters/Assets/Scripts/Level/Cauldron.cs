using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Cauldron : GridEntity
{
    [SerializeField] private bool controlledByLeft;
    [SerializeField] private GameObject cookIndicator;

    private MonsterName cookingMonster;

    public enum State {
        Ready,
        Cooking
    }

    public State CookState { get; private set; }

    protected override void Start() {
        base.Start();
        Controller = GameManager.Instance.GetTeam(controlledByLeft);
        Controller.Spawnpoint = this;
        Controller.OnTurnStart += FinishCook;
        SetOutlineColor(Controller.TeamColor);
    }

    public void StartCook(MonsterName monsterType) {
        if(CookState != State.Ready) {
            return;
        }

        CookState = State.Cooking;
        cookingMonster = monsterType;
        cookIndicator.GetComponent<SpriteRenderer>().sprite = PrefabContainer.Instance.monsterToSprite[monsterType];
        cookIndicator.SetActive(false);
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(cookIndicator, true));
    }

    public void FinishCook() {
        if(CookState != State.Cooking) {
            return;
        }

        // find the spot to spawn on
        LevelGrid level = LevelGrid.Instance;
        Vector2Int levelMid = new Vector2Int(level.Width / 2, level.Height / 2);
        List<Vector2Int> options = level.GetTilesInRange(Tile, 1, true).Filter((Vector2Int tile) => { return level.IsOpenTile(tile); });
        if(options.Count == 0) {
            return;
        }

        CookState = State.Ready;
        Vector2Int spawnSpot = options.Max((Vector2Int tile) => DetermineSpawnSpotPriority(tile, levelMid));

        // spawn the monster
        Monster spawned = GameManager.Instance.SpawnMonster(cookingMonster, spawnSpot, Controller);
        spawned.gameObject.SetActive(false);
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(spawned.gameObject, true));
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(cookIndicator, false));
    }

    private int DetermineSpawnSpotPriority(Vector2Int tile, Vector2Int levelMid) {
        Vector2Int toCenter = levelMid - Tile;
        bool horizontal = toCenter.x > toCenter.y;
        return (Global.CalcTileDistance(tile, Tile) > 1 ? -100 : 0) // prioritize orthogonally adjacent over diagonal
            + -10 * (horizontal ? Mathf.Abs(tile.x - levelMid.x) : Mathf.Abs(tile.y - levelMid.y))
            + -1 * (horizontal ? Mathf.Abs(tile.y - levelMid.y) : Mathf.Abs(tile.x - levelMid.x));
    }
}
