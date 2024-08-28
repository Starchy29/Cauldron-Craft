using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Selection
{
    public List<Vector2Int> Unfiltered { get; private set; }
    public List<Vector2Int> Filtered { get; private set; }

    public Selection(List<Vector2Int> unfiltered, List<Vector2Int> filtered) {
        Unfiltered = unfiltered;
        Filtered = filtered;
    }
}
