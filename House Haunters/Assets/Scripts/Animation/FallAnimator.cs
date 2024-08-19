using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// animates an object falling into the pit. Places it back on a nearby tile at the end
public class FallAnimator : IMoveAnimator
{
    public bool Completed { get; private set; }

    private const float DURATION = 1f;
    private const float PAUSE = 0.3f;

    private GameObject faller;
    private Vector3 endPosition;
    private float t;
    private float startScale;
    private float pauseTime;

    public FallAnimator(GameObject faller, Vector3 endPosition) {
        this.faller = faller;
        this.endPosition = endPosition;
        startScale = faller.transform.localScale.x;
    }

    public void Start() {}

    public void Update(float deltaTime) {
        t += deltaTime / DURATION;
        if(t >= 1f) {
            pauseTime += deltaTime;
            if(pauseTime >= PAUSE) {
                Completed = true;
                faller.transform.localScale = new Vector3(startScale, startScale, 1f);
                faller.transform.position = endPosition;
            }
            return;
        }

        float currentScale = startScale * (1f - t*t);
        faller.transform.localScale = new Vector3(currentScale, currentScale, 1f);
    }
}
