using System;
using System.Collections.Generic;
using UnityEngine;

public struct TileEffect {
    public StatusEffect? appliedStatus;
    public int movementTax;
    public int duration;
    public GameObject prefab;
    public MonsterTrigger landEffect;

    public TileEffect(StatusEffect? appliedStatus, int movementTax, int duration, GameObject prefab, MonsterTrigger landEffect) {
        this.appliedStatus = appliedStatus;
        this.movementTax = movementTax;
        this.duration = duration;
        this.prefab = prefab;
        this.landEffect = landEffect;
    }
}
