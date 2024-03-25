using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoverer : MonoBehaviour
{
    [SerializeField] private float range;
    [SerializeField] private float speed;

    private float time;
    private float startY;

    void Start()
    {
        startY = transform.localPosition.y;
    }

    void Update() {
        time += Time.deltaTime * speed;
        time %= 2 * Mathf.PI;
        
        Vector3 newPos = transform.localPosition;
        newPos.y = startY + range * Mathf.Sin(time);
        transform.localPosition = newPos;
    }
}
