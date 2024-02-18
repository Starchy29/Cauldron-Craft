using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarAnimator : IMoveAnimator
{
    public bool Completed { get { return endPause <= 0; } }

    private float endPause = 0.4f;
    private HealthBarScript healthBar;

    public HealthBarAnimator(HealthBarScript healthBar) {
        this.healthBar = healthBar;
    }

    public void Start() {
        healthBar.gameObject.SetActive(true);
    }

    public void Update(float deltaTime) {
        if(healthBar.IsAccurate) {
            endPause -= deltaTime;
            if(endPause <= 0) {
                healthBar.gameObject.SetActive(false);
            }
        } else {
            healthBar.UpdateDisplay(deltaTime);
        }
    }
}
