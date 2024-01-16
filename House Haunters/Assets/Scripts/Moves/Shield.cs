using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield
{
    public enum Strength {
        None,
        Weak,
        Medium,
        Strong,
        Invincible
    }

    public Strength StrengthLevel { get; set; }
    public int Duration { get; set; }
    public bool BlocksStatus { get; set; }
    public bool BlocksOnce { get; set; }
    public float DamageMultiplier { get {
        switch(StrengthLevel) {
            case Shield.Strength.None:
                return 1.0f;
            case Shield.Strength.Weak:
                return 0.75f;
            case Shield.Strength.Medium:
                return 0.5f;
            case Shield.Strength.Strong:
                return 0.25f;
            case Shield.Strength.Invincible:
                return 0f;
        }

        return 1.0f;
    } }

    public Shield(Strength strength, int duration, bool blocksStatus, bool blocksOnce) {
        StrengthLevel = strength;
        Duration = duration;
        BlocksStatus = blocksStatus;
        BlocksOnce = blocksOnce;
    }
}
