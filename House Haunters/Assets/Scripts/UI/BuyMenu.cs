using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BuyMenu : MonoBehaviour
{
    [SerializeField] private GameObject MonsterButtonPrefab;
    [SerializeField] public GameObject Background;
    private BuyMonsterButton[] buttons;

    void Awake() {
        float spacing = 1.2f;

        MonsterName[] monsters = (MonsterName[])Enum.GetValues(typeof(MonsterName));
        float startY = (monsters.Length - 1) / 2f * spacing;
        buttons = new BuyMonsterButton[monsters.Length];
        for(int i = 0; i < monsters.Length; i++) {
            buttons[i] = Instantiate(MonsterButtonPrefab).GetComponent<BuyMonsterButton>();
            buttons[i].SetMonster(monsters[i]);
            buttons[i].transform.SetParent(transform);
            buttons[i].transform.localPosition = new Vector3(0, startY - i * spacing, 0);
        }

        gameObject.SetActive(false);
    }

    public void Open(Team team) {
        gameObject.SetActive(true);

        // move to the correct side of the screen
        Vector3 craftPos = transform.localPosition;
        craftPos.x = (team.OnLeft ? -1 : 1) * Mathf.Abs(craftPos.x);
        transform.localPosition = craftPos;

        // set button clickablility
        bool cauldronReady = team.Spawnpoint.CookState == Cauldron.State.Ready;
        bool currentTurn = GameManager.Instance.CurrentTurn == team;
        foreach(BuyMonsterButton button in buttons) {
            button.Disabled = !team.CanAfford(button.MonsterOption) || !cauldronReady || !currentTurn;
            button.checkmark.SetActive(team.CraftedMonsters[button.MonsterOption]);
        }
    }
}
