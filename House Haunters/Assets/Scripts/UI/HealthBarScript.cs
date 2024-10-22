using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField] private Monster tracked;
    [SerializeField] private GameObject VisualBar;
    [SerializeField] private GameObject traceBar;
    [SerializeField] private GameObject predictBar;
    private SpriteRenderer barColor;
    public bool IsAccurate { get { return currentWidth == DetermineWidth(tracked.Health); } }
    public int RepresentedHealth { get; private set; }

    private float maxWidth;
    private float localLeft;
    private float currentWidth;

    private SpriteRenderer predictRender;
    private float predictTime;

    void Awake() {
        maxWidth = VisualBar.transform.localScale.x;
        currentWidth = maxWidth;
        localLeft = VisualBar.transform.localPosition.x - maxWidth / 2f;
        barColor = VisualBar.GetComponent<SpriteRenderer>();
        barColor.color = Color.cyan;
        RepresentedHealth = -1;
        predictBar.SetActive(false);
        predictRender = predictBar.GetComponent<SpriteRenderer>();
    }

    void Update() {
        if(predictBar.activeInHierarchy) {
            predictTime += Time.deltaTime * 5f;
            predictTime %= Mathf.PI * 2f;
            Color newColor = predictRender.color;
            newColor.a = Mathf.Abs(Mathf.Cos(predictTime));
            predictRender.color = newColor;
        }
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

    public void HidePredict() {
        predictBar.SetActive(false);
    }

    public void PredictDamage(Monster attacker, Monster defender, int baseDamage) {
        predictTime = 0f;
        predictBar.SetActive(true);

        int damage = defender.DetermineDamage(baseDamage, attacker);
        if(damage > defender.Health) {
            damage = defender.Health;
        }
        float width = DetermineWidth(damage);
        
        Vector3 scale = predictBar.transform.localScale;
        scale.x = width;
        predictBar.transform.localScale = scale;

        float localHealthRight = VisualBar.transform.localPosition.x + VisualBar.transform.localScale.x / 2f;

        Vector3 position = predictBar.transform.localPosition;
        position.x = localHealthRight - width / 2f;
        predictBar.transform.localPosition = position;
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
