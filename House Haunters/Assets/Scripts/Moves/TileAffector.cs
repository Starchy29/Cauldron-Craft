using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAffector
{
    private Vector2Int tile;
    public Team Controller { get; private set; }

    private GameObject visual;
    public int Duration { get; private set; }

    public bool HasNegativeEffect { get { return landEffect != null || endTurnEffect != null; } }
    public int MovementTax { get; private set; }
    private MonsterTrigger landEffect;
    private MonsterTrigger endTurnEffect;
    public bool StopsMovement { get; private set; }
    private bool destroyOnUse;

    public static void ApplyEffect(TileAffector blueprint, Team controller, Vector2Int tile) {
        TileAffector previousEffect = LevelGrid.Instance.GetTile(tile).CurrentEffect;
        if(previousEffect != null) {
            previousEffect.Finish();
        }

        GameObject visual = GameObject.Instantiate(blueprint.visual);
        visual.transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)tile);
        visual.SetActive(false);
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(visual, true));

        TileAffector createdEffect = new TileAffector(visual, blueprint.Duration, blueprint.MovementTax, blueprint.landEffect, blueprint.destroyOnUse, blueprint.StopsMovement, blueprint.endTurnEffect);
        createdEffect.tile = tile;
        createdEffect.Controller = controller;
        controller.OnTurnStart += createdEffect.DecreaseDuration;
        GameManager.Instance.OpponentOf(controller).OnTurnEnd += createdEffect.ApplyTurnEndEffect;
        LevelGrid.Instance.SetTileAffect(tile, createdEffect);
    }

    public static TileAffector CreateBlueprint(GameObject prefab, int duration, int movementTax, MonsterTrigger landEffect, bool destroyOnUse = false, bool stopsMovement = false, MonsterTrigger endTurnEffect = null) {
        return new TileAffector(prefab, duration, movementTax, landEffect, destroyOnUse, stopsMovement, endTurnEffect);
    }

    private TileAffector(GameObject visual, int duration, int movementTax, MonsterTrigger landEffect, bool destroyOnUse, bool stopsMovement, MonsterTrigger endTurnEffect) {
        this.visual = visual;
        Duration = duration;
        MovementTax = movementTax;
        this.landEffect = landEffect;
        StopsMovement = stopsMovement;
        this.destroyOnUse = destroyOnUse;
        this.endTurnEffect = endTurnEffect;
    }

    public void LandMonster(Monster lander) {
        if(landEffect != null) {
            landEffect(lander);
        }

        if(destroyOnUse) {
            Finish();
        }
    }

    private void DecreaseDuration() {
        Duration--;
        if(Duration <= 0) {
            Finish();
        }
    }

    private void ApplyTurnEndEffect() {
        Monster occupant = LevelGrid.Instance.GetMonster(tile);
        if(endTurnEffect != null && occupant != null && occupant.Controller != Controller) {
            AnimationsManager.Instance.QueueAnimation(new CameraAnimator(occupant.transform.position));
            endTurnEffect(occupant);
        }
    }

    private void Finish() {
        AnimationsManager.Instance.QueueAnimation(new DestructionAnimator(visual));
        Controller.OnTurnStart -= DecreaseDuration;
        LevelGrid.Instance.SetTileAffect(tile, null);
        GameManager.Instance.OpponentOf(Controller).OnTurnEnd -= ApplyTurnEndEffect;
    }
}
