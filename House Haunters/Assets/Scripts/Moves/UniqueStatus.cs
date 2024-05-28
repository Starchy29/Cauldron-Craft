using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniqueStatus
{
    private static List<UniqueStatus> allStatuses = new List<UniqueStatus>();

    protected Monster target;
    private int duration;
    private GameObject visual;
    private Type type;

    public enum Type {
        LeechSpore,
        Wither
    }

    protected UniqueStatus(Monster target, int duration, GameObject visualPrefab, Type type) {
        UniqueStatus existing = allStatuses.Find((UniqueStatus status) => { return status.target == target && status.type == type; });
        if(existing != null) {
            existing.Remove();
        }

        allStatuses.Add(this);

        visual = GameObject.Instantiate(visualPrefab);
        visual.transform.SetParent(target.transform);
        visual.transform.localPosition = Vector3.zero;

        this.target = target;
        this.type = type;
        this.duration = duration;
        target.ActiveUniqueStatuses[type] = true;
        target.OnTurnEnd += DecreaseDuration;
        target.OnDeath += Remove;
    }

    protected virtual void Remove() {
        allStatuses.Remove(this);
        GameObject.Destroy(visual);
        target.ActiveUniqueStatuses[type] = false;
        target.OnDeath -= Remove;
        target.OnTurnEnd -= DecreaseDuration;
    }

    // override this function for additional turn end effects
    protected virtual void DecreaseDuration() {
        duration--;
        if(duration <= 0) {
            Remove();
        }
    }
}
