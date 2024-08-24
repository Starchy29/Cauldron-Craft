using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationsManager : MonoBehaviour
{
    public static AnimationsManager Instance { get; private set; }

    private Queue<IMoveAnimator> animationQueue;

    public bool Animating { get { return animationQueue.Count > 0; } }

    void Awake() {
        Instance = this;
        animationQueue = new Queue<IMoveAnimator>();
    }

    void Update() {
        if(animationQueue.Count == 0 || MenuManager.Instance.Paused) {
            return;
        }

        animationQueue.Peek().Update(Time.deltaTime);
        if(animationQueue.Peek().Completed) {
            animationQueue.Dequeue();
            if(animationQueue.Count > 0) {
                animationQueue.Peek().Start();
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
