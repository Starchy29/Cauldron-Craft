using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveAnimator
{
    public abstract bool Completed { get; }

    public abstract void Start();

    public abstract void Update(float deltaTime);
}
