using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShieldStrength {
    None,
    Weak,
    Medium,
    Strong,
    Invincible
}

public class Shield
{
    private int duration;
    private bool blocksStatus;
    private ShieldStrength strength;

    public float DamageMultiplier { get {
        switch(strength) {
            case ShieldStrength.None:
                return 1.0f;

            case ShieldStrength.Weak:
                return 0.25f;

            case ShieldStrength.Medium:
                return 0.5f;

            case ShieldStrength.Strong:
                return 0.75f;

            case ShieldStrength.Invincible:
                return 0f;
        }

        return 1.0f;
    } }
    
    public Shield() {

    }
}
