using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// manages player input in regards to menus during gameplay
public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject TileSelector;

    public bool UseKBMouse { get; set; }

    private Vector3Int hoveredTile;

    void Start() {
        UseKBMouse = true;
    }

    void Update() {
        if(UseKBMouse && Mouse.current != null) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            hoveredTile = LevelGrid.Instance.Tiles.WorldToCell(mousePos);
            TileSelector.transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld(hoveredTile);

            if(Mouse.current.leftButton.wasPressedThisFrame) {
                Select(hoveredTile);   
            }
        }
    }

    private void Select(Vector3Int tile) {
        Monster selected = LevelGrid.Instance.GetMonsterOnTile((Vector2Int)tile);
    }
}
