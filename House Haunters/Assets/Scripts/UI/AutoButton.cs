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

    protected Color baseColor = new Color(0.7f, 0.7f, 0.7f);
    private Color disabledColor;
    private Color hoveredColor;

    void Start() {
        SetBackColor(baseColor);
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
            sprite.color = disabledColor;
        } else {
            sprite.color = hovered ? hoveredColor : baseColor;
        }
    }

    public void SetBackColor(Color color) {
        baseColor = color;
        disabledColor = Global.ChangeSaturation(Global.ChangeValue(color, -0.3f), -0.1f);
        hoveredColor = Global.ChangeSaturation(Global.ChangeValue(color, +0.1f), +0.3f);
    }
}
