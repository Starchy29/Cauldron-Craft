using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TravelTax {
    public bool taxesPlayer;
    public bool taxesEnemy;
    public int cost;
}

public class WorldTile
{
    private int baseTravelCost;
    private int playerTravelCost;
    private int enemyTravelCost;
    private List<TravelTax> taxes;

    public bool Walkable { get; private set; }
    public bool BlocksVision { get; private set; }

    public WorldTile(bool walkable, bool blockVision, int travelCost) {
        Walkable = walkable;
        BlocksVision = blockVision;
        baseTravelCost = travelCost;
        taxes = new List<TravelTax>();
        UpdateTravelCosts();
    }

    public int GetTravelCost(bool onPlayerTeam) {
        return onPlayerTeam ? playerTravelCost : enemyTravelCost;
    }

    // public virtual void OnTileEntered(Monster arrival) { } jk this should be an event

    private void UpdateTravelCosts() {
        playerTravelCost = baseTravelCost;
        enemyTravelCost = baseTravelCost;
        foreach(TravelTax tax in taxes) {
            if(tax.taxesPlayer) {
                playerTravelCost += tax.cost;
            }
            if(tax.taxesEnemy) {
                enemyTravelCost += tax.cost;
            }
        }
    }
}
