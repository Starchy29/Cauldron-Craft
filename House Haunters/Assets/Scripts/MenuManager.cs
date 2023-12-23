using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// manages player input in regards to menus during gameplay
public class MenuManager : MonoBehaviour
{
    private enum SelectionState {
        None,
        Tile,
        Monster,
        Move
    }

    [SerializeField] private GameObject TileSelector;

    public bool UseKBMouse { get; set; }

    private SelectionState state;
    private Vector3Int hoveredTile;
    private Monster selectedMonster;

    void Start() {
        UseKBMouse = true;
        state = SelectionState.Tile;
    }

    void Update() {
        if(UseKBMouse && Mouse.current != null) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            hoveredTile = LevelGrid.Instance.Tiles.WorldToCell(mousePos);
            TileSelector.transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld(hoveredTile);

            if(Mouse.current.leftButton.wasPressedThisFrame) {
                Select((Vector2Int)hoveredTile);
            }
        }
    }

    private void Select(Vector2Int tile) {
        switch(state) {
            case SelectionState.Tile:
                selectedMonster = LevelGrid.Instance.GetEntityOnTile(tile).GetComponent<Monster>();
                if(selectedMonster != null) {
                    state = SelectionState.Monster;
                }
                break;

            case SelectionState.Monster:
                // move monster
                LevelGrid.Instance.MoveEntity(selectedMonster, tile);
                state = SelectionState.Tile;
                break;
        }
    }
}
