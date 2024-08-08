using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class MoveMenu : MonoBehaviour
{
    [SerializeField] private GameObject MoveButtonPrefab;
    [SerializeField] private GameObject MonsterInfoAnchor;
    [SerializeField] private TextMeshPro HealthMarker;
    [SerializeField] private SpriteRenderer Heart;
    [SerializeField] private GameObject StatusZone;
    [SerializeField] private GameObject StatusIconPrefab;
    [SerializeField] private StatusTooltip statusTooltip;
    [SerializeField] private GameObject remainingMoveTracker;

    private SpriteRenderer[] movesLeftCircles;
    private MoveButton[] buttons;
    private int numButtons;
    private float buttonHeight;
    private const float BUTTON_GAP = 0.2f;
    private const int MAX_MOVES = 3;

    private List<StatusIcon> activeStatusIcons = new List<StatusIcon>();
    private Dictionary<StatusEffect, StatusIcon> normalStatusIcons;
    private Dictionary<UniqueStatuses, StatusIcon> uniqueStatusIcons;

    public GameObject Background { get; private set; }

    void Awake() {
        buttons = new MoveButton[8];
        for(int i = 0; i < buttons.Length; i++) {
            buttons[i] = Instantiate(MoveButtonPrefab).GetComponent<MoveButton>();
            buttons[i].transform.SetParent(transform);
            buttons[i].gameObject.SetActive(false);
        }

        Background = transform.GetChild(0).gameObject;
        buttonHeight = MoveButtonPrefab.transform.localScale.y;

        SetUpStatusIcons();

        movesLeftCircles = new SpriteRenderer[MAX_MOVES];
        for(int i = 0; i < MAX_MOVES; i++) {
            GameObject spawned = new GameObject();
            movesLeftCircles[i] = spawned.AddComponent<SpriteRenderer>();
            movesLeftCircles[i].sortingLayerName = "UI";
            spawned.transform.SetParent(remainingMoveTracker.transform);
            spawned.transform.localPosition = new Vector3((-1f + i) / remainingMoveTracker.transform.localScale.x, 0f, 0f);
            spawned.transform.localScale = new Vector3(0.7f / remainingMoveTracker.transform.localScale.x, 0.7f, 1f);
        }

        gameObject.SetActive(false);
    }

    public void Open(Monster monster, Team opener) {
        gameObject.SetActive(true);
        Move[] moves = monster.Stats.Moves;

        // place on the correct side of the screen
        transform.localPosition = new Vector3((monster.Controller.OnLeft ? -1 : 1) * Mathf.Abs(transform.localPosition.x), transform.localPosition.y, transform.localPosition.z); // assumes this is a child of the camera
        foreach(MoveButton button in buttons) {
            Vector3 pos = button.tooltip.transform.localPosition;
            pos.x = (monster.Controller.OnLeft ? 1 : -1) * Mathf.Abs(pos.x);
            button.tooltip.transform.localPosition = pos;
        }
        statusTooltip.transform.localPosition = new Vector3((monster.Controller.OnLeft ? 1 : -1) * Mathf.Abs(statusTooltip.transform.localPosition.x), statusTooltip.transform.localPosition.y, statusTooltip.transform.localPosition.z);

        // set up move buttoms
        numButtons = moves.Length;
        float buttonSpan = buttonHeight * (numButtons + 1) + BUTTON_GAP * numButtons;
        Background.transform.localScale = new Vector3(MoveButtonPrefab.transform.localScale.x + 0.5f, buttonSpan + 0.5f, 1);
        for(int i = 0; i < buttons.Length; i++) {
            if(i >= numButtons) {
                buttons[i].gameObject.SetActive(false);
                continue;
            }

            buttons[i].gameObject.SetActive(true);
            buttons[i].transform.localPosition = new Vector3(0, -(buttonSpan - buttonHeight) / 2f + (i + 1) * (buttonHeight + BUTTON_GAP), 0);
            buttons[i].SetMove(monster, i);
            buttons[i].Disabled = monster.Controller != opener || !monster.CanUse(i);
        }

        MonsterInfoAnchor.transform.localPosition = new Vector3(0, -(buttonSpan - buttonHeight) / 2f, 0);

        // set up remaining move count
        remainingMoveTracker.SetActive(monster.Controller == opener);
        if(monster.Controller == opener) {
            remainingMoveTracker.transform.localPosition = new Vector3(0f, Background.transform.localScale.y / 2f + 0.5f, 0f);
            for(int i = 0; i < MAX_MOVES; i++) {
                movesLeftCircles[i].sprite = i < monster.MovesLeft ? PrefabContainer.Instance.fullCircle : PrefabContainer.Instance.emptyCircle;
                movesLeftCircles[i].gameObject.SetActive(i < monster.MaxMoves);
            }
        }

        // set up monster health
        HealthMarker.text = monster.Health + "/" + monster.Stats.Health;
        float healthPercent = (float)monster.Health / monster.Stats.Health;
        Heart.color = new Color(0f, 0.6f, 0f);
        if(healthPercent <= 0.1f) {
            Heart.color = new Color(0.5f, 0.0f, 0.0f);
        }
        else if(healthPercent <= 0.25f) {
            Heart.color = new Color(0.8f, 0.0f, 0.0f);
        }
        else if(healthPercent <= 0.5f) {
            Heart.color = new Color(0.8f, 0.8f, 0.2f);
        }
        else if(healthPercent < 1.0f) {
            Heart.color = new Color(0.3f, 0.8f, 0.2f);
        }

        // set up shield

        // find which status icons to show
        foreach(StatusIcon oldStatus in activeStatusIcons) {
            oldStatus.gameObject.SetActive(false);
        }
        activeStatusIcons.Clear();

        foreach(UniqueStatus status in monster.UniqueStatuses) {
            activeStatusIcons.Add(uniqueStatusIcons[status.Type]);
            uniqueStatusIcons[status.Type].duration = status.Duration;
        }

        foreach(StatusEffect effect in Enum.GetValues(typeof(StatusEffect))) {
            if(monster.HasStatus(effect)) {
                activeStatusIcons.Add(normalStatusIcons[effect]);
                normalStatusIcons[effect].duration = 0; // 0 indicates the status is a result of the terrain the monster is standing on
            }
        }

        foreach(StatusAilment ailment in monster.Statuses) {
            foreach(StatusEffect effect in ailment.effects) {
                if(ailment.duration > normalStatusIcons[effect].duration) {
                    normalStatusIcons[effect].duration = ailment.duration;
                }
            }
        }

        // place status icons
        int statusDims = 1;
        while(activeStatusIcons.Count > statusDims * statusDims) {
            statusDims++; // keep the statuses in a square grid
        }
        
        float zoneWidth = StatusZone.transform.lossyScale.x;
        const float GAP_PERCENT = 0.2f;
        float iconWidth = zoneWidth * ((1f - GAP_PERCENT * (statusDims - 1)) / statusDims);
        float gapWidth = zoneWidth * GAP_PERCENT;
        Vector3 localTopLeft = new Vector3(-zoneWidth / 2f + iconWidth / 2f, zoneWidth / 2f - iconWidth / 2f, 0);

        for(int i = 0; i < activeStatusIcons.Count; i++) {
            int x = i % statusDims;
            int y = i / statusDims;

            GameObject statusIcon = activeStatusIcons[i].gameObject;
            statusIcon.SetActive(true);
            statusIcon.transform.localPosition = localTopLeft + new Vector3(x * (iconWidth + gapWidth), y * -(iconWidth + gapWidth), 0);
            statusIcon.transform.localScale = new Vector3(iconWidth, iconWidth, 1);
        }
    }

    private void Update() {
        Vector2 mousePos = InputManager.Instance.GetMousePosition();
        foreach(StatusIcon statusIcon in activeStatusIcons) {
            if(Global.GetObjectArea(statusIcon.gameObject).Contains(mousePos)) {
                statusTooltip.gameObject.SetActive(true);
                statusTooltip.NameLabel.text = statusIcon.statusName;
                statusTooltip.DescriptionLabel.text = statusIcon.description;
                statusTooltip.DurationLabel.text = statusIcon.duration == 0 ? "-" : "" + statusIcon.duration;
                return;
            }
        }

        statusTooltip.gameObject.SetActive(false);
    }

    private void SetUpStatusIcons() {
        Dictionary<StatusEffect, string> names = new Dictionary<StatusEffect, string>() {
            { StatusEffect.Regeneration, "Regenerating" },
            { StatusEffect.Strength, "Strengthened" },
            { StatusEffect.Swiftness, "Swift" },
            { StatusEffect.Energy, "Energized" },
            { StatusEffect.Poison, "Poisoned" },
            { StatusEffect.Fear, "Fearful" },
            { StatusEffect.Slowness, "Slowed" },
            { StatusEffect.Drowsiness, "Drowsy" },
            { StatusEffect.Haunted, "Haunted" }
        };

        Dictionary<StatusEffect, string> descriptions = new Dictionary<StatusEffect, string>() {
            { StatusEffect.Regeneration, "Heal 2 health at the end of every turn." },
            { StatusEffect.Strength, "Deal 1.5x damage." },
            { StatusEffect.Swiftness, "Move up to one tile further." },
            { StatusEffect.Energy, "Gain an additional action every turn." },
            { StatusEffect.Poison, "Take 2 damage at the end of every turn." },
            { StatusEffect.Fear, "Deal half the normal amount of damage." },
            { StatusEffect.Slowness, "Movement is reduced by 1 tile." },
            { StatusEffect.Drowsiness, "Lose 1 action every turn." },
            { StatusEffect.Haunted, "Receive 1.5x damage." }
        };

        PrefabContainer prefabs = PrefabContainer.Instance;
        normalStatusIcons = new Dictionary<StatusEffect, StatusIcon>();
        foreach(StatusEffect effect in Enum.GetValues(typeof(StatusEffect))) {
            normalStatusIcons[effect] = Instantiate(StatusIconPrefab).GetComponent<StatusIcon>().SetData(names[effect], descriptions[effect], prefabs.statusToSprite[effect]);
            normalStatusIcons[effect].transform.SetParent(StatusZone.transform, false);
            normalStatusIcons[effect].gameObject.SetActive(false);
        }

        uniqueStatusIcons = new Dictionary<UniqueStatuses, StatusIcon>();
        uniqueStatusIcons[UniqueStatuses.LeechSpore] = Instantiate(StatusIconPrefab).GetComponent<StatusIcon>()
            .SetData("Infected", "Drained for 2 life every turn.", prefabs.infectedIcon);
        uniqueStatusIcons[UniqueStatuses.Wither] = Instantiate(StatusIconPrefab).GetComponent<StatusIcon>()
            .SetData("Withering", "Take 4 damage at the end of every turn.", prefabs.witherIcon);
        uniqueStatusIcons[UniqueStatuses.Thorns] = Instantiate(StatusIconPrefab).GetComponent<StatusIcon>()
            .SetData("Thorny", "Deal 6 damage to enemies that melee attack this.", prefabs.thornsIcon);

        foreach (UniqueStatuses effect in Enum.GetValues(typeof(UniqueStatuses))) {
            uniqueStatusIcons[effect].transform.SetParent(StatusZone.transform, false);
            uniqueStatusIcons[effect].gameObject.SetActive(false);
        }
    }
}
