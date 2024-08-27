using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BuyMonsterButton : AutoButton
{
    [SerializeField] private SpriteRenderer monsterImage;
    [SerializeField] private SpriteRenderer[] ingredientLogos;
    [SerializeField] private GameObject craftIndicator;
    [SerializeField] private SpriteRenderer craftFilling;
    [SerializeField] private Sprite emptyCircle;
    [SerializeField] private Sprite fullCircle;

    public MonsterName MonsterOption { get; private set; }

    void Awake() {
        OnClick = () => { MenuManager.Instance.BuyMonster(MonsterOption); };
    }

    public void SetMonster(MonsterName monster, bool leftSide) {
        MonsterOption = monster;
        MonsterType data = MonstersData.Instance.GetMonsterData(monster);

        monsterImage.sprite = PrefabContainer.Instance.monsterToSprite[monster];

        for(int i = 0; i < data.Recipe.Count; i++) {
            ingredientLogos[i].sprite = PrefabContainer.Instance.ingredientToSprite[data.Recipe[i]];
        }

        if(!leftSide) {
            craftIndicator.transform.position = new Vector3(-craftIndicator.transform.position.x, 0f, 0f);
            Vector3 scale = craftIndicator.transform.localScale;
            scale.x *= -1f;
            craftIndicator.transform.localScale = scale;
        }

        // setup tooltip
        int healthScore = Mathf.Min(3, (data.Health - 22) / 2);
        int speedScore = data.Speed - 2;

        Move primary = data.Moves[MonsterType.PRIMARY_INDEX];
        int rangeScore = Mathf.CeilToInt(primary.Range / 2f); // 1-2,3-4,5-6

        int damageScore = 0;
        if(primary is Attack) {
            damageScore = Mathf.Max(1, ((primary as Attack).Damage - 5) / 2); // needs balance first
        }
        else if(primary.Type == MoveType.Decay) {
            damageScore = 1;
        }

        int[] scores = new int[] { rangeScore, speedScore, damageScore, healthScore }; // bottom up
        const float SPACING = 0.5f;

        for(int y = 0; y < 4; y++) {
            for(int x = 1; x < 4; x++) {
                PlaceMarker((x - 1.5f) * SPACING, (y - 1.5f) * SPACING, x <= scores[y]);
            }
        }
    }

    private void PlaceMarker(float x, float y, bool filled) {
        GameObject marker = new GameObject();
        SpriteRenderer render = marker.AddComponent<SpriteRenderer>();
        render.sortingLayerName = "UI";
        render.sortingOrder = 4;
        render.sprite = filled ? fullCircle : emptyCircle;
        marker.transform.SetParent(tooltip.transform);
        marker.transform.localPosition = new Vector3(x, y, 0f);
        marker.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
    }

    public void SetCrafted(Team team) {
        craftFilling.color = team.CraftedMonsters[MonsterOption] ? team.TeamColor : Color.clear;
    }
}
