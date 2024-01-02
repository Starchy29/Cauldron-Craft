using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum MonsterName {
    Temporary,
    LostSoul,
    Demon,
    Flytrap,
    ThornBush,
    Mushroom,
}

public class MonstersData
{
    private static MonstersData instance;
    public static MonstersData Instance { get {
        if(instance == null) {
            instance = new MonstersData();
        }
        return instance;
    } }

    private MonsterType[] monsterTypes; // index is name enum cast to an int

    // define the stats and abilities of all monster types
    private MonstersData() {
        monsterTypes = new MonsterType[Enum.GetValues(typeof(MonsterName)).Length];

        monsterTypes[(int)MonsterName.Temporary] = new MonsterType(PrefabContainer.Instance.TempMonsterPrefab,
            Ingredient.Decay, Ingredient.Decay, Ingredient.Decay,
            10, 3
        );
    }

    public MonsterType GetMonsterData(MonsterName name) {
        return monsterTypes[(int)name];
    }
}
