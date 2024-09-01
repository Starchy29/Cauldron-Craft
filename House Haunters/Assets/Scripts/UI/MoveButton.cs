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
    [SerializeField] private TextMeshPro rangeLabel;

    private int moveSlot;
    public List<Selection> TargetOptions { get; private set; }
    public List<Vector2Int> CoveredArea { get; private set; }
    public List<Vector2Int> CoveredAreaAfterWalk { get; private set; }

    private static Dictionary<MoveType, Color> moveColors = new Dictionary<MoveType, Color> {
        { MoveType.Attack, Color.red },
        { MoveType.Movement, Color.green },
        { MoveType.Heal, new Color(0.9f, 0.1f, 0.6f) },
        { MoveType.Boost, Color.yellow },
        { MoveType.Disrupt, new Color(0.7f, 0f, 0.9f) },
        { MoveType.Decay, new Color(0f, 0.3f, 0.7f) },
        { MoveType.Terrain, new Color(0.2f, 0.6f, 0.4f) },
        { MoveType.Shift, new Color(0.9f, 0.4f, 0.1f) },
    };

    void Awake() {
        OnHover = HighlightArea;
        OnMouseLeave = HideHighlight;
        OnClick = () => { MenuManager.Instance.SelectMove(moveSlot, TargetOptions); };
        CoveredAreaAfterWalk = new List<Vector2Int>();
    }

    public void SetMove(Monster user, int moveSlot) {
        this.moveSlot = moveSlot;
        Move move = user.Stats.Moves[moveSlot];
        
        bool showFiltered = move.TargetType == Move.Targets.StandableSpot;
        List<Selection> targetGroups = move.GetOptions(user, false);
        TargetOptions = targetGroups.FindAll((Selection targets) => targets.Filtered.Count > 0);
        CoveredArea = targetGroups.ConvertAll((Selection targets) => new List<Vector2Int>(showFiltered ? targets.Filtered : targets.Unfiltered))
            .Collapse((List<Vector2Int> cur, List<Vector2Int> next) => { cur.AddRange(next); return cur; });
        
        CoveredAreaAfterWalk.Clear();
        if(moveSlot != MonsterType.WALK_INDEX && move.Range > 0) {
            foreach(KeyValuePair<Vector2Int, List<Selection>> option in user.GetMoveOptionsAfterWalk(moveSlot, true)) {
                CoveredAreaAfterWalk.AddRange(option.Value
                    .ConvertAll((Selection targets) => new List<Vector2Int>(targets.Unfiltered))
                    .Collapse((List<Vector2Int> cur, List<Vector2Int> next) => { cur.AddRange(next); return cur; })
                );
            }
        }

        nameLabel.text = move.Name;
        cooldown.text = user.Cooldowns[moveSlot] <= 1 ? "" : "" + (user.Cooldowns[moveSlot] - 1);
        hourglass.SetActive(user.Cooldowns[moveSlot] > 1);

        typeIcon.sprite = PrefabContainer.Instance.moveTypeToSprite[move.Type];

        baseColor = new Color(0.7f, 0.7f, 0.7f) + 0.3f * moveColors[move.Type];
        disabledColor = new Color(0.15f, 0.15f, 0.15f) + 0.15f * moveColors[move.Type];
        hoveredColor = new Color(0.95f, 0.95f, 0.95f);

        // description
        description.text = move.Description;
        maxCooldownLabel.text = "" + (move.Cooldown - 1);
        rangeLabel.text = move.Range > 0 ? "" + move.Range : "" + user.Stats.Speed;
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
