using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntity : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer renderer;
    [SerializeField] private int startTeam;
    public Vector2Int Tile { get; set; }

    private Team controller;
    public Team Controller { 
        get { return controller; }
        set {
            controller = value;
            renderer.material.color = Controller == null ? Color.clear : Controller.TeamColor;
        }
    }

    protected virtual void Start() {
        Tile = (Vector2Int)LevelGrid.Instance.Tiles.WorldToCell(transform.position);
        transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)Tile);
        LevelGrid.Instance.PlaceEntity(this, Tile);

        switch(startTeam) {
            case 1:
                Controller = GameManager.Instance.PlayerTeam;
                break;

            case 2:
                Controller = GameManager.Instance.EnemyTeam;
                break;
        }
    }
}
