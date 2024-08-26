using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrustAnimator : IMoveAnimator
{
    public bool Completed { get; private set; }

    private GameObject mover;
    private Vector3 direction;
    private Vector3 startPos;

    private const float MAX_DIST = 0.7f;
    private float t;

    public ThrustAnimator(GameObject mover, Vector2 direction) {
        this.mover = mover;
        this.direction = direction.normalized;
    }

    public void Start() {
        startPos = mover.transform.position;
    }

    public void Update(float deltaTime) {
        t += 5f * deltaTime;
        if(t >= 1f) {
            t = 1f;
            Completed = true;
        }

        float distance = 1f - 2f * Mathf.Abs(t - 0.5f);
        mover.transform.position = startPos + MAX_DIST * distance * direction;
    }
}
