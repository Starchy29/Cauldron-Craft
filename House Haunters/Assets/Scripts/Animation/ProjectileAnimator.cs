using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAnimator : IMoveAnimator
{
    public bool Completed { get { return false; } }

    public ProjectileAnimator(GameObject prefab, Vector2 startPosition, Vector2 endPosition, float speed) {

    }

    public void Start() {
        throw new NotImplementedException();
    }

    public void Update(float deltaTime) {
        throw new NotImplementedException();
    }
}
