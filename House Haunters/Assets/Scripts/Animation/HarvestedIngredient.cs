using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestedIngredient : MonoBehaviour
{
    private const float DURATION = 1f;
    private const float THREE_SPINS = 360f * 3f;

    private Vector3 startPos;
    private Vector3 endPos;
    private float endRot;
    private float quadA; // b = -a;

    private float t;

    void Start() {
        startPos = transform.position;
        endPos = startPos + new Vector3(Random.value * 2f - 1f, 0, 0); // -1 to 1
        endRot = Random.value * 2 * THREE_SPINS - THREE_SPINS; // up to three spins in either direction
        float maxHeight = Random.value + 0.7f; // 0.7 - 1.7f
        quadA = -4 * maxHeight;
    }

    void Update() {
        t += Time.deltaTime / DURATION;
        if(t > 1f) {
            Destroy(gameObject);
            return;
        }

        float x = startPos.x + t * (endPos.x - startPos.x);
        float y = startPos.y + quadA * t * t - quadA * t; // (-4H)t^2 + (4H)t = y;
        float r = t * endRot;

        transform.position = new Vector3(x, y, 0);
        transform.rotation = Quaternion.Euler(0, 0, r);
    }
}
