using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BuyMonsterButton : AutoButton
{
    [SerializeField] private SpriteRenderer monsterImage;
    [SerializeField] private SpriteRenderer[] ingredientLogos;

    public MonsterName MonsterOption { get; private set; }

    void Awake() {
        OnClick = () => { MenuManager.Instance.BuyMonster(MonsterOption); };
    }

    public void SetMonster(MonsterName monster) {
        MonsterOption = monster;
        MonsterType data = MonstersData.Instance.GetMonsterData(monster);

        monsterImage.sprite = PrefabContainer.Instance.monsterToSprite[monster];

        int nextIngredient = 0;
        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            for(int i = 0; i < data.Recipe[ingredient]; i++) {
                ingredientLogos[nextIngredient].sprite = PrefabContainer.Instance.ingredientToSprite[ingredient];
                nextIngredient++;
            }
        }
    }
}
