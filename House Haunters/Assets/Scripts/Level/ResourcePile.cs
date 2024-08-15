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
    [SerializeField] private GameObject productionIndicator;
    [SerializeField] private CaptureVFX captureVisual;
    
    public Ingredient Type { get { return type; } }

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

        SetOutlineColor(Controller.TeamColor);
        productionIndicator.SetActive(true);
        StartCoroutine(HideResourceType());
    }

    public bool IsInCaptureRange(Vector2Int tile) {
        return Mathf.Abs(tile.x - Tile.x) <= 1 && Mathf.Abs(tile.y - Tile.y) <= 1;
    }

    private void GrantResource(Team turnEnder, Team turnStarter) {
        if(Controller == GameManager.Instance.Defender && turnStarter == Controller) {
            turnStarter.Resources[type]++;
            AnimationsManager.Instance.QueueAnimation(new FunctionAnimator(SpawnHarvestParticle));
        }
    }

    private void SpawnHarvestParticle() {
        GameObject harvest = Instantiate(PrefabContainer.Instance.HarvestParticle);
        harvest.GetComponent<SpriteRenderer>().sprite = PrefabContainer.Instance.ingredientToSprite[type];
        harvest.transform.position = transform.position;
    }

    private void CheckCapture(Monster mover) {
        if(Controller == GameManager.Instance.Attacker) {
            return;
        }

        LevelGrid level = LevelGrid.Instance;
        List<Team> adjacentTeams = level.GetTilesInRange(Tile, 1, true)
            .Map((Vector2Int tile) => { 
                Monster monster = level.GetMonster(tile);
                return monster == null ? null : monster.Controller; 
            });

        if(adjacentTeams.Contains(GameManager.Instance.Attacker) && !adjacentTeams.Contains(GameManager.Instance.Defender)) {
            Controller = GameManager.Instance.Attacker;
            AnimationsManager.Instance.QueueAnimation(new FunctionAnimator(() => { SetOutlineColor(Controller == null ? Color.clear : Controller.TeamColor); }));
            AnimationsManager.Instance.QueueAnimation(new VFXAnimator(captureVisual, Controller == null ? Color.white : Controller.TeamColor));
        }
    }

    private IEnumerator HideResourceType() {
        yield return new WaitForSeconds(4.0f);
        productionIndicator.SetActive(false);
    }
}
