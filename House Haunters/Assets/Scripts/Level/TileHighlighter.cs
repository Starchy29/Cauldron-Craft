using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileHighlighter : MonoBehaviour
{
    public enum State {
        Highlighted, // just for informatiom
        Selectable, // shows what can be selected
        Hovered, // shows that something will be selected if the mouse is clicked
        Selected
    }

    private const float ALPHA = 0.5f;

    private bool hovered;
    private bool highlighted;
    private bool selectable;
    private bool selected;
    private SpriteRenderer sprite;

    void Start() {
        sprite = GetComponent<SpriteRenderer>();
        SetState(State.Selected, false);
    }

    public void SetState(State state, bool activated) {
        switch(state) {
            case State.Selected:
                selected = activated;
                break;
            case State.Selectable:
                selectable = activated;
                break;
            case State.Highlighted:
                highlighted = activated;
                break;
            case State.Hovered:
                hovered = activated;
                break;
        }

        if(highlighted) {
            sprite.color = new Color(0.8f, 0.8f, 0.2f, ALPHA); // yellow
        }
        else if(selected) {
            sprite.color = new Color(0f, 0.8f, 0.2f, ALPHA); // green
        }
        else if(hovered) {
            sprite.color = new Color(0f, 0.2f, 0.8f, ALPHA); // blue
        }
        else if(selectable) {
            sprite.color = new Color(0.0f, 0.8f, 0.8f, ALPHA); // light blue
        }
        else {
            sprite.color = new Color(0f, 0f, 0f, 0f);
        }
    }
}
