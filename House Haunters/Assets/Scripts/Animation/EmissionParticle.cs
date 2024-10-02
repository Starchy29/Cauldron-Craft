using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionParticle : MonoBehaviour
{
    private const int CYCLES = 4;
    private const float MAX_HEIGHT = 2f;
    private const float MAX_SHIFT = 0.5f;
    private const float DURATION = 2f;

    private Vector3 startPos;
    private float t;
    private float cycleOffset;

    void Start() {
        Vector3 randomShift = new Vector3((Random.value - 0.5f) * 0.7f, (Random.value - 0.5f) * 0.7f, 0f);
        cycleOffset = Random.value * 2f * Mathf.PI;
        startPos = transform.position + randomShift;
        transform.position = startPos;
    }

    void Update() {
        t += Time.deltaTime / DURATION * CYCLES;
        if(t > DURATION) {
            Destroy(gameObject);
            return;
        }
        float rise = t / CYCLES * MAX_HEIGHT;
        float shift = Mathf.Sin(2f * Mathf.PI * t + cycleOffset) * rise / MAX_HEIGHT * MAX_SHIFT;
        transform.position = startPos + new Vector3(shift, rise, 0);
    }
}
