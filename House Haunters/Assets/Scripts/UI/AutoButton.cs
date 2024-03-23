using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class AutoButton : MonoBehaviour
{
    public enum ClickFunction {
        None,
        EndTurn
    }

    [SerializeField] private ClickFunction clickFunction;
    [SerializeField] private SpriteRenderer sprite;
    private bool hovered;

    public bool disabled;

    public Trigger OnClick;
    public Trigger OnHover;
    public Trigger OnMouseLeave;

    void Start() {
        switch(clickFunction) {
            case ClickFunction.EndTurn:
                OnClick = MenuManager.Instance.EndTurn;
                break;
        }
    }

    void Update() {
        Vector2 mousePos = InputManager.Instance.GetMousePosition();
        bool nowHovered = Global.GetObjectArea(gameObject).Contains(mousePos);
        if(!hovered && nowHovered && OnHover != null) {
            OnHover();
        } 
        else if(hovered && !nowHovered && OnMouseLeave != null) {
             OnMouseLeave();
        }

        hovered = nowHovered;

        if(!disabled && hovered && InputManager.Instance.SelectPressed()) {
            OnClick();
        }

        if(disabled) {
            sprite.color = Color.gray;
        } else {
            sprite.color = hovered ? Color.blue : Color.white;
        }
    }
}
