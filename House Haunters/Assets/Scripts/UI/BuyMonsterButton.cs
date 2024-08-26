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

        for(int i = 0; i < data.Recipe.Count; i++) {
            ingredientLogos[i].sprite = PrefabContainer.Instance.ingredientToSprite[data.Recipe[i]];
        }

        // set tooltip stats
        int healthScore = Mathf.Min(3, (data.Health - 22) / 2);
        int speedScore = data.Speed - 2;

        Move primary = data.Moves[MonsterType.PRIMARY_INDEX];
        int rangeScore = Mathf.CeilToInt(primary.Range / 2f); // 1-2,3-4,5-6

        int damageScore = 0;
        if(primary is Attack) {
            damageScore = (primary as Attack).Damage / 5; // needs balance first
        }
        else if(primary.Type == MoveType.Decay) {
            damageScore = 1;
        }

    }
}
