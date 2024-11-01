using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private const float BOUNDS_EXTENDS = 1f;
    private const float HALF_HEIGHT = 5.625f;
    private const float HALF_WIDTH = HALF_HEIGHT * 16f/9f;
    private Vector2? mouseAnchor;

    public static CameraController Instance { get; private set; }
    private void Awake() {
        Instance = this;
    }

    void Update() {
        if(GameManager.Instance.CurrentTurn.AI == null) {
            PlayerUpdate();
        }

        // BUILD DEBUG
        Vector3 direction = Vector3.zero;
        if(Keyboard.current.leftArrowKey.isPressed) {
            direction.x -= 1;
        }
        if(Keyboard.current.rightArrowKey.isPressed) {
            direction.x += 1;
        }
        if(Keyboard.current.upArrowKey.isPressed) {
            direction.y += 1;
        }
        if(Keyboard.current.downArrowKey.isPressed) {
            direction.y -= 1;
        }

        transform.position += 10 * Time.deltaTime * direction;
        // END DEBUG
    }
    private void PlayerUpdate() {
        if(Mouse.current == null || CameraAnimator.CameraLocked) {
            mouseAnchor = null;
            return;
        }
        
        if(Mouse.current.rightButton.wasPressedThisFrame || Mouse.current.rightButton.isPressed && !mouseAnchor.HasValue) {
            mouseAnchor = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
        else if(Mouse.current.rightButton.isPressed) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector3 mouseDelta = mousePos - mouseAnchor.Value;
            mouseDelta.x = 0;
            transform.position -= mouseDelta;
            transform.position = ClampToLevel(transform.position);
        }
    }

    public Vector3 ClampToLevel(Vector3 point) {
        LevelGrid level = LevelGrid.Instance;
        return new Vector3(
            level.Width / 2f, //Mathf.Clamp(transform.position.x, HALF_WIDTH - BOUNDS_EXTENDS, level.Width + BOUNDS_EXTENDS - HALF_WIDTH),
            Mathf.Clamp(point.y, HALF_HEIGHT - BOUNDS_EXTENDS, level.Height + BOUNDS_EXTENDS - HALF_HEIGHT),
            point.z
        );
    }
}
