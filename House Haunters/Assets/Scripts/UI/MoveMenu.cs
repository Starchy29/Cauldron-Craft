using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoveMenu : MonoBehaviour
{
    [SerializeField] private GameObject MoveButtonPrefab;
    [SerializeField] private GameObject MonsterInfoAnchor;
    [SerializeField] private TextMeshPro HealthMarker;
    [SerializeField] private SpriteRenderer Heart;
    [SerializeField] private GameObject StatusZone;
    [SerializeField] private GameObject StatusIconPrefab;
    [SerializeField] private StatusTooltip statusTooltip;

    private MoveButton[] buttons;
    private int numButtons;
    private float buttonHeight;
    private const float BUTTON_GAP = 0.2f;
    private const int MAX_MOVES = 3;

    private List<StatusIcon> activeStatusIcons = new List<StatusIcon>();
    private Dictionary<StatusEffect, StatusIcon> normalStatusIcons;

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

        // set up monster health
        HealthMarker.text = monster.Health + "/" + monster.Stats.Health;
        float healthPercent = (float)monster.Health / monster.Stats.Health;
        Heart.color = monster.Controller.TeamColor; //healthPercent * new Color(0f, 0.6f, 0f) + (1f - healthPercent) * new Color(0.5f, 0.0f, 0.0f);
        //if(healthPercent <= 0.1f) {
        //    Heart.color = new Color(0.5f, 0.0f, 0.0f);
        //}
        //else if(healthPercent <= 0.25f) {
        //    Heart.color = new Color(0.8f, 0.0f, 0.0f);
        //}
        //else if(healthPercent <= 0.5f) {
        //    Heart.color = new Color(0.8f, 0.8f, 0.2f);
        //}
        //else if(healthPercent < 1.0f) {
        //    Heart.color = new Color(0.3f, 0.8f, 0.2f);
        //}

        // find which status icons to show
        foreach(StatusIcon oldStatus in activeStatusIcons) {
            oldStatus.gameObject.SetActive(false);
        }
        activeStatusIcons.Clear();

        foreach(StatusEffect effect in Enum.GetValues(typeof(StatusEffect))) {
            if(monster.HasStatus(effect)) {
                activeStatusIcons.Add(normalStatusIcons[effect]);
                normalStatusIcons[effect].duration = 0;
            }
        }

        foreach(StatusAilment ailment in monster.Statuses) {
            if(ailment.duration > normalStatusIcons[ailment.effect].duration) {
                normalStatusIcons[ailment.effect].duration = ailment.duration;
            }
        }

        // place status icons
        int rows = activeStatusIcons.Count > 3 ? 2 : 1;
        int cols = Mathf.CeilToInt(activeStatusIcons.Count / (float)rows);
        Vector2 zoneSize = StatusZone.transform.lossyScale;
        const float GAP_PERCENT = 0.1f;
        float iconWidth = (zoneSize.y - GAP_PERCENT) / 2f;
        float gapWidth = zoneSize.y * GAP_PERCENT;
        float stepWidth = iconWidth + gapWidth;
        Vector3 startPos = new Vector3(-0.5f * stepWidth * (cols - 1), 0.5f * stepWidth * (rows - 1), 0f);

        for(int i = 0; i < activeStatusIcons.Count; i++) {
            int col = i % cols;
            int row = i / cols;

            GameObject statusIcon = activeStatusIcons[i].gameObject;
            statusIcon.SetActive(true);
            statusIcon.transform.position = StatusZone.transform.position + startPos + new Vector3(col * stepWidth, row * -stepWidth, 0);
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
            { StatusEffect.Power, "Powered" },
            { StatusEffect.Swift, "Swift" },
            { StatusEffect.Poison, "Poisoned" },
            { StatusEffect.Fear, "Fearful" },
            { StatusEffect.Slowness, "Slowed" },
            { StatusEffect.Haunt, "Haunted" },
            { StatusEffect.Sturdy, "Sturdy" },
            { StatusEffect.Cursed, "Cursed" }
        };

        Dictionary<StatusEffect, string> descriptions = new Dictionary<StatusEffect, string>() {
            { StatusEffect.Power, "Deal 1.5x damage." },
            { StatusEffect.Swift, "Move up to one tile further." },
            { StatusEffect.Poison, "Take 5 damage at the end of every turn." },
            { StatusEffect.Fear, "Deal half damage." },
            { StatusEffect.Slowness, "Movement is reduced by 2 tiles." },
            { StatusEffect.Haunt, "Receive 1.5x damage." },
            { StatusEffect.Sturdy, "Receive half damage." },
            { StatusEffect.Cursed, "Enemies that attack this take 4 damage." }
        };

        PrefabContainer prefabs = PrefabContainer.Instance;
        normalStatusIcons = new Dictionary<StatusEffect, StatusIcon>();
        foreach(StatusEffect effect in Enum.GetValues(typeof(StatusEffect))) {
            normalStatusIcons[effect] = Instantiate(StatusIconPrefab).GetComponent<StatusIcon>().SetData(names[effect], descriptions[effect], prefabs.statusToSprite[effect]);
            normalStatusIcons[effect].transform.SetParent(transform, false);
            normalStatusIcons[effect].gameObject.SetActive(false);
        }
    }
}
