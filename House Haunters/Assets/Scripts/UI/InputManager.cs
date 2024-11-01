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
        if(Mouse.current == null) {
            return Vector2.zero;
        }
        
        return Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    public bool SelectPressed() {
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    public bool BackPressed() {
        return Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame;
    }

    public bool PausePressed() {
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }

    public bool SkipHeld() {
        return Mouse.current != null && Mouse.current.rightButton.isPressed
            || Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
    }
}
