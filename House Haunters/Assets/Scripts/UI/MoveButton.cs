using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveButton : MonoBehaviour
{
    private SpriteRenderer sprite;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    public void SetHovered(bool hovered) {
        sprite.color = hovered ? Color.blue : Color.white;
    }

    public bool IsWithin(Vector2 position) {
        Vector2 middle = transform.position;
        Vector2 scale = transform.lossyScale;
        return new Rect(middle - scale / 2, scale).Contains(position);
    }
}
