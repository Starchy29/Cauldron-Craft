using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Cauldron : GridEntity
{
    [SerializeField] private GameObject cookIndicator;
    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite emptySprite;

    private MonsterName cookingMonster;

    public enum State {
        Ready,
        Cooking,
        Cooldown
    }

    public State CookState { get; private set; }

    protected override void Start() {
        base.Start();
        Controller.OnTurnStart += UpdateCook;
        Controller.OnTurnStart += NotifyCookable;
        Controller.OnTurnEnd += HideCookable;
        Controller.Spawnpoint = this;
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

    private void UpdateCook() {
        // pause during cooldown
        if(CookState == State.Cooldown) {
            CookState = State.Ready;
            spriteRenderer.sprite = fullSprite;
            return;
        }

        // nothing to cook yet
        if(CookState == State.Ready) {
            return;
        }

        // finish cook
        CookState = State.Cooldown;
        spriteRenderer.sprite = emptySprite;

        // find the spot to spawn on
        LevelGrid level = LevelGrid.Instance;
        Vector2Int levelMid = new Vector2Int(level.Width / 2, level.Height / 2);
        List<Vector2Int> options = level.GetTilesInRange(Tile, 1, true).Filter((Vector2Int tile) => { return !LevelGrid.Instance.GetTile(tile).IsWall && level.GetEntity(tile) == null; });
        if(options.Count == 0) {
            return;
        }

        options.Sort((Vector2Int current, Vector2Int next) => { return DetermineSpawnSpotPriority(current, levelMid) - DetermineSpawnSpotPriority(next, levelMid); });
        Vector2Int spawnSpot = options[0];

        // spawn the monster
        Monster spawned = GameManager.Instance.SpawnMonster(cookingMonster, spawnSpot, Controller);
        spawned.ApplyStatus(new StatusAilment(StatusEffect.Haste, 1, PrefabContainer.Instance.SpawnSpeedPrefab), null);
        cookIndicator.SetActive(false);
    }

    private void NotifyCookable() {
        if(CookState != State.Ready) {
            return;
        }

        int totalIngredients = 0;
        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            totalIngredients += Controller.Resources[ingredient];
        }

        if(totalIngredients >= 3) {
            cookIndicator.SetActive(true);
            cookIndicator.GetComponent<SpriteRenderer>().sprite = null;
        }
    }

    private void HideCookable() {
        if(CookState != State.Cooking) {
            cookIndicator.SetActive(false);
        }
    }

    private int DetermineSpawnSpotPriority(Vector2Int tile, Vector2Int levelMid) {
        Vector2Int toCenter = levelMid - Tile;
        bool horizontal = toCenter.x > toCenter.y;
        return -100 * (Global.CalcTileDistance(tile, Tile) <= 1 ? 1 : 0) // prioritize orthogonally adjacent over diagonal
            + 10 * (horizontal ? Mathf.Abs(tile.x - levelMid.x) : Mathf.Abs(tile.y - levelMid.y))
            + 1 * (horizontal ? Mathf.Abs(tile.y - levelMid.y) : Mathf.Abs(tile.x - levelMid.x));
    }
}
