using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BuyMenu : MonoBehaviour
{
    [SerializeField] private GameObject MonsterButtonPrefab;
    [SerializeField] private TMPro.TextMeshPro DecayAmount;
    [SerializeField] private TMPro.TextMeshPro FloraAmount;
    [SerializeField] private TMPro.TextMeshPro MineralAmount;
    [SerializeField] public GameObject Background;
    private BuyMonsterButton[] buttons;

    void Awake() {
        float ySpacing = 1.2f;
        float xSpacing = MonsterButtonPrefab.transform.localScale.x + 0.2f;

        MonsterName[] monsters = (MonsterName[])Enum.GetValues(typeof(MonsterName));
        float startY = (monsters.Length - 1) / 2f * ySpacing;
        buttons = new BuyMonsterButton[monsters.Length];
        for(int i = 0; i < monsters.Length; i++) {
            buttons[i] = Instantiate(MonsterButtonPrefab).GetComponent<BuyMonsterButton>();
            buttons[i].SetMonster(monsters[i]);
            buttons[i].transform.SetParent(transform);

            int ySpot = i / 2;
            int xSpot = i % 2;
            buttons[i].transform.localPosition = new Vector3(-xSpacing / 2f + xSpot * xSpacing, startY - ySpot * ySpacing, 0);
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

        // display ingredient amounts
        DecayAmount.text = "x" + team.Resources[Ingredient.Decay];
        FloraAmount.text = "x" + team.Resources[Ingredient.Flora];
        MineralAmount.text = "x" + team.Resources[Ingredient.Mineral];
    }
}
