using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// defines the method by which a move selects its choices
public interface ISelector
{
    public int Range { get; }

    // returns a list of grouped tiles that can be selected
    public abstract List<List<Vector2Int>> GetSelectionGroups(Monster user);
}
