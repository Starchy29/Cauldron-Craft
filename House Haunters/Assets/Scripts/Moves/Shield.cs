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

    private static Dictionary<Strength, float> strengthMultipliers = new Dictionary<Strength, float>() { 
        { Strength.None, 1f },
        { Strength.Weak, 0.75f },
        { Strength.Medium, 0.5f },
        { Strength.Strong, 0.25f },
        { Strength.Invincible, 0f }
    };

    public delegate void BlockEffect(Monster attacker, Monster blocker);
    public BlockEffect OnBlock { get; private set; }

    public Strength StrengthLevel { get; private set; }
    public int Duration { get; set; }
    public GameObject Visual { get; private set; }
    public bool BlocksStatus { get; private set; }
    public bool BlocksOnce { get; private set; }
    public float DamageMultiplier { get { return strengthMultipliers[StrengthLevel]; } }

    public Shield(Strength strength, int duration, bool blocksStatus, bool blocksOnce, GameObject visual, BlockEffect blockEffect = null) {
        StrengthLevel = strength;
        Duration = duration;
        BlocksStatus = blocksStatus;
        BlocksOnce = blocksOnce;
        OnBlock = blockEffect;
        Visual = visual;
    }
}
