using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// manages player input in regards to menus during gameplay
public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject TileSelector;
    [SerializeField] private GameObject moveMenu;
    [SerializeField] private ControlledButton endTurnButton;

    public bool UseKBMouse { get; set; }
    
    private LevelGrid level;
    private Camera gameCamera;
    private GameManager gameManager;
    private Team controller;
    private ControlledButton[] moveButtons;

    // monster selection data
    private Monster selectedMonster;

    // target selection data
    private List<List<Vector2Int>> tileGroups;
    private Vector2[] tileGroupCenters;
    private int? selectedMove;

    void Start() {
        UseKBMouse = true;
        level = LevelGrid.Instance;
        gameCamera = Camera.main;
        gameManager = GameManager.Instance;
        controller = gameManager.PlayerTeam;

        moveButtons = new ControlledButton[4];
        for(int i = 0; i < 4; i++) {
            moveButtons[i] = moveMenu.transform.GetChild(i).GetComponent<ControlledButton>();
        }
    }

    void Update() {
        // reset all visibility
        Vector3 mousePos = GetMousePosition();
        TileSelector.SetActive(false);
        moveMenu.SetActive(false);
        endTurnButton.Disabled = true;
        endTurnButton.Hovered = false;

        //  target selection
        if(selectedMove.HasValue) {
            Vector2 closestMidpoint = tileGroupCenters.Min((Vector2 spot) => { return Vector2.Distance(GetMousePosition(), spot); });
            int hoveredTargetIndex = tileGroupCenters.IndexOf(closestMidpoint).Value;
            level.ColorTiles(tileGroups[hoveredTargetIndex], TileHighlighter.State.Selectable);

            if(SelectPressed()) {
                // use the move on the hovered target
                selectedMonster.UseMove(selectedMove.Value, tileGroups[hoveredTargetIndex]);
                selectedMove = null;
                selectedMonster = null;
                level.ColorTiles(null, TileHighlighter.State.Highlighted);
                level.ColorTiles(null, TileHighlighter.State.Selectable);
            }
            else if(BackPressed()) {
                // go back to move selection of the selected monster
                selectedMove = null;
                level.ColorTiles(null, TileHighlighter.State.Highlighted);
            }
            return;
        }

        endTurnButton.Disabled = false;

        // move selection
        if(selectedMonster != null) {
            moveMenu.SetActive(true);
            foreach(ControlledButton moveButton in moveButtons) {
                moveButton.Hovered = false;
            }

            if(selectedMonster.Controller == controller) {
                // check if hovering a move
                int? hoveredMove = FindHoveredMove(mousePos);

                if(hoveredMove.HasValue) {
                    moveButtons[hoveredMove.Value].Hovered = true;

                    if(moveButtons[hoveredMove.Value].Disabled == false && SelectPressed()) {
                        selectedMove = hoveredMove.Value;
                        tileGroups = selectedMonster.GetMoveOptions(selectedMove.Value);
                        tileGroupCenters = DetermineCenters(tileGroups);

                        List<Vector2Int> allTiles = new List<Vector2Int>();
                        foreach(List<Vector2Int> group in tileGroups) {
                            allTiles.AddRange(group);
                        }
                        level.ColorTiles(allTiles, TileHighlighter.State.Highlighted);
                    }
                    else if(BackPressed()) {
                        selectedMonster = null;
                    }

                    return; // do not allow selecting anything beneath the menu
                }
            }

            if(BackPressed()) {
                selectedMonster = null;
            }
        }

        if(endTurnButton.IsHovered(mousePos)) {
            endTurnButton.Hovered = true;

            if(SelectPressed()) {
                controller.EndTurn();
                selectedMove = null;
                selectedMonster = null;
            }
            return;
        }
        
        Vector3Int tile = level.Tiles.WorldToCell(mousePos);
        if(!level.IsInGrid((Vector2Int)tile)) {
            return;
        }

        TileSelector.SetActive(true);
        TileSelector.transform.position = level.Tiles.GetCellCenterWorld(tile);

        GridEntity hovered = level.GetEntity((Vector2Int)tile);
        if(hovered == null) {
            return;
        }

        if(SelectPressed() && hovered is Monster && gameManager.CurrentTurn == controller) {
            selectedMonster = (Monster)hovered;

            // determine which moves are usable
            for(int i = 0; i < moveButtons.Length; i++) {
                moveButtons[i].Disabled = selectedMonster.Controller != controller || !selectedMonster.CanUse(i);
            }
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

    private int? FindHoveredMove(Vector2 mousePos) {
        for(int i = 0; i < moveButtons.Length; i++) {
            if(moveButtons[i].IsHovered(mousePos)) {
                return i;
            }
        }

        return null;
    }

    private Vector3 GetMousePosition() {
        return gameCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    private bool SelectPressed() {
        return !AnimationsManager.Instance.Animating && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    private bool BackPressed() {
        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame
            || Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.backspaceKey.wasPressedThisFrame);
    }
}
