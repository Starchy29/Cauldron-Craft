using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// special move of the fungus monster
public class LeechStatus
{
    private const int DURATION = 3;

    private Monster user;
    private Monster target;

    private GameObject visual;
    private int turnsLeft;

    private static List<LeechStatus> allLeeches = new List<LeechStatus>();

    private LeechStatus(Monster user, Monster target) {
        this.user = user;
        this.target = target;
        turnsLeft = DURATION;

        visual = GameObject.Instantiate(PrefabContainer.Instance.leechSeed);
        visual.transform.SetParent(target.transform);
        visual.transform.localPosition = Vector3.zero;

        target.OnTurnEnd += StealLife;
        target.OnDeath += Remove;
        user.OnDeath += Remove;
    }

    public static void ApplyLeech(Monster user, Vector2Int tile) {
        Monster target = LevelGrid.Instance.GetMonster(tile);

        LeechStatus existing = allLeeches.Find((LeechStatus leech) => { return leech.target == target; });
        if(existing != null) {
            if(existing.user == user) {
                // restart the timer
                existing.turnsLeft = DURATION;
                return;
            }

            // replace an existing leech from another user
            existing.Remove();
        }

        allLeeches.Add(new LeechStatus(user, target));
    }
    
    private void Remove() {
        allLeeches.Remove(this);
        GameObject.Destroy(visual);

        target.OnTurnEnd -= StealLife;
        target.OnDeath -= Remove;
        user.OnDeath -= Remove;
    }

    private void StealLife() {
        target.TakeDamage(2, null);
        user.Heal(2);
        
        turnsLeft--;
        if(turnsLeft <= 0) {
            Remove();
        }
    }
}
