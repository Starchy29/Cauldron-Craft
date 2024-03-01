using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cauldron : GridEntity
{
    private MonsterType cookingMonster;
    private bool cookingTurn;

    void Start() {
        Controller.OnTurnStart += Cook;
    }

    private void Cook() {
        if(cookingMonster != null) {
            // find the spot to spawn on

            cookingMonster = null;
        }
    }
}
