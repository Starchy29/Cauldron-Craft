using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Cauldron : GridEntity
{
    [SerializeField] private bool controlledByLeft;
    [SerializeField] private GameObject indicator;
    [SerializeField] private SpriteRenderer cookIndicator;
    [SerializeField] private TMPro.TextMeshPro craftCounter;

    private Particle cookAnimator;
    private Sprite defaultSprite;
    private MonsterName cookingMonster;
    private bool newCraft;

    public enum State {
        Ready,
        Cooking
    }

    public State CookState { get; private set; }

    protected override void Start() {
        base.Start();
        defaultSprite = spriteRenderer.sprite;
        cookAnimator = spriteRenderer.gameObject.GetComponent<Particle>();
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
        newCraft = !Controller.CraftedMonsters[monsterType];

        cookIndicator.sprite = PrefabContainer.Instance.monsterToSprite[monsterType];
        indicator.SetActive(false);
        AnimationsManager.Instance.QueueFunction(() => { SetCookVisual(true); });
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(indicator, true));
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(cookIndicator.gameObject, true));
    }

    public void FinishCook() {
        if(CookState != State.Cooking) {
            return;
        }

        // find the spot to spawn on
        LevelGrid level = LevelGrid.Instance;
        Vector2Int levelMid = new Vector2Int(level.Width / 2, level.Height / 2);
        List<Vector2Int> options = level.GetTilesInRange(Tile, 1, true).FindAll((Vector2Int tile) => { return level.IsOpenTile(tile); });
        if(options.Count == 0) {
            return;
        }

        CookState = State.Ready;
        Vector2Int spawnSpot = options.Max((Vector2Int tile) => DetermineSpawnSpotPriority(tile, levelMid));

        // spawn the monster
        Monster spawned = GameManager.Instance.SpawnMonster(cookingMonster, spawnSpot, Controller);
        spawned.gameObject.SetActive(false);
        AnimationsManager.Instance.QueueAnimation(new CameraAnimator(transform.position));
        AnimationsManager.Instance.QueueFunction(() => {
            spawned.gameObject.SetActive(true);
            cookIndicator.gameObject.SetActive(false);
            if(newCraft) {
                craftCounter.gameObject.SetActive(true);
                craftCounter.text = Controller.TotalCrafted + "/13";
            } else {
                indicator.SetActive(false);
            }
            SetCookVisual(false); 
            Instantiate(PrefabContainer.Instance.spawnSmoke).transform.position = spawned.SpriteModel.transform.position + new Vector3(0, 0.2f, 0);
        });
        AnimationsManager.Instance.QueueAnimation(new PauseAnimator(2.0f));
        AnimationsManager.Instance.QueueFunction(() => {
            indicator.SetActive(false);
            craftCounter.gameObject.SetActive(false);
        });
    }

    private int DetermineSpawnSpotPriority(Vector2Int tile, Vector2Int levelMid) {
        return (Global.CalcTileDistance(tile, Tile) > 1 ? -100 : 0) // prioritize orthogonally adjacent over diagonal
            + -10 * Mathf.Abs(tile.x - levelMid.x)
            + -1 * Mathf.Abs(tile.y - levelMid.y);
    }

    private void SetCookVisual(bool cooking) {
        if(cooking) {
            cookAnimator.enabled = true;
        } else {
            cookAnimator.enabled = false;
            spriteRenderer.sprite = defaultSprite;
        }
    }
}
