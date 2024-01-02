using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Move
{
    public Selector Selection { get; private set; }
    public int Cost { get; private set; }

    public Move(int cost, Selector selection) {
        Cost = cost;
        Selection = selection;
    }

    public abstract void Use(Monster user, List<Vector2Int> tiles);
}
