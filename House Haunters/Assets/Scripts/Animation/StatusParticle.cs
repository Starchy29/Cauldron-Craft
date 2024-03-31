using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusParticle : Particle
{
    private Monster attachTarget;
    private int turnsLeft;

    private static List<StatusParticle> allStatuses = new List<StatusParticle>();

    public void AttachTo(Monster monster, int duration) {
        // if the target monster already has this particle, restart the old and delete the new one
        StatusParticle duplicate = allStatuses.Find((StatusParticle status) => { return status.attachTarget == monster && status.gameObject.name == gameObject.name; });
        if(duplicate != null) {
            duplicate.turnsLeft = duration;
            Destroy(gameObject);
            return;
        }

        attachTarget = monster;
        monster.OnTurnEnd += DecreaseTimer;
        allStatuses.Add(this);
        transform.SetParent(monster.transform);
        transform.localPosition = Vector3.zero;
        turnsLeft = duration;
    }

    private void DecreaseTimer() {
        turnsLeft--;
        if(turnsLeft <= 0) {
            Destroy(gameObject);
            attachTarget.OnTurnEnd -= DecreaseTimer;
            allStatuses.Remove(this);
        }
    }
}
