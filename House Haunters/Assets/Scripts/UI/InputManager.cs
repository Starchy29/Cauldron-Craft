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

    public Vector2 GetMousePosition() {
        return Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    public bool SelectPressed() {
        return !AnimationsManager.Instance.Animating && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    public bool BackPressed() {
        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame
            || Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.backspaceKey.wasPressedThisFrame);
    }
}
