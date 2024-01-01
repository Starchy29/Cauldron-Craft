using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack
{
    private int damage;
    private int range;
    private bool straightShot;

    public Attack() {

    }

    protected virtual void OnEnemyHit() { }
}
