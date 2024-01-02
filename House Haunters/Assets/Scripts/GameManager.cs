using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private bool playerTurn;

    void Awake() {
        Instance = this;
        playerTurn = true;
    }

    void Start() {
        LevelGrid.Instance.SpawnEntity(MonstersData.Instance.GetMonsterData(MonsterName.Temporary).Prefab, Vector2Int.zero);
    }

    void Update() {
        if(playerTurn) {
            return;
        }

        // run the AI's turn
    }

    public void EndPlayerTurn() {
        playerTurn = false;
    }
}
