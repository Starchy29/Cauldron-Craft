using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BuyMonsterButton : AutoButton
{
    [SerializeField] private SpriteRenderer monsterImage;
    [SerializeField] private SpriteRenderer[] ingredientLogos;
    [SerializeField] public GameObject checkmark;

    public MonsterName MonsterOption { get; private set; }

    void Awake() {
        OnClick = () => { MenuManager.Instance.BuyMonster(MonsterOption); };
    }

    public void SetMonster(MonsterName monster) {
        MonsterOption = monster;
        MonsterType data = MonstersData.Instance.GetMonsterData(monster);

        monsterImage.sprite = PrefabContainer.Instance.monsterToSprite[monster];

        for(int i = 0; i < data.Recipe.Count; i++) {
            ingredientLogos[i].sprite = PrefabContainer.Instance.ingredientToSprite[data.Recipe[i]];
        }
    }
}
