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
    [SerializeField] private CaptureVFX captureVisual;
    
    public Ingredient Type { get { return type; } }

    private bool contested; // both teams present

    protected override void Start() {
        base.Start();
        LevelGrid.Instance.OnMonsterMove += CheckCapture;
        GameManager.Instance.OnMonsterDefeated += CheckCapture;
        GameManager.Instance.OnTurnChange += GrantResource;
        GameManager.Instance.AllResources.Add(this);

        // place particles on the ground around this tile
        List<Vector2Int> openAdjTiles = LevelGrid.Instance.GetTilesInRange(Tile, 1, true).Filter((Vector2Int tile) => { return tile != this.Tile && LevelGrid.Instance.GetTile(tile).Walkable; });
        foreach(Vector2Int tile in openAdjTiles) {
            GameObject floorCover = Instantiate(floorCoverPrefab);
            floorCover.transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)tile);
            floorCover.transform.position = transform.position + 0.8f * (floorCover.transform.position - transform.position);

            floorCover.transform.localScale = new Vector3(Random.value < 0.5f ? 1f : 1f, Random.value < 0.5f ? 1f : 1f, 1f);
            floorCover.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 4) * 90f);
        }
    }

    public bool IsInCaptureRange(Vector2Int tile) {
        return Mathf.Abs(tile.x - Tile.x) <= 1 && Mathf.Abs(tile.y - Tile.y) <= 1;
    }

    private void GrantResource(Team turnEnder, Team turnStarter) {
        if(contested || turnStarter == Controller) {
            turnStarter.Resources[type] += 2;
            AnimationsManager.Instance.QueueFunction(SpawnHarvestParticle);
            AnimationsManager.Instance.QueueFunction(SpawnHarvestParticle);
        }
    }

    private void SpawnHarvestParticle() {
        GameObject harvest = Instantiate(PrefabContainer.Instance.HarvestParticle);
        harvest.GetComponent<SpriteRenderer>().sprite = PrefabContainer.Instance.ingredientToSprite[type];
        harvest.transform.position = transform.position;
    }

    private void CheckCapture(Monster mover) {
        LevelGrid level = LevelGrid.Instance;
        List<Monster> capturers = level.GetTilesInRange(Tile, 1, true)
            .Map((Vector2Int tile) => { return level.GetMonster(tile); })
            .Filter((Monster monster) => monster != null);

        if(capturers.Count == 0 && !contested) {
            // players retain control when they leave the capture area
            return;
        }

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

        if(newController != Controller || nowContested != contested) {
            contested = nowContested;
            Controller = newController;

            Color color = Color.clear;
            if(contested) {
                color = (Controller.TeamColor + GameManager.Instance.OpponentOf(Controller).TeamColor) / 2f;
            }
            else if(Controller != null) {
                color = Controller.TeamColor;
            }

            AnimationsManager.Instance.QueueFunction(() => { SetOutlineColor(color); });
            AnimationsManager.Instance.QueueAnimation(new VFXAnimator(captureVisual, color == Color.clear ? Color.white : color));
        }
    }
}
