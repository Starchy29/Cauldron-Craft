using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// manages player input in regards to menus during gameplay
public class MenuManager : MonoBehaviour
{
    private enum SelectionState {
        None,
        Monster,
        Movement
    }

    [SerializeField] private GameObject TileSelector;

    public bool UseKBMouse { get; set; }

    private SelectionState state;
    private Vector3Int? hoveredTile;
    private Monster selectedMonster;
    private LevelGrid level;
    private Camera gameCamera;

    private List<Vector2Int> selectableTiles; // delete this
    private Vector2[] tileGroupCenters;

    void Start() {
        UseKBMouse = true;
        state = SelectionState.Monster;
        level = LevelGrid.Instance;
        gameCamera = Camera.main;
    }

    void Update() {
        if(UseKBMouse && Mouse.current != null) {
            Vector3 mousePos = gameCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
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
    }

    private void Select(Vector2Int selectedTile) {
        switch(state) {
            case SelectionState.Monster:
                GridEntity selectedEntity = level.GetEntity(selectedTile);
                if(selectedEntity == null || selectedEntity.GetComponent<Monster>() == null) {
                    return;
                }
                    
                selectedMonster = selectedEntity.GetComponent<Monster>();
                state = SelectionState.Movement;

                // determine which tiles can be walked to
                selectableTiles = level.GetTilesInRange(selectedTile, selectedMonster.Stats.Speed, false)
                    .Filter((Vector2Int tile) => { return selectedMonster.FindPath(tile) != null; });
                selectableTiles.Remove(selectedTile);
                level.ColorTiles(selectableTiles, TileHighlighter.State.Highlighted);
                level.ColorTiles(selectedTile, TileHighlighter.State.Selected);
                break;

            case SelectionState.Movement:
                // move monster
                List<Vector2Int> path = selectedMonster.FindPath(selectedTile);
                if(path != null) {
                    level.MoveEntity(selectedMonster, selectedTile);
                    level.ColorTiles(null, TileHighlighter.State.Highlighted);
                    level.ColorTiles(null, TileHighlighter.State.Selected);
                    state = SelectionState.Monster;
                }
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
}
