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
    private bool disabled;

    public bool Disabled { 
        get {  return disabled; }
        set { 
            disabled = value;
            sprite.color = disabled ? Color.gray : sprite.color = Color.white;
        }
    }

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
        if(!hovered && nowHovered) {
            if(!disabled) {
                sprite.color = Color.blue;
            }

            if(OnHover != null) {
                OnHover();
            }
        } 
        else if(hovered && !nowHovered) {
            if(!disabled) {
                sprite.color = Color.white;
            }

            if(OnMouseLeave != null) {
                OnMouseLeave();
            }
        }

        hovered = nowHovered;

        if(!disabled && hovered && InputManager.Instance.SelectPressed()) {
            OnClick();
        }
    }
}
