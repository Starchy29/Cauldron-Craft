using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a button that only conveys whether or not it is hovered
public class ControlledButton : MonoBehaviour
{
    private SpriteRenderer sprite;
    private bool hovered;
    private bool disabled;

    public bool Hovered {
        get { return hovered; }
        set { hovered = value; UpdateColor(); }
    }

    public bool Disabled {
        get { return disabled; }
        set { disabled = value; UpdateColor(); }
    }

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    private void UpdateColor() {
        if(disabled) {
            sprite.color = Color.gray;
        }
        else if(hovered) {
            sprite.color = Color.blue;
        }
        else {
            sprite.color = Color.white;
        }
    }

    public bool IsHovered(Vector2 position) {
        Vector2 middle = transform.position;
        Vector2 scale = transform.lossyScale;
        return new Rect(middle - scale / 2, scale).Contains(position);
    }
}
