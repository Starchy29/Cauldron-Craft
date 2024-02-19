using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// manages player input in regards to menus during gameplay
public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject TileSelector;
    [SerializeField] private MoveMenu moveMenu;
    [SerializeField] private ControlledButton endTurnButton;

    public bool UseKBMouse { get; set; }

    private enum SelectionTarget { Monster, Move, Targets }
    private SelectionTarget state;
    
    private LevelGrid level;
    private GameManager gameManager;
    private Team controller;

    // target selection data
    private List<List<Vector2Int>> tileGroups;
    private Vector2[] tileGroupCenters;
    private int selectedMoveSlot;

    void Start() {
        state = SelectionTarget.Monster;
        UseKBMouse = true;
        level = LevelGrid.Instance;
        gameManager = GameManager.Instance;
        controller = gameManager.PlayerTeam;
    }

    void Update() {
        InputManager input = InputManager.Instance;
        Vector2 mousePos = InputManager.Instance.GetMousePosition();

        switch(state) {
            case SelectionTarget.Monster:
                UpdateMonsterSelector(mousePos);
                UpdateEndTurnButton(mousePos);
                break;

            case SelectionTarget.Move:
                // update handled in MoveMenu.cs

                if(input.BackPressed()) {
                    state = SelectionTarget.Monster;
                    moveMenu.gameObject.SetActive(false);
                    level.ColorTiles(null, TileHighlighter.State.Highlighted);
                    break;
                }

                TileSelector.SetActive(false);
                if(Global.GetObjectArea(moveMenu.Background).Contains(mousePos)) {
                    if(moveMenu.HoveredMoveSlot.HasValue && input.SelectPressed()) {
                        // select a move
                        state = SelectionTarget.Targets;
                        moveMenu.gameObject.SetActive(false);
                        selectedMoveSlot = moveMenu.HoveredMoveSlot.Value;
                        tileGroups = moveMenu.Selected.GetMoveOptions(selectedMoveSlot);
                        tileGroupCenters = DetermineCenters(tileGroups);

                        List<Vector2Int> allTiles = new List<Vector2Int>();
                        foreach(List<Vector2Int> group in tileGroups) {
                            allTiles.AddRange(group);
                        }
                        level.ColorTiles(allTiles, TileHighlighter.State.Selectable);
                        level.ColorTiles(null, TileHighlighter.State.Highlighted);
                    }
                } else {
                    // if not hovering the move menu, check if selecting a different monster
                    UpdateMonsterSelector(mousePos);
                    UpdateEndTurnButton(mousePos);
                }
                break;

            case SelectionTarget.Targets:
                Vector2 closestMidpoint = tileGroupCenters.Min((Vector2 spot) => { return Vector2.Distance(mousePos, spot); });
                int hoveredTargetIndex = tileGroupCenters.IndexOf(closestMidpoint).Value;
                level.ColorTiles(tileGroups[hoveredTargetIndex], TileHighlighter.State.Hovered);

                if(input.SelectPressed()) {
                    // use the move on the hovered target
                    moveMenu.Selected.UseMove(selectedMoveSlot, tileGroups[hoveredTargetIndex]);
                    level.ColorTiles(null, TileHighlighter.State.Hovered);
                    level.ColorTiles(null, TileHighlighter.State.Selectable);
                    state = SelectionTarget.Monster;
                }
                else if(input.BackPressed()) {
                    // go back to move selection of the selected monster
                    state = SelectionTarget.Move;
                    moveMenu.gameObject.SetActive(true);
                    level.ColorTiles(null, TileHighlighter.State.Hovered);
                    level.ColorTiles(null, TileHighlighter.State.Selectable);
                }
                break;
        }
    }

    private void UpdateMonsterSelector(Vector2 mousePos) {
        TileSelector.SetActive(false);
        Vector3Int tile = level.Tiles.WorldToCell(mousePos);
        if(!level.IsInGrid((Vector2Int)tile)) {
            return;
        }

        TileSelector.SetActive(true);
        TileSelector.transform.position = level.Tiles.GetCellCenterWorld(tile);

        Monster hovered = level.GetMonster((Vector2Int)tile);
        if(hovered == null) {
            if(InputManager.Instance.SelectPressed()) {
                state = SelectionTarget.Monster; // close move menu
                moveMenu.gameObject.SetActive(false);
            }
            return;
        }

        if(gameManager.CurrentTurn == controller && InputManager.Instance.SelectPressed()) {
            moveMenu.GetComponent<MoveMenu>().Open(hovered, controller);
            state = SelectionTarget.Move;
        }
    }

    private void UpdateEndTurnButton(Vector2 mousePos) {
        endTurnButton.Hovered = endTurnButton.IsHovered(mousePos);

        if(endTurnButton.Hovered && InputManager.Instance.SelectPressed()) {
            controller.EndTurn();
            moveMenu.gameObject.SetActive(false);
            state = SelectionTarget.Monster;
        }
    }

    private Vector2[] DetermineCenters(List<List<Vector2Int>> tileGroups) {
        Vector2[] centers = new Vector2[tileGroups.Count];
        for(int i = 0; i < tileGroups.Count; i++) {
            Vector3 center = new Vector2();
            foreach(Vector2Int tile in tileGroups[i]) {
                center += level.Tiles.GetCellCenterWorld((Vector3Int)tile);
            }
            centers[i] = center / tileGroups[i].Count;
        }
        return centers;
    }
}
