using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathAnimator : IMoveAnimator
{
    public bool Completed => completed;
    private bool completed;

    private Monster dead;

    public DeathAnimator(Monster dead) {
        this.dead = dead;
    }

    public void Start() {
        
    }

    public void Update(float deltaTime) {
        completed = true;
        GameObject.Destroy(dead.gameObject);
    }
}
