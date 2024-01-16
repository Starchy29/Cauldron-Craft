using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEffect
{
    private List<Vector2Int> affectedTiles;
    private Team controller;

    private MonsterTrigger OnStep; // effect that triggers when an enemy passes over this tile
    private MonsterTrigger OnLand; // effect that triggers when an enemy finishes their movement on this tile
    
    public int MovementTax { get; private set; }
    public StatusEffect? AppliedStatus { get; private set; }

    public TileEffect(List<Vector2Int> affectedTiles, Team controller) {
        this.affectedTiles = affectedTiles;
        this.controller = controller;

        LevelGrid level = LevelGrid.Instance;
        foreach(Vector2Int tile in affectedTiles) {
            level.GetTile(tile).CurrentEffect = this;
        }
    }
}
