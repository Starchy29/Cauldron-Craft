using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoveButton : ControlledButton
{
    [SerializeField] private TextMeshPro nameLabel;
    [SerializeField] private TextMeshPro cooldown;

    public List<Vector2Int> CoveredArea { get; private set; }

    public void SetMove(Monster user, int moveSlot) {
        Move move = user.Stats.Moves[moveSlot];
        CoveredArea = move.GetCoveredArea(user);
        nameLabel.text = move.Name;
        cooldown.text = "" + user.Cooldowns[moveSlot];
    }
}
