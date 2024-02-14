using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveButton : ControlledButton
{
    public List<Vector2Int> CoveredArea { get; private set; }

    void Start() {
        
    }

    public void SetMove(Monster user, Move move) {
        CoveredArea = move.GetCoveredArea(user);
    }
}
