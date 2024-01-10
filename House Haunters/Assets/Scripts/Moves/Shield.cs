using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : Move
{
    public enum StrengthLevel {
        None,
        Weak,
        Medium,
        Strong,
        Invincible
    }

    public int Duration { get; private set; }
    public StrengthLevel Strength { get; private set; }
    public bool BlocksStatus { get; private set; }
    public bool BlocksOnce { get; private set; }

    public float DamageMultiplier { get {
        switch(Strength) {
            case StrengthLevel.None:
                return 1.0f;
            case StrengthLevel.Weak:
                return 0.25f;
            case StrengthLevel.Medium:
                return 0.5f;
            case StrengthLevel.Strong:
                return 0.75f;
            case StrengthLevel.Invincible:
                return 0f;
        }

        return 1.0f;
    } }
    
    public Shield(int cost, Selector selection) : base(cost, Move.Targets.Allies, selection) {

    }

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        
    }
}
