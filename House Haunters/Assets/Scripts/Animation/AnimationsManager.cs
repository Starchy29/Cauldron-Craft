using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationsManager : MonoBehaviour
{
    public static AnimationsManager Instance { get; private set; }

    private Queue<IMoveAnimator> animationQueue;

    public bool Animating { get { return animationQueue.Count > 0; } }

    public delegate void TurnTrigger(Team turn);
    public event TurnTrigger OnAnimationsEnd;

    void Awake() {
        Instance = this;
        animationQueue = new Queue<IMoveAnimator>();
    }

    void Update() {
        if(animationQueue.Count == 0) {
            return;
        }

        animationQueue.Peek().Update(Time.deltaTime);
        if(animationQueue.Peek().Completed) {
            animationQueue.Dequeue();
            if(animationQueue.Count > 0) {
                animationQueue.Peek().Start();
            } else {
                OnAnimationsEnd?.Invoke(GameManager.Instance.CurrentTurn);
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
