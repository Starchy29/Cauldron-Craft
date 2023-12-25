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
    private Stack<Vector2Int> path;

    void Start() {
        UseKBMouse = true;
        state = SelectionState.Monster;
    }

    void Update() {
        if(UseKBMouse && Mouse.current != null) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector3Int tile = LevelGrid.Instance.Tiles.WorldToCell(mousePos);
            if(LevelGrid.Instance.IsInGrid((Vector2Int)tile)) {
                hoveredTile = tile;
                TileSelector.transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld(hoveredTile.Value);
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
                GridEntity selectedEntity = LevelGrid.Instance.GetEntity(selectedTile);
                if(selectedEntity == null || selectedEntity.GetComponent<Monster>() == null) {
                    return;
                }
                    
                selectedMonster = selectedEntity.GetComponent<Monster>();
                state = SelectionState.Movement;

                // determine which tiles can be walked to
                List<Vector2Int> walkableTiles = LevelGrid.Instance.GetTilesInRange(selectedTile, selectedMonster.Stats.Speed, false)
                    .Filter((Vector2Int tile) => { return selectedMonster.FindPath(tile) != null; });
                walkableTiles.Remove(selectedTile);
                LevelGrid.Instance.HighlightTiles(walkableTiles);
                break;

            case SelectionState.Movement:
                // move monster
                List<Vector2Int> path = selectedMonster.FindPath(selectedTile);
                if(path != null) {
                    LevelGrid.Instance.MoveEntity(selectedMonster, selectedTile);
                    LevelGrid.Instance.HighlightTiles(null);
                    state = SelectionState.Monster;
                }
                break;
        }
    }
}
