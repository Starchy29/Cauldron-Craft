using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationsManager : MonoBehaviour
{
    public static AnimationsManager Instance { get; private set; }

    private Queue<MoveAnimator> animationQueue;

    public bool Animating { get { return animationQueue.Count > 0; } }

    void Awake() {
        Instance = this;
        animationQueue = new Queue<MoveAnimator>();
    }

    void Update() {
        
    }
}
