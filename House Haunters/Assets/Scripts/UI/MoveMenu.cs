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

    private MoveButton[] buttons;
    private int numButtons;
    private float buttonHeight;
    private const float BUTTON_GAP = 0.2f;

    private List<GameObject> activeStatusIcons = new List<GameObject>();
    private Dictionary<StatusEffect, StatusIcon> normalIcons;

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
        transform.localPosition = new Vector3((monster.Controller.OnLeft ? -1 : 1) * Mathf.Abs(transform.localPosition.x), transform.localPosition.y, transform.localPosition.z); // assumes this is a child of the camera
        Move[] moves = monster.Stats.Moves;

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
        foreach(GameObject oldStatus in activeStatusIcons) {
            oldStatus.SetActive(false);
        }
        activeStatusIcons.Clear();

        foreach(StatusEffect effect in Enum.GetValues(typeof(StatusEffect))) {
            if(monster.HasStatus(effect)) {
                activeStatusIcons.Add(normalIcons[effect].gameObject);
                normalIcons[effect].duration = 0; // 0 indicates the status is a result of the terrain the monster is standing on
            }
        }

        foreach(StatusAilment ailment in monster.Statuses) {
            foreach(StatusEffect effect in ailment.effects) {
                if(ailment.duration > normalIcons[effect].duration) {
                    normalIcons[effect].duration = ailment.duration;
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

            GameObject statusIcon = activeStatusIcons[i];
            statusIcon.SetActive(true);
            statusIcon.transform.localPosition = localTopLeft + new Vector3(x * (iconWidth + gapWidth), y * -(iconWidth + gapWidth), 0);
            statusIcon.transform.localScale = new Vector3(iconWidth, iconWidth, 1);
        }
    }

    private void Update() {
        Vector2 mousePos = InputManager.Instance.GetMousePosition();
        foreach(GameObject statusIcon in activeStatusIcons) {
            if(Global.GetObjectArea(statusIcon).Contains(mousePos)) {
                return;
            }
        }
    }

    private void SetUpStatusIcons() {
        Dictionary<StatusEffect, string> names = new Dictionary<StatusEffect, string>() {
            { StatusEffect.Regeneration, "Regenerating" },
            { StatusEffect.Strength, "Strenghtned" },
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
            { StatusEffect.Swiftness, "Move up to one tile further" },
            { StatusEffect.Energy, "Gain an additional action every turn." },
            { StatusEffect.Poison, "Take 2 damage at the end of every turn." },
            { StatusEffect.Fear, "Deal half the normal amount of damage." },
            { StatusEffect.Slowness, "Movement is reduced by 1 tile." },
            { StatusEffect.Drowsiness, "Lose 1 action every turn." },
            { StatusEffect.Haunted, "Receive 1.5x damage." }
        };

        PrefabContainer prefabs = PrefabContainer.Instance;
        normalIcons = new Dictionary<StatusEffect, StatusIcon>();
        foreach(StatusEffect effect in Enum.GetValues(typeof(StatusEffect))) {
            normalIcons[effect] = Instantiate(StatusIconPrefab).GetComponent<StatusIcon>().SetData(names[effect], descriptions[effect], prefabs.statusToSprite[effect]);
            normalIcons[effect].transform.SetParent(StatusZone.transform, false);
            normalIcons[effect].gameObject.SetActive(false);
        }
    }
}
