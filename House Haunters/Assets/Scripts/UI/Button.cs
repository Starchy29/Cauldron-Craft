using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    [SerializeField] private ButtonFunctionName effect;

    private Rect area;
    private Trigger clickEffect;

    void Start() {
        Vector2 scale = transform.localScale;
        Vector2 middle = transform.position;
        area = new Rect(middle - scale / 2, scale);

    }

    void Update() {
        
    }
}
