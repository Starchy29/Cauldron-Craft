using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialAnimator : IMoveAnimator
{
    public bool Completed { get; private set; }

    private Monster user;
    private RadialParticle particle;
    private float t;
    private float duration;
    private bool oscillate;

    public RadialAnimator(GameObject particlePrefab, Monster user, float duration, bool oscillate, Color color = new Color()) {
        particle = GameObject.Instantiate(particlePrefab).GetComponent<RadialParticle>();
        this.user = user;
        this.duration = duration;
        this.oscillate = oscillate;
        particle.SetColor(color);
        particle.gameObject.SetActive(false);
    }

    public void Start() {
        particle.transform.position = user.SpriteModel.transform.position;
        particle.gameObject.SetActive(true);
    }

    public void Update(float deltaTime) {
        t += deltaTime / duration;
        if(oscillate) {
            float radius = 0f;
            if(t < 1f/6f || t > 5f/6f) {
                radius = -6f * Mathf.Abs(t - 0.5f) + 3f;
            } else {
                radius = t;
                if(t > 0.5f) {
                    radius -= 1f/3f;
                }

                const float LOCAL_MIN = 0.8f;
                radius = 6f * (1f - LOCAL_MIN) * Mathf.Abs(radius - 1f/3f) + LOCAL_MIN;
            }

            radius *= radius;
            radius /= 2f;
            particle.SetRadius(radius);
        } else {
            particle.SetRadius(t / 2f);
        }

        if(t >= 1f) {
            GameObject.Destroy(particle.gameObject);
            Completed = true;
        }
    }
}
