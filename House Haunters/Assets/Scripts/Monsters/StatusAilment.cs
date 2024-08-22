using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum StatusEffect
{
    Wither,
    Strength,
    Swiftness,
    Fear,
    Slowness,
    Haunted
}

public class StatusAilment : IEquatable<StatusAilment>
{
    public const int SPEED_BOOST = 1;

    public static StatusEffect[] negativeStatuses = new StatusEffect[] { 
        StatusEffect.Wither, 
        StatusEffect.Fear,
        StatusEffect.Slowness,
        StatusEffect.Haunted
    };

    public GameObject visual;
    public List<StatusEffect> effects;
    public int duration;

    public StatusAilment(StatusEffect effect, int duration, GameObject visualPrefab) : this(new List<StatusEffect>(){ effect }, duration, visualPrefab) {}
    public StatusAilment(List<StatusEffect> effects, int duration, GameObject visualPrefab) {
        this.effects = effects;
        this.duration = duration;
        visual = visualPrefab;
    }

    public bool Equals(StatusAilment other) {
        return this.visual == other.visual;
    }

    public void Terminate() {
        AnimationsManager.Instance.QueueAnimation(new DestructionAnimator(visual));
    }
}
