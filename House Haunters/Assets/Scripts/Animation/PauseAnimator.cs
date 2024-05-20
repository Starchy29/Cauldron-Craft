using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// used to create a pause between animations
public class PauseAnimator : IMoveAnimator
{
    public bool Completed { get { return timer >= delay; } }

    private float timer;
    private float delay;

    public PauseAnimator(float delay) {
        this.delay = delay;
    }

    public void Start() {}

    public void Update(float deltaTime) {
        timer += deltaTime;
    }
}
