using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMenu : MonoBehaviour
{
    [SerializeField] private GameObject MoveButtonPrefab;

    private MoveButton[] buttons;
    private int numButtons;
    private float buttonHeight;
    private const float BUTTON_GAP = 0.2f;

    public GameObject Background { get; private set; }

    void Start() {
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

        numButtons = moves.Length;
        float buttonSpan = buttonHeight * numButtons + BUTTON_GAP * (numButtons - 1);
        Background.transform.localScale = new Vector3(MoveButtonPrefab.transform.localScale.x + 0.5f, buttonSpan + 0.5f, 1);
        for(int i = 0; i < buttons.Length; i++) {
            if(i >= numButtons) {
                buttons[i].gameObject.SetActive(false);
                continue;
            }

            buttons[i].gameObject.SetActive(true);
            buttons[i].transform.localPosition = new Vector3(0, -(buttonSpan - buttonHeight) / 2f + i * (buttonHeight + BUTTON_GAP), 0);
            buttons[i].SetMove(monster, i);
            buttons[i].disabled = monster.Controller != player || !monster.CanUse(i);
        }
    }
}
