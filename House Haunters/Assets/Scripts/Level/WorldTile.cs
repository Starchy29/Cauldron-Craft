using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct WorldTile
{
    private int baseTravelCost;

    public bool Walkable { get; private set; }
    public bool IsWall { get; private set; }

    public TileAffector CurrentEffect { get; set; }

    public WorldTile(bool walkable, bool blockVision, int travelCost) {
        Walkable = walkable;
        IsWall = blockVision;
        baseTravelCost = travelCost;
        CurrentEffect = null;
    }

    public int GetTravelCost(Monster monster) {
        return baseTravelCost + (CurrentEffect == null || CurrentEffect.Controller == monster.Controller ? 0 : CurrentEffect.MovementTax);
    }
}
