using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BuyMenu : MonoBehaviour
{
    [SerializeField] private GameObject MonsterButtonPrefab;
    private BuyMonsterButton[] buttons;

    void Start() {
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

    void Update() {
        
    }

    public void Open(Team team) {
        gameObject.SetActive(true);
        Dictionary<Ingredient, int> resources = team.Resources;
        foreach(BuyMonsterButton button in buttons) {
            button.Disabled = CanBuy(resources, button.MonsterOption);
        }
    }

    private bool CanBuy(Dictionary<Ingredient, int> resources, MonsterType monster) {
        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            if(resources[ingredient] < monster.Recipe[ingredient]) {
                return false;
            }
        }

        return true;
    }
}
