using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTile
{
    private int baseTravelCost;

    public bool Walkable { get; private set; }
    public bool IsWall { get; private set; }

    public WorldTile(bool walkable, bool blockVision, int travelCost) {
        Walkable = walkable;
        IsWall = blockVision;
        baseTravelCost = travelCost;
    }

    public int GetTravelCost(Monster monster) {
        return 1;
    }

    // public virtual void OnTileEntered(Monster arrival) { } jk this should be an event
    // OnTileLanded
}
