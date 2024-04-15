using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationsManager : MonoBehaviour
{
    public static AnimationsManager Instance { get; private set; }

    private Queue<IMoveAnimator> animationQueue;

    public bool Animating { get { return animationQueue.Count > 0; } }
    public event Trigger OnAnimationsEnd;

    void Awake() {
        Instance = this;
        animationQueue = new Queue<IMoveAnimator>();
    }

    void Update() {
        if(!Animating) {
            return;
        }

        animationQueue.Peek().Update(Time.deltaTime);
        if(animationQueue.Peek().Completed) {
            animationQueue.Dequeue();
            if(Animating) {
                animationQueue.Peek().Start();
            } else {
                OnAnimationsEnd?.Invoke();
            }
        }
    }

    public void QueueAnimation(IMoveAnimator moveAnimator) {
        animationQueue.Enqueue(moveAnimator);
        if(animationQueue.Count == 1) {
            moveAnimator.Start();
        }
    }
}
