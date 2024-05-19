using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructionAnimator : IMoveAnimator
{
    public bool Completed { get; private set; }

    private GameObject destroyed;
    private GameObject particlePrefab;

    public DestructionAnimator(GameObject destroyed, GameObject particlePrefab = null) {
        this.destroyed = destroyed;
        this.particlePrefab = particlePrefab;
    }

    public void Start() {
        if(particlePrefab != null) {
            GameObject.Instantiate(particlePrefab).transform.position = destroyed.transform.position;
        }
        GameObject.Destroy(destroyed);
        Completed = true;
    }

    public void Update(float deltaTime) {}
}
