using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WitherStatus : UniqueStatus
{
    private WitherStatus(Monster target) : base(target, 3, PrefabContainer.Instance.demonCurse, Type.Wither) { }

    public static void Apply(Monster user, Vector2Int tile) {
        WitherStatus wither = new WitherStatus(LevelGrid.Instance.GetMonster(tile));
    }

    protected override void DecreaseDuration() {
        target.TakeDamage(4, null);
        base.DecreaseDuration();
    }
}
