using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// special move of the fungus monster
public class LeechStatus
{
    public const int DURATION = 4;

    private Monster user;
    private Monster target;
    private int turnsLeft;

    private static List<LeechStatus> allLeeches = new List<LeechStatus>();

    public static void Infect(Monster user, Vector2Int tile) {
        Monster target = LevelGrid.Instance.GetMonster(tile);
        LeechStatus existingLeech = allLeeches.Find((LeechStatus leech) => { return leech.target == target; });
        if(existingLeech != null) {
            existingLeech.user = user;
            existingLeech.turnsLeft = DURATION;
            return;
        }

        allLeeches.Add(new LeechStatus(user, target));
    }

    private LeechStatus(Monster user, Monster target) {
        this.user = user;
        this.target = target;
        turnsLeft = DURATION;
        target.OnTurnEnd += StealLife;
    }

    private void StealLife() {
        target.TakeDamage(1, null);
        user.Heal(1);

        turnsLeft--;
        if(turnsLeft <= 0) {
            target.OnTurnEnd -= StealLife;
            allLeeches.Remove(this);
        }
    }
}
