using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMenu : MonoBehaviour
{
    [SerializeField] private GameObject MoveButtonPrefab;

    private MoveButton[] buttons;
    private int numButtons;

    public Monster Selected { get; private set; }

    void Start() {
        buttons = new MoveButton[8];
        for(int i = 0; i < buttons.Length; i++) {
            buttons[i] = Instantiate(MoveButtonPrefab).GetComponent<MoveButton>();
        }
    }

    void Update() {
        Vector2 mousePos = InputManager.Instance.GetMousePosition();

        for(int i = 0; i < numButtons; i++) {
            buttons[i].Hovered = Global.GetObjectArea(buttons[i].gameObject).Contains(mousePos);
        }
    }

    public void Open(Monster monster, Team player) {
        Selected = monster;
        Move[] moves = monster.Stats.Moves;

        numButtons = moves.Length;
        for(int i = 0; i < buttons.Length; i++) {
            if(i >= numButtons) {
                buttons[i].gameObject.SetActive(false);
                continue;
            }

            buttons[i].gameObject.SetActive(true);
            buttons[i].SetMove(moves[i]);
            buttons[i].Disabled = monster.Controller != player || !monster.CanUse(i);
            buttons[i].CoveredArea = moves[i].GetCoveredArea(monster);
        }
    }
}
