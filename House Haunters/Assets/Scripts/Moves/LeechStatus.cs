using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// special move of the fungus monster
public class LeechStatus : UniqueStatus
{
    private Monster user;

    private LeechStatus(Monster user, Monster target) : base(target, 3, PrefabContainer.Instance.leechSeed, UniqueStatuses.LeechSpore) {
        user.OnDeath += Remove;
        this.user = user;
    }

    public static void ApplyLeech(Monster user, Vector2Int tile) {
        LeechStatus leech = new LeechStatus(user, LevelGrid.Instance.GetMonster(tile));
    }

    protected override void Remove() {
        base.Remove();
        user.OnDeath -= Remove;
    }

    protected override void DecreaseDuration() {
        target.TakeDamage(3);
        user.Heal(3);
        base.DecreaseDuration();
    }
}
