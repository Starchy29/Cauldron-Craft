using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField] private MonsterName monsterType;

    private bool onPlayerTeam;
    public bool OnPlayerTeam { get { return onPlayerTeam; } }

    private MonsterType monsterStats;
    private int health;
    // statuses
    // shields

    void Start() {
        monsterStats = MonsterData.Instance.GetMonsterData(monsterType);
        health = monsterStats.Health;
    }

    void Update() {
        
    }
}
