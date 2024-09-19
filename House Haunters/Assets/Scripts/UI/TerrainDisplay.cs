using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainDisplay : MonoBehaviour
{
    public SpriteRenderer ColorBack;
    public TMPro.TextMeshPro DurationLabel;

    float startY;
    float time;

    void FixedUpdate() {
        time += Time.deltaTime * 2f;
        time %= 2 * Mathf.PI;
        
        Vector3 newPos = transform.localPosition;
        newPos.y = startY + 0.1f * Mathf.Sin(time);
        transform.localPosition = newPos;
    }
}
