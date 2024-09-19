using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// pans the camera to a certain point of focus
public class CameraAnimator : IMoveAnimator
{
    private const float MIN_SPEED = 1f;

    public static bool CameraLocked;
    public bool Completed { get; private set; }

    private Vector2 targetPosition;
    private CameraController camera;

    public CameraAnimator(Vector2 targetPosition) {
        camera = CameraController.Instance;
        this.targetPosition = camera.ClampToLevel(targetPosition);
    }

    public void Start() {
        CameraLocked = true;    
    }

    public void Update(float deltaTime) {
        Vector2 toGoal = targetPosition - (Vector2)camera.transform.position;
        toGoal.x = 0f; // for the current level, only pan up and down

        if(toGoal.magnitude <= MIN_SPEED * Time.deltaTime) {
            Vector3 newPos = targetPosition;
            newPos.z = camera.transform.position.z;
            camera.transform.position = newPos;
            Completed = true;
            CameraLocked = false;
            return;
        }

        float speed = Mathf.Max(MIN_SPEED, toGoal.magnitude * 4.0f);
        camera.transform.position += (Vector3)(speed * Time.deltaTime * toGoal.normalized);
        camera.ClampToLevel(camera.transform.position);
    }
}
