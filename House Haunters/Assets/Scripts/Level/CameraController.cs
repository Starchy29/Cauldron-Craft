using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private const float BOUNDS_EXTENDS = 0.8f;
    private const float HALF_HEIGHT = 5.625f;
    private const float HALF_WIDTH = HALF_HEIGHT * 16f/9f;
    private Vector2 mouseAnchor;

    void Update() {
        if(GameManager.Instance.CurrentTurn.AI == null) {
            PlayerUpdate();
        } else {
            AIUpdate();
        }
    }

    private void PlayerUpdate() {
        if(Mouse.current == null) {
            return;
        }

        if(Mouse.current.rightButton.wasPressedThisFrame) {
            mouseAnchor = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
        else if(Mouse.current.rightButton.isPressed) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector3 mouseDelta = mousePos - mouseAnchor;
            mouseDelta.x = 0;
            transform.position -= mouseDelta;
            LevelGrid level = LevelGrid.Instance;
            transform.position = new Vector3(
                transform.position.x, //Mathf.Clamp(transform.position.x, HALF_WIDTH - BOUNDS_EXTENDS, level.Width + BOUNDS_EXTENDS - HALF_WIDTH),
                Mathf.Clamp(transform.position.y, HALF_HEIGHT - BOUNDS_EXTENDS, level.Height + BOUNDS_EXTENDS - HALF_HEIGHT),
                transform.position.z
            );
        }
    }

    private void AIUpdate() {

    }
}
