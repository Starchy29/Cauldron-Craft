using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexStatus : UniqueStatus
{
    private HexStatus(Monster target) : base(target, 1, PrefabContainer.Instance.hexBlast, UniqueStatuses.Hexed) { }

    public static void ApplyHex(Monster user, Vector2Int tile) {
        HexStatus hex = new HexStatus(LevelGrid.Instance.GetMonster(tile));
    }

    protected override void Remove() {
        base.Remove();
        // create particle
        target.TakeDamage(8);
    }
}
