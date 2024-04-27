using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Cauldron : GridEntity
{
    [SerializeField] private GameObject cookIndicator;

    private MonsterName? cookingMonster;
    private bool cookingTurn;

    public bool Cooking { get { return cookingMonster.HasValue; } }

    protected override void Start() {
        base.Start();
        Controller.OnTurnStart += FinishCook;
        Controller.OnTurnStart += NotifyCookable;
        Controller.Spawnpoint = this;
    }

    public void StartCook(MonsterName monsterType) {
        cookingMonster = monsterType;
        cookIndicator.SetActive(true);
        cookIndicator.GetComponent<SpriteRenderer>().sprite = PrefabContainer.Instance.monsterToSprite[monsterType];
    }

    private void FinishCook() {
        if(!cookingMonster.HasValue) {
            return;
        }

        // find the spot to spawn on
        LevelGrid level = LevelGrid.Instance;
        Vector2Int levelMid = new Vector2Int(level.Width / 2, level.Height / 2);
        List<Vector2Int> options = level.GetTilesInRange(Tile, 1, true).Filter((Vector2Int tile) => { return level.GetEntity(tile) == null; });
        if(options.Count == 0) {
            return;
        }

        options.Sort((Vector2Int current, Vector2Int next) => { return DetermineSpawnSpotPriority(current, levelMid) - DetermineSpawnSpotPriority(next, levelMid); });
        Vector2Int spawnSpot = options[0];

        // spawn the monster
        GameManager.Instance.SpawnMonster(cookingMonster.Value, spawnSpot, Controller);
        cookingMonster = null;
        cookIndicator.SetActive(false);
    }

    private void NotifyCookable() {
        int totalIngredients = 0;
        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            totalIngredients += Controller.Resources[ingredient];
        }

        if(totalIngredients >= 3) {
            cookIndicator.SetActive(true);
            cookIndicator.GetComponent<SpriteRenderer>().sprite = null;
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
