using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearanceAnimator : IMoveAnimator
{
    public bool Completed { get; private set; }

    private GameObject appearer;

    public AppearanceAnimator(GameObject appearer) {
        this.appearer = appearer;
    }

    public void Start() {
        appearer.SetActive(true);
        Completed = true;
    }

    public void Update(float deltaTime) { }
}
