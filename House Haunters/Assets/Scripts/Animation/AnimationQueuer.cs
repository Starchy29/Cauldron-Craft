using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void AnimQueueFunc(Monster user, List<Vector2Int> tiles);

public class AnimationQueuer
{
    public bool UseFilteredSelection { get; private set; }
    private AnimQueueFunc animator;

    public AnimationQueuer(bool useFilter, AnimQueueFunc animator) {
        UseFilteredSelection = useFilter;
        this.animator = animator;
    }

    public void QueueAnimation(Monster user, List<Vector2Int> tiles) {
        animator(user, tiles);
    }
}
