using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StretchType {
    Horizontal,
    Vertical
}

public class StretchAnimator : IMoveAnimator
{
    public bool Completed { get { return sequenceIndex >= sequence.Count; } }

    private GameObject stretcher;
    private List<StretchType> sequence;
    private float elementDuration;
    private Vector3 startScale;
    float maxStretch;

    private float t;
    private int sequenceIndex;

    public StretchAnimator(GameObject stretcher, List<StretchType> sequence, float maxStretch, float elementDuration) {
        this.stretcher = stretcher;
        this.sequence = sequence;
        this.elementDuration = elementDuration;
        this.maxStretch = maxStretch;
    }

    public void Start() {
        startScale = stretcher.transform.localScale;    
    }

    public void Update(float deltaTime) {
        t += deltaTime / elementDuration;
        if(t > 1f) {
            stretcher.transform.localScale = startScale;
            sequenceIndex++;
            t = 0f;
            return;
        }

        float stretchMultiplier = 1f + maxStretch - maxStretch * 2f * Mathf.Abs(t - 0.5f);
        float horizontalMult = sequence[sequenceIndex] == StretchType.Horizontal ? stretchMultiplier : 1f / stretchMultiplier;
        stretcher.transform.localScale = new Vector3(startScale.x * horizontalMult, startScale.y / horizontalMult, startScale.z);
    }
}
