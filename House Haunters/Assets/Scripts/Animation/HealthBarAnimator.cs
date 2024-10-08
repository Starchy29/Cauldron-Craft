using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarAnimator : IMoveAnimator
{
    public bool Completed { get { return endPause <= 0; } }

    private float endPause;
    private HealthBarScript healthBar;
    private int targetHealth;
    private bool movingBar;

    public HealthBarAnimator(HealthBarScript healthBar, int targetHealth) {
        this.healthBar = healthBar;
        this.targetHealth = targetHealth;
        endPause = 0.8f;
        movingBar = true;
    }

    public void Start() {
        healthBar.gameObject.SetActive(true);
    }

    public void Update(float deltaTime) {
        if(movingBar) {
            healthBar.UpdateDisplay(deltaTime, targetHealth);
            if(healthBar.RepresentedHealth == targetHealth) {
                movingBar = false;
            }
        } else {
            endPause -= deltaTime;
            if(endPause <= 0) {
                healthBar.gameObject.SetActive(false);
                healthBar.MarkTrace();
            }
        }
    }
}
