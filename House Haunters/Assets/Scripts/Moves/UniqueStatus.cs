using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UniqueStatuses
{
    LeechSpore,
    Wither,
    Sentry,
    Hexed
}

// allows scripting specific behavior of unique status effects
public class UniqueStatus
{
    private static List<UniqueStatus> allStatuses = new List<UniqueStatus>();

    private GameObject visual;
    protected Monster target;
    public UniqueStatuses Type { get; private set; }
    public int Duration { get; private set; }

    protected UniqueStatus(Monster target, int duration, GameObject visualPrefab, UniqueStatuses type) {
        UniqueStatus existing = allStatuses.Find((UniqueStatus status) => { return status.target == target && status.Type == type; });
        if(existing != null) {
            existing.Remove();
        }

        allStatuses.Add(this);

        visual = GameObject.Instantiate(visualPrefab);
        visual.transform.SetParent(target.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.SetActive(false);
        AnimationsManager.Instance.QueueAnimation(new AppearanceAnimator(visual, true));

        this.target = target;
        this.Type = type;
        this.Duration = duration;
        target.UniqueStatuses.Add(this);
        target.OnTurnEnd += DecreaseDuration;
        target.OnDeath += Remove;
    }

    protected virtual void Remove() {
        allStatuses.Remove(this);
        GameObject.Destroy(visual);
        target.UniqueStatuses.Remove(this);
        target.OnDeath -= Remove;
        target.OnTurnEnd -= DecreaseDuration;
    }

    // override this function for additional turn end effects
    protected virtual void DecreaseDuration() {
        Duration--;
        if(Duration <= 0) {
            Remove();
        }
    }
}
