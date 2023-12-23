using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntity : MonoBehaviour
{
    public Vector2Int Tile { get; set; }

    void OnDestroy() {
        LevelGrid.Instance.ClearTile(Tile);
    }
}
