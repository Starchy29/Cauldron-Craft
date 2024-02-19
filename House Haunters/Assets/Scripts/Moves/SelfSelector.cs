using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfSelector : ISelector
{
    public List<List<Vector2Int>> GetSelectionGroups(Monster user) {
        return new List<List<Vector2Int>>() { new List<Vector2Int>() { user.Tile } };
    }
}
