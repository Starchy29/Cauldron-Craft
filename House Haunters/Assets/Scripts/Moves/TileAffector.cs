using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAffector
{
    private Vector2Int tile;
    public Team Controller { get; private set; }

    private GameObject visual;
    public int Duration { get; private set; }

    public StatusEffect? AppliedStatus { get; private set; }
    public int MovementTax { get; private set; }
    private MonsterTrigger landEffect;
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

        TileAffector createdEffect = new TileAffector(visual, blueprint.Duration, blueprint.AppliedStatus, blueprint.MovementTax, blueprint.landEffect, blueprint.destroyOnUse, blueprint.StopsMovement);
        createdEffect.tile = tile;
        createdEffect.Controller = controller;
        controller.OnTurnStart += createdEffect.DecreaseDuration;
        LevelGrid.Instance.SetTileAffect(tile, createdEffect);
    }

    public static TileAffector CreateBlueprint(GameObject prefab, int duration, StatusEffect? appliedStatus, int movementTax, MonsterTrigger landEffect, bool destroyOnUse = false, bool stopsMovement = false) {
        return new TileAffector(prefab, duration, appliedStatus, movementTax, landEffect, destroyOnUse, stopsMovement);
    }

    private TileAffector(GameObject visual, int duration, StatusEffect? appliedStatus, int movementTax, MonsterTrigger landEffect, bool destroyOnUse, bool stopsMovement) {
        this.visual = visual;
        Duration = duration;
        AppliedStatus = appliedStatus;
        MovementTax = movementTax;
        this.landEffect = landEffect;
        StopsMovement = stopsMovement;
        this.destroyOnUse = destroyOnUse;
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

    private void Finish() {
        AnimationsManager.Instance.QueueAnimation(new DestructionAnimator(visual));
        Controller.OnTurnStart -= DecreaseDuration;
        LevelGrid.Instance.SetTileAffect(tile, null);
    }
}
