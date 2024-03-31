using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneDestructionAnimator : IMoveAnimator
{
    public bool Completed { get; private set; }

    private GameObject zone;

    public ZoneDestructionAnimator(GameObject zone) {
        this.zone = zone;
    }

    public void Start() {
        GameObject.Destroy(zone);
        Completed = true;
    }

    public void Update(float deltaTime) {}
}
