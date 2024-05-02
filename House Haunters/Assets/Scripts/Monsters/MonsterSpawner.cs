using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a container object used to spawn and place a monster at start
public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] private MonsterName monsterType;
    [SerializeField] private int team;

    void Start() {
        Vector2Int tile = (Vector2Int)LevelGrid.Instance.Tiles.WorldToCell(transform.position);
        GameManager.Instance.SpawnMonster(monsterType, tile, GameManager.Instance.AllTeams[team]);
        Destroy(gameObject);
    }
}
