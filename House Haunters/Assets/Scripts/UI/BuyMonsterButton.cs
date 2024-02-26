using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BuyMonsterButton : ControlledButton
{
    [SerializeField] private SpriteRenderer monsterImage;
    [SerializeField] private SpriteRenderer[] ingredientLogos;

    public MonsterType MonsterOption { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMonster(MonsterName monster) {
        MonsterOption = MonstersData.Instance.GetMonsterData(monster);

        monsterImage.sprite = PrefabContainer.Instance.monsterToSprite[monster];

        int nextIngredient = 0;
        foreach(Ingredient ingredient in Enum.GetValues(typeof(Ingredient))) {
            for(int i = 0; i < MonsterOption.Recipe[ingredient]; i++) {
                ingredientLogos[nextIngredient].sprite = PrefabContainer.Instance.ingredientToSprite[ingredient];
                nextIngredient++;
            }
        }
    }
}
