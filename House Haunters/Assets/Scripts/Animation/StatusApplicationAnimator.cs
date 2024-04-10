using System;
using System.Collections.Generic;
using UnityEngine;

class StatusApplicationAnimator : IMoveAnimator
{
    public bool Completed { get; private set; }

    private Monster target;
    private GameObject visualPrefab;
    private int duration;

    public StatusApplicationAnimator(Monster target, GameObject visualPrefab, int duration) {
        this.target = target;
        this.visualPrefab = visualPrefab;
        this.duration = duration;
    }

    public void Start() {
        //StatusParticle visual = GameObject.Instantiate(visualPrefab).GetComponent<StatusParticle>();
        //visual.AttachTo(target, duration);
        //Completed = true;
    }

    public void Update(float deltaTime) { }
}
