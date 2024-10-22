using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearanceAnimator : IMoveAnimator
{
    public bool Completed { get; private set; }

    private GameObject appearer;
    private bool visible;

    public AppearanceAnimator(GameObject appearer, bool visible) {
        this.appearer = appearer;
        this.visible = visible;
    }

    public void Start() {
        if(appearer != null) {
            appearer.SetActive(visible);
        }
        Completed = true;
    }

    public void Update(float deltaTime) { }
}
