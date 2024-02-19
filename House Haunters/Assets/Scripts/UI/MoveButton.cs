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
        cooldown.text = user.Cooldowns[moveSlot] > 0 ? ""+user.Cooldowns[moveSlot] : "";
        // move type icon

        // open info menu

        // description
        // current cooldown / max cooldown

        if(move is Attack) {
            // damage
        }
        else if(move is ShieldMove) {
            // strength
            // duration
            // fragile
            // blocks status
        }
        // status
            // effects
            // duration
        // zone
    }
}
