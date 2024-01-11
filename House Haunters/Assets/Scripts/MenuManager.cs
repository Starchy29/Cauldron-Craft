using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// manages player input in regards to menus during gameplay
public class MenuManager : MonoBehaviour
{
    private enum SelectedItem {
        None,
        Monster,
        Move
    }

    [SerializeField] private GameObject TileSelector;

    public bool UseKBMouse { get; set; }

    private SelectedItem currentSelection;
    private Vector3Int? hoveredTile;
    private Monster selectedMonster;
    private LevelGrid level;
    private Camera gameCamera;
    private GameManager gameManager;
    private Team controller;

    private List<List<Vector2Int>> tileGroups;
    private Vector2[] tileGroupCenters;
    private int selectedMove;

    void Start() {
        UseKBMouse = true;
        currentSelection = SelectedItem.None;
        level = LevelGrid.Instance;
        gameCamera = Camera.main;
        gameManager = GameManager.Instance;
        controller = gameManager.PlayerTeam;
    }

    void Update() {
        if(controller != gameManager.CurrentTurn) {
            return;
            // TO DO: make the visuals update when it's not your turn but disable selection
        }

        switch(currentSelection) {
            case SelectedItem.None:
                // look for a monster to select
                if(UseKBMouse && Mouse.current != null) {
                    Vector3 mousePos = GetMousePosition();
                    Vector3Int tile = level.Tiles.WorldToCell(mousePos);
                    if(level.IsInGrid((Vector2Int)tile)) {
                        hoveredTile = tile;
                        TileSelector.transform.position = level.Tiles.GetCellCenterWorld(hoveredTile.Value);
                        TileSelector.SetActive(true);
                    } else {
                        hoveredTile = null;
                        TileSelector.SetActive(false);
                    }

                    if(Mouse.current.leftButton.wasPressedThisFrame && hoveredTile.HasValue) {
                        Select((Vector2Int)hoveredTile);
                    }
                }
                break;

            case SelectedItem.Monster:
                int hoveredIndex = tileGroupCenters.IndexOf(
                    tileGroupCenters.Min((Vector2 spot) => { return Vector2.Distance((Vector2)GetMousePosition(), spot); }))
                    .Value;
                level.ColorTiles(tileGroups[hoveredIndex], TileHighlighter.State.Selectable);

                if(Mouse.current.leftButton.wasPressedThisFrame) {
                    selectedMonster.UseMove(0, tileGroups[hoveredIndex]);
                    level.ColorTiles(null, TileHighlighter.State.Highlighted);
                    level.ColorTiles(null, TileHighlighter.State.Selectable);
                    currentSelection = SelectedItem.None;
                    GameManager.Instance.EndTurn(controller);
                }
                break;
        }
    }

    private void Select(Vector2Int selectedTile) {
        switch(currentSelection) {
            case SelectedItem.None:
                GridEntity selectedEntity = level.GetEntity(selectedTile);
                if(selectedEntity == null || !(selectedEntity is Monster)) {
                    return;
                }
                    
                selectedMonster = (Monster)selectedEntity;
                currentSelection = SelectedItem.Monster;

                // determine which tiles can be walked to
                tileGroups = selectedMonster.GetMoveOptions(0);
                tileGroupCenters = DetermineCenters(tileGroups);

                List<Vector2Int> highlightTiles = new List<Vector2Int>();
                foreach(List<Vector2Int> group in tileGroups) {
                    highlightTiles.AddRange(group);
                }

                level.ColorTiles(highlightTiles, TileHighlighter.State.Highlighted);
                break;

            case SelectedItem.Move:
                break;
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

    private Vector3 GetMousePosition() {
        return gameCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }
}
