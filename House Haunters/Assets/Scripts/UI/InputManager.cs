using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager
{
    private static InputManager instance;
    public static InputManager Instance { get { 
        if(instance == null) {
            instance = new InputManager();
        }
        return instance;
    } }

    private InputManager() { }

    // gets the world position of the mouse
    public Vector2 GetMousePosition() {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public bool SelectPressed() {
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    public bool BackPressed() {
        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame
            || Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.backspaceKey.wasPressedThisFrame);
    }
}
