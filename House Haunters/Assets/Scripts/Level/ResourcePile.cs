using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Ingredient
{
    Decay,
    Flora,
    Mineral,
    //Swarm
}

public class ResourcePile : GridEntity
{
    [SerializeField] private Ingredient type;
    [SerializeField] private GameObject floorCoverPrefab;
    [SerializeField] public GameObject productionIndicator;
    [SerializeField] private RadialParticle captureVisual;
    
    public const int CAPTURE_SIZE = 2;

    public Ingredient Type { get { return type; } }
    public bool Contested { get; private set; } // both teams present

    protected override void Start() {
        base.Start();
        LevelGrid.Instance.OnMonsterMove += CheckCapture;
        GameManager.Instance.OnMonsterDefeated += CheckCapture;
        GameManager.Instance.OnTurnChange += GrantResource;
        GameManager.Instance.AllResources.Add(this);

        // place particles on the ground around this tile
        List<Vector2Int> openAdjTiles = LevelGrid.Instance.GetTilesInRange(Tile, CAPTURE_SIZE, true).FindAll((Vector2Int tile) => { return tile != this.Tile && LevelGrid.Instance.GetTile(tile).Walkable; });
        foreach(Vector2Int tile in openAdjTiles) {
            GameObject floorCover = Instantiate(floorCoverPrefab);
            floorCover.transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)tile);
            floorCover.transform.position = transform.position + 0.8f * (floorCover.transform.position - transform.position);

            floorCover.transform.localScale = new Vector3(Random.value < 0.5f ? 1f : 1f, Random.value < 0.5f ? 1f : 1f, 1f);
            floorCover.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 4) * 90f);
        }
    }

    public bool IsInCaptureRange(Vector2Int tile) {
        return Mathf.Abs(tile.x - Tile.x) <= CAPTURE_SIZE && Mathf.Abs(tile.y - Tile.y) <= CAPTURE_SIZE;
    }

    private void GrantResource(Team turnEnder, Team turnStarter) {
        int harvestCount = 0;
        if(Contested) {
            harvestCount = 1;
        }
        else if(turnStarter == Controller) {
            harvestCount = 2;
        }

        if(harvestCount == 0) {
            return;
        }

        turnStarter.Resources[type] += harvestCount;

        AnimationsManager.Instance.QueueAnimation(new CameraAnimator(transform.position));
        for(int i = 0; i < harvestCount; i ++) {
            AnimationsManager.Instance.QueueFunction(SpawnHarvestParticle);
        }
        AnimationsManager.Instance.QueueAnimation(new PauseAnimator(HarvestedIngredient.DURATION));
    }

    private void SpawnHarvestParticle() {
        GameObject harvest = Instantiate(PrefabContainer.Instance.HarvestParticle);
        harvest.GetComponent<SpriteRenderer>().sprite = PrefabContainer.Instance.ingredientToSprite[type];
        harvest.transform.position = transform.position;
    }

    private void CheckCapture(Monster mover) {
        LevelGrid level = LevelGrid.Instance;
        List<Monster> capturers = level.GetTilesInRange(Tile, CAPTURE_SIZE, true)
            .ConvertAll((Vector2Int tile) => { return level.GetMonster(tile); })
            .FindAll((Monster monster) => monster != null);

        //if(capturers.Count == 0 && !Contested) {
        //    // players retain control when they leave the capture area
        //    return;
        //}

        bool nowContested = false;
        Team newController = null;
        foreach(Monster monster in capturers) {
            if(newController == null) {
                newController = monster.Controller;
            }
            else if(monster.Controller != newController) {
                // both have control when contested
                nowContested = true;
                break;
            }
        }

        if(nowContested) {
            // keep the same controller when contested to avoid bugs
            newController = Controller;
        }

        if(newController != Controller || nowContested != Contested) {
            Contested = nowContested;
            Controller = newController;

            Color color = Color.clear;
            if(Contested) {
                color = (Controller.TeamColor + GameManager.Instance.OpponentOf(Controller).TeamColor) / 2f;
            }
            else if(Controller != null) {
                color = Controller.TeamColor;
            }

            AnimationsManager.Instance.QueueFunction(() => { LevelHighlighter.Instance.UpdateCapture(this); });
            AnimationsManager.Instance.QueueFunction(() => { SetOutlineColor(color); });
            AnimationsManager.Instance.QueueAnimation(new VFXAnimator(captureVisual, color == Color.clear ? Color.white : color));
        }
    }
}
