using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoveButton : AutoButton
{
    [SerializeField] private TextMeshPro nameLabel;
    [SerializeField] private TextMeshPro cooldown;

    private int moveSlot;
    public List<Vector2Int> CoveredArea { get; private set; }

    void Awake() {
        OnHover = HighlightArea;
        OnMouseLeave = HideHighlight;
        OnClick = () => { MenuManager.Instance.SelectMove(moveSlot); };
    }

    public void SetMove(Monster user, int moveSlot) {
        this.moveSlot = moveSlot;
        Move move = user.Stats.Moves[moveSlot];
        
        CoveredArea = new List<Vector2Int>();
        bool showFiltered = move.TargetType == Move.Targets.Traversable || move.TargetType == Move.Targets.StandableSpot;
        List<List<Vector2Int>> groups = move.GetOptions(user, showFiltered, false);
        foreach(List<Vector2Int> group in groups) {
            CoveredArea.AddRange(group);
        }

        nameLabel.text = move.Name;
        cooldown.text = user.Cooldowns[moveSlot] > 0 ? "" + user.Cooldowns[moveSlot] : "";
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

    private void HighlightArea() {
        LevelGrid.Instance.ColorTiles(CoveredArea, TileHighlighter.State.Highlighted);
    }

    private void HideHighlight() {
        LevelGrid.Instance.ColorTiles(null, TileHighlighter.State.Highlighted);
    }
}
