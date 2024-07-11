using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridEntity : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] private int startTeam;
    public Vector2Int Tile { get; set; }

    public Team Controller;

    protected virtual void Start() {
        Tile = (Vector2Int)LevelGrid.Instance.Tiles.WorldToCell(transform.position);
        transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)Tile);
        LevelGrid.Instance.PlaceEntity(this, Tile);
        UpdateSortingOrder();
        if(startTeam >= 0) {
            Controller = GameManager.Instance.AllTeams[startTeam];
        }
    }

    // make it so entities lower on screen are drawn above higher ones
    public void UpdateSortingOrder() {
        spriteRenderer.sortingOrder = (int)(-100 * transform.position.y);
    }

    public void SetSpriteFlip(bool flipped) {
        Vector3 scale = spriteRenderer.gameObject.transform.localScale;
        scale.x = (flipped ? -1 : 1) * Mathf.Abs(scale.x);
        spriteRenderer.gameObject.transform.localScale = scale;
    }

    public void SetOutlineColor(Color color) {
        spriteRenderer.material.color = color;
    }
}
