using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoveMenu : MonoBehaviour
{
    [SerializeField] private GameObject MoveButtonPrefab;
    [SerializeField] private GameObject MonsterInfoAnchor;
    [SerializeField] private TextMeshPro HealthMarker;
    [SerializeField] private SpriteRenderer Heart;

    private MoveButton[] buttons;
    private int numButtons;
    private float buttonHeight;
    private const float BUTTON_GAP = 0.2f;

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

        // set up monster info
        MonsterInfoAnchor.transform.localPosition = new Vector3(0, -(buttonSpan - buttonHeight) / 2f, 0);
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
    }
}
