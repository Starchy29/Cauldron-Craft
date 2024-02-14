using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMenu : MonoBehaviour
{
    [SerializeField] private GameObject MoveButtonPrefab;

    private MoveButton[] buttons;
    private int numButtons;
    private MoveButton hoveredButton;
    private float buttonHeight;
    private const float BUTTON_GAP = 0.2f;

    public int? HoveredMoveSlot { get; private set; }
    public Monster Selected { get; private set; }
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

    void Update() {
        Vector2 mousePos = InputManager.Instance.GetMousePosition();

        MoveButton lastHovered = hoveredButton;
        hoveredButton = null;
        HoveredMoveSlot = null;
        for(int i = 0; i < numButtons; i++) {
            buttons[i].Hovered = Global.GetObjectArea(buttons[i].gameObject).Contains(mousePos);
            if(buttons[i].Hovered) {
                hoveredButton = buttons[i];
                if(!buttons[i].Disabled) {
                    HoveredMoveSlot = i;
                }
            }
        }

        // highlight the range of the move
        if(hoveredButton != lastHovered) {
            LevelGrid.Instance.ColorTiles(hoveredButton == null ? null : hoveredButton.CoveredArea, TileHighlighter.State.Highlighted);
        }
    }

    public void Open(Monster monster, Team player) {
        gameObject.SetActive(true);
        Selected = monster;
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
            buttons[i].transform.localPosition = new Vector3(0, (buttonSpan - buttonHeight) / 2f - i * (buttonHeight + BUTTON_GAP), 0);
            buttons[i].SetMove(Selected, i);
            buttons[i].Disabled = monster.Controller != player || !monster.CanUse(i);
        }
    }
}
