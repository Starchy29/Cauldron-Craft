using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTile : MonoBehaviour
{
    public bool Walkable { get; private set; }
    public bool BlocksVision { get; private set; }

    public virtual void OnTileEntered(Monster arrival) { }
}
