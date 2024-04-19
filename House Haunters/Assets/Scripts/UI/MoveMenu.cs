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
    private List<GameObject> statusIcons = new List<GameObject>();

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
        gameObject.SetActive(false);
    }

    public void Open(Monster monster, Team player) {
        gameObject.SetActive(true);
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
            buttons[i].disabled = monster.Controller != player || !monster.CanUse(i);
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

        // set up statuses
        foreach(GameObject oldStatus in statusIcons) {
            Destroy(oldStatus);
        }
        statusIcons.Clear();

        Dictionary<StatusEffect, int> statusDurations = new Dictionary<StatusEffect, int>();
        foreach(StatusEffect effect in Enum.GetValues(typeof(StatusEffect))) {
            if(monster.HasStatus(effect)) {
                statusDurations[effect] = 0; // 0 indicates the status is a result of the terrain the monster is standing on
            }
        }
        StatusEffect[] statusList = statusDurations.Keys.ToArray();

        foreach(StatusAilment ailment in monster.Statuses) {
            foreach(StatusEffect effect in ailment.effects) {
                if(ailment.duration > statusDurations[effect]) {
                    statusDurations[effect] = ailment.duration;
                }
            }
        }

        int statusDims = 1;
        while(statusDurations.Count > statusDims * statusDims) {
            statusDims++; // keep the statuses in a square grid
        }
        
        float zoneWidth = StatusZone.transform.lossyScale.x;
        const float GAP_PERCENT = 0.2f;
        float iconWidth = zoneWidth * ((1f - GAP_PERCENT * (statusDims - 1)) / statusDims);
        float gapWidth = zoneWidth * GAP_PERCENT;
        Vector3 localTopLeft = new Vector3(-zoneWidth / 2f + iconWidth / 2f, zoneWidth / 2f - iconWidth / 2f, 0);

        for(int i = 0; i < statusDurations.Count; i++) {
            int x = i % statusDims;
            int y = i / statusDims;
            GameObject statusIcon = Instantiate(StatusIconPrefab);
            statusIcons.Add(statusIcon);
            statusIcon.transform.SetParent(StatusZone.transform, false);
            statusIcon.transform.localPosition = localTopLeft + new Vector3(x * (iconWidth + gapWidth), y * -(iconWidth + gapWidth), 0);
            statusIcon.transform.localScale = new Vector3(iconWidth, iconWidth, 1);
            statusIcon.GetComponent<SpriteRenderer>().sprite = PrefabContainer.Instance.statusToSprite[statusList[i]];
        }
    }
}
