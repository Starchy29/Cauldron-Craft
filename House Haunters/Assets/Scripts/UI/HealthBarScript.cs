using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField] private Monster tracked;
    [SerializeField] private GameObject VisualBar;
    [SerializeField] private GameObject traceBar;
    private SpriteRenderer barColor;
    public bool IsAccurate { get { return currentWidth == DetermineWidth(tracked.Health); } }
    public int RepresentedHealth { get; private set; }

    private float maxWidth;
    private float localLeft;
    private float currentWidth;

    void Awake() {
        maxWidth = VisualBar.transform.localScale.x;
        currentWidth = maxWidth;
        localLeft = VisualBar.transform.localPosition.x - maxWidth / 2f;
        barColor = VisualBar.GetComponent<SpriteRenderer>();
        barColor.color = Color.cyan;
        RepresentedHealth = -1;
    }

    public void UpdateDisplay(float deltaTime, int targetHealth) {
        float targetWidth = DetermineWidth(targetHealth);
        float difference = targetWidth - currentWidth;
        if(difference == 0) {
            RepresentedHealth = targetHealth;
            return;
        }

        float change = deltaTime * (Mathf.Sign(difference) * 0.3f + 5f * difference / maxWidth);
        if(Mathf.Abs(change) > Mathf.Abs(difference)) {
            currentWidth = targetWidth;
            RepresentedHealth = targetHealth;
        } else {
            currentWidth += change;
        }

        // update visual bar
        SetWidth(VisualBar);
    }

    public void MarkTrace() {
        SetWidth(traceBar);
    }

    private void SetWidth(GameObject bar) {
        Vector3 scale = bar.transform.localScale;
        scale.x = currentWidth;
        bar.transform.localScale = scale;
        Vector3 position = bar.transform.localPosition;
        position.x = localLeft + currentWidth / 2f;
        bar.transform.localPosition = position;
    }

    private float DetermineWidth(int health) {
        return maxWidth * health / tracked.Stats.Health;
    }
}
