using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusParticle : Particle
{
    private Monster attachTarget;
    private int turnsLeft;

    public void AttachTo(Monster monster, int duration) {
        attachTarget = monster;
        monster.OnTurnEnd += DecreaseTimer;
        transform.SetParent(monster.transform);
        transform.localPosition = Vector3.zero;
        turnsLeft = duration;
    }

    private void DecreaseTimer() {
        turnsLeft--;
        if(turnsLeft <= 0) {
            Destroy(gameObject);
            attachTarget.OnTurnEnd -= DecreaseTimer;
        }
    }
}
