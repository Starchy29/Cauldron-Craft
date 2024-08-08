using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoveButton : AutoButton
{
    [SerializeField] private TextMeshPro nameLabel;
    [SerializeField] private TextMeshPro cooldown;
    [SerializeField] private SpriteRenderer typeIcon;
    [SerializeField] private TextMeshPro description;
    [SerializeField] private GameObject hourglass;
    [SerializeField] private TextMeshPro maxCooldownLabel;

    private int moveSlot;
    public List<Vector2Int> CoveredArea { get; private set; }
    public List<Vector2Int> CoveredAreaAfterWalk { get; private set; }

    void Awake() {
        OnHover = HighlightArea;
        OnMouseLeave = HideHighlight;
        OnClick = () => { MenuManager.Instance.SelectMove(moveSlot); };
        CoveredAreaAfterWalk = new List<Vector2Int>();
    }

    public void SetMove(Monster user, int moveSlot) {
        this.moveSlot = moveSlot;
        Move move = user.Stats.Moves[moveSlot];
        
        bool showFiltered = move.TargetType == Move.Targets.StandableSpot;
        CoveredArea = move.GetOptions(user, showFiltered, false)
            .Collapse((List<Vector2Int> cur, List<Vector2Int> next) => { cur.AddRange(next); return cur; });
        
        CoveredAreaAfterWalk.Clear();
        if(moveSlot != MonsterType.WALK_INDEX && move.Range > 0 && 
            (user.Controller != GameManager.Instance.CurrentTurn || user.CanUse(MonsterType.WALK_INDEX) && user.MovesLeft > 1 || user.MovesLeft == 0)
        ) {
            foreach(KeyValuePair<Vector2Int, List<List<Vector2Int>>> option in user.GetMoveOptionsAfterWalk(moveSlot, true)) {
                CoveredAreaAfterWalk.AddRange(option.Value.Collapse((List<Vector2Int> cur, List<Vector2Int> next) => { cur.AddRange(next); return cur; }));
            }
        }

        nameLabel.text = move.Name;
        cooldown.text = user.Cooldowns[moveSlot] > 0 ? "" + user.Cooldowns[moveSlot] : "";
        hourglass.SetActive(user.Cooldowns[moveSlot] > 0);

        typeIcon.sprite = PrefabContainer.Instance.moveTypeToSprite[move.Type];
        //SetBackColor(moveTypeToColor[move.Type]);

        // open info menu

        // description
        description.text = move.Description;
        maxCooldownLabel.text = "" + move.Cooldown;

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
        LevelGrid.Instance.ColorTiles(CoveredAreaAfterWalk, TileHighlighter.State.WeakHighlight);
    }

    private void HideHighlight() {
        LevelGrid.Instance.ColorTiles(null, TileHighlighter.State.Highlighted);
        LevelGrid.Instance.ColorTiles(null, TileHighlighter.State.WeakHighlight);
    }
}
