using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrustAnimator : IMoveAnimator
{
    public bool Completed { get; private set; }

    private GameObject mover;
    private Vector3 directionTarget;
    private Vector3 direction;
    private Vector3 startPos;

    private const float MAX_DIST = 0.7f;
    private float t;
    private bool enlarge;
    private Vector3 startScale;

    public ThrustAnimator(GameObject mover, Vector3 directionTarget, bool enlarge = false) {
        this.mover = mover;
        this.directionTarget = directionTarget;
        this.enlarge = enlarge;
    }

    public void Start() {
        startPos = mover.transform.position;
        direction = (directionTarget - startPos).normalized;
        startScale = mover.transform.localScale;
    }

    public void Update(float deltaTime) {
        t += 5f * deltaTime;
        if(t >= 1f) {
            t = 1f;
            Completed = true;
            mover.transform.position = startPos;
            mover.transform.localScale = startScale;
            return;
        }

        float distance = 1f - 2f * Mathf.Abs(t - 0.5f);
        mover.transform.position = startPos + MAX_DIST * distance * direction;

        if(enlarge) {
            float growth = 1f - 2f * Mathf.Abs(t - 0.5f);
            mover.transform.localScale = new Vector3(startScale.x + growth, startScale.y + growth, startScale.z);
        }
    }
}
