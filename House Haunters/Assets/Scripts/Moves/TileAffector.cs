using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAffector
{
    private GameObject visual;
    private Vector2Int tile;
    public Team Controller { get; private set; }
    public TileEffect Effect { get; private set; }
    public int TurnsLeft { get; private set; }

    public int MovementTax { get { return Effect.movementTax; } }
    public bool StopsMovement { get { return Effect.stopsMovement; } }
    public StatusEffect? AppliedStatus { get { return Effect.appliedStatus; } }
    public MonsterTrigger LandEffect { get { return Effect.landEffect; } }

    public static void ApplyTileEffect(Team controller, TileEffect effect, Vector2Int tile) {
        LevelGrid.Instance.SetTileAffect(tile, new TileAffector(controller, effect, tile));
    }

    // create a tile effect in the level
    private TileAffector(Team controller, TileEffect effect, Vector2Int tile) {
        Controller = controller;
        TurnsLeft = effect.duration;
        this.tile = tile;
        Effect = effect;
        visual = GameObject.Instantiate(effect.prefab);
        visual.transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)tile);

        controller.OnTurnStart += DecreaseDuration;
    }

    private void DecreaseDuration() {
        TurnsLeft--;
        if(TurnsLeft <= 0) {
            Finish();
        }
    }

    public void Finish() {
        AnimationsManager.Instance.QueueAnimation(new ZoneDestructionAnimator(visual));
        Controller.OnTurnEnd -= DecreaseDuration;
        LevelGrid.Instance.SetTileAffect(tile, null);
    }
}
