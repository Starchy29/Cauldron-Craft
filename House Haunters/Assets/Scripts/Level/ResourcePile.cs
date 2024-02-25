using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Ingredient
{
    Decay,
    Plant,
    Mineral,
    Insect
}

public class ResourcePile : GridEntity
{
    [SerializeField] private Ingredient type;
    public Ingredient Type { get { return type; } }

    void Start() {
        GameManager.Instance.OnTurnEnd += GrantResource;
        Tile = (Vector2Int)LevelGrid.Instance.Tiles.WorldToCell(transform.position);
        transform.position = LevelGrid.Instance.Tiles.GetCellCenterWorld((Vector3Int)Tile);
        LevelGrid.Instance.PlaceEntity(this, Tile);
    }

    private void GrantResource(Team turnEnder, Team nextTurn) {
        // check for a change in ownership
        LevelGrid level = LevelGrid.Instance;
        List<Monster> adjacentMonsters = level.GetTilesInRange(Tile, 1, true).Map((Vector2Int tile) => { return level.GetMonster(tile); }).Filter((Monster monster) => { return monster != null; });

        Team adjacentTeam = null;
        foreach(Monster monster in adjacentMonsters) {
            if(adjacentTeam == null) {
                adjacentTeam = monster.Controller;
            }
            else if(monster.Controller != adjacentTeam) {
                Controller = null;
                return;
            }
        }

        if(adjacentTeam != null) {
            Controller = adjacentTeam;
        }

        // give a resource to the controller
        if(turnEnder == Controller) {
            Controller.AddResource(type);
        }
    }
}
