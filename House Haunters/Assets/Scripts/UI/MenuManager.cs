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
                if(Global.GetObjectArea(moveMenu.gameObject).Contains(mousePos)) {
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
                    state = SelectionTarget.Monster;
                    moveMenu.gameObject.SetActive(true);
                    level.ColorTiles(null, TileHighlighter.State.Hovered);
                    level.ColorTiles(null, TileHighlighter.State.Selectable);
                }
                break;
        }


        // reset all visibility
        //TileSelector.SetActive(false);
        //moveMenu.gameObject.SetActive(false);
        //endTurnButton.Disabled = true;
        //endTurnButton.Hovered = false;

        //  target selection
        //if(selectedMove.HasValue) {
        //    Vector2 closestMidpoint = tileGroupCenters.Min((Vector2 spot) => { return Vector2.Distance(mousePos, spot); });
        //    int hoveredTargetIndex = tileGroupCenters.IndexOf(closestMidpoint).Value;
        //    level.ColorTiles(tileGroups[hoveredTargetIndex], TileHighlighter.State.Selectable);

        //    if(input.SelectPressed()) {
        //        // use the move on the hovered target
        //        selectedMonster.UseMove(selectedMove.Value, tileGroups[hoveredTargetIndex]);
        //        selectedMove = null;
        //        selectedMonster = null;
        //        level.ColorTiles(null, TileHighlighter.State.Highlighted);
        //        level.ColorTiles(null, TileHighlighter.State.Selectable);
        //    }
        //    else if(input.BackPressed()) {
        //        // go back to move selection of the selected monster
        //        selectedMove = null;
        //        level.ColorTiles(null, TileHighlighter.State.Highlighted);
        //    }
        //    return;
        //}

        //endTurnButton.Disabled = false;

        //// move selection
        //if(selectedMonster != null) {
        //    moveMenu.gameObject.SetActive(true);
        //    foreach(ControlledButton moveButton in moveButtons) {
        //        moveButton.Hovered = false;
        //    }

        //    if(selectedMonster.Controller == controller) {
        //        // check if hovering a move
        //        int? hoveredMove = FindHoveredMove(mousePos);

        //        if(hoveredMove.HasValue) {
        //            moveButtons[hoveredMove.Value].Hovered = true;

        //            if(moveButtons[hoveredMove.Value].Disabled == false && input.SelectPressed()) {
        //                selectedMove = hoveredMove.Value;
        //                tileGroups = selectedMonster.GetMoveOptions(selectedMove.Value);
        //                tileGroupCenters = DetermineCenters(tileGroups);

        //                List<Vector2Int> allTiles = new List<Vector2Int>();
        //                foreach(List<Vector2Int> group in tileGroups) {
        //                    allTiles.AddRange(group);
        //                }
        //                level.ColorTiles(allTiles, TileHighlighter.State.Highlighted);
        //            }
        //            else if(input.BackPressed()) {
        //                selectedMonster = null;
        //            }

        //            return; // do not allow selecting anything beneath the menu
        //        }
        //    }

        //    if(input.BackPressed()) {
        //        selectedMonster = null;
        //    }
        //}

        //if(endTurnButton.IsHovered(mousePos)) {
        //    endTurnButton.Hovered = true;

        //    if(input.SelectPressed()) {
        //        controller.EndTurn();
        //        selectedMove = null;
        //        selectedMonster = null;
        //    }
        //    return;
        //}
        
        //Vector3Int tile = level.Tiles.WorldToCell(mousePos);
        //if(!level.IsInGrid((Vector2Int)tile)) {
        //    return;
        //}

        //TileSelector.SetActive(true);
        //TileSelector.transform.position = level.Tiles.GetCellCenterWorld(tile);

        //GridEntity hovered = level.GetEntity((Vector2Int)tile);
        //if(hovered == null) {
        //    return;
        //}

        //if(input.SelectPressed() && hovered is Monster && gameManager.CurrentTurn == controller) {
        //    selectedMonster = (Monster)hovered;

        //    // determine which moves are usable
        //    for(int i = 0; i < moveButtons.Length; i++) {
        //        moveButtons[i].Disabled = selectedMonster.Controller != controller || !selectedMonster.CanUse(i);
        //    }
        //}
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
