using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// allows insertion of any function into the animation queue
public class FunctionAnimator : IMoveAnimator
{
    public bool Completed { get { return true; } }

    private Trigger effect;

    public FunctionAnimator(Trigger effect) {
        this.effect = effect;
    }

    public void Start() {
        effect();
    }

    public void Update(float deltaTime) {}
}
