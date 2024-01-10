using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : Move
{
    public int Damage { get; private set; }
    public int Range { get; private set; }
    public bool StraightShot { get; private set; }

    public Attack(int cost, Selector selection) : base(cost, Move.Targets.Enemies, selection) {

    }

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        
    }
}
