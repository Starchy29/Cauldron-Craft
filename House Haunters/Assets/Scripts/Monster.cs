using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : GridEntity
{
    [SerializeField] private MonsterName monsterType;

    public bool OnPlayerTeam { get; private set; }

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
