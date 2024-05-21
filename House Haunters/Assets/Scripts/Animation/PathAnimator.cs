using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathAnimator : IMoveAnimator
{
    public bool Completed { get { return nextPointIndex > pathPoints.Count - 1; } }

    private Monster mover;
    private List<Vector3> pathPoints;
    private int nextPointIndex;
    private float speed;

    public PathAnimator(Monster mover, List<Vector3> pathPoints, float speed) {
        this.mover = mover;
        this.pathPoints = pathPoints;
        this.speed = speed;
    }

    public void Start() { }

    public void Update(float deltaTime) {
        if(Completed) {
            return;
        }

        Vector3 targetPos = pathPoints[nextPointIndex];
        Vector3 direction = (targetPos - mover.transform.position).normalized;
        mover.transform.position += speed * deltaTime * direction;

        mover.UpdateSortingOrder();
        if(direction.x > 0) {
            mover.SetSpriteFlip(false);
        }
        if(direction.x < 0) {
            mover.SetSpriteFlip(true);
        }

        // check if reached the destination
        if(Vector3.Dot(targetPos - mover.transform.position, direction) <= 0) {
            mover.transform.position = pathPoints[nextPointIndex];
            nextPointIndex++;
        }
    }
}
