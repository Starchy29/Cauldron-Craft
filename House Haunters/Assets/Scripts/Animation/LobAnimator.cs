using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobAnimator : IMoveAnimator
{
    public bool Completed { get; private set; }

    private GameObject visual;
    private Vector3 start;
    private Vector3 end;
    private float quadraticA;
    private float duration;

    private float t;

    public LobAnimator(GameObject prefab, Vector3 start, Vector3 end, float height, float duration) {
        visual = GameObject.Instantiate(prefab);
        visual.SetActive(false);
        this.start = start;
        this.end = end;
        quadraticA =  -4f * height; // -4hx^2 + 4hx = y
        this.duration = duration;
    }

    public void Start() {
        visual.SetActive(true);
        visual.transform.position = start;
    }

    public void Update(float deltaTime) {
        t += deltaTime / duration;
        if(t > 1f) {
            GameObject.Destroy(visual);
            Completed = true;
            return;
        }

        Vector3 prevPos = visual.transform.position;
        Vector3 groundPos = start * (1f - t) + t * end;
        Vector3 heightBoost = new Vector3(0f, quadraticA * t * t -quadraticA * t, 0f);
        visual.transform.position = groundPos + heightBoost;
        Vector3 direction = visual.transform.position - prevPos;
        visual.transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x));
    }
}
