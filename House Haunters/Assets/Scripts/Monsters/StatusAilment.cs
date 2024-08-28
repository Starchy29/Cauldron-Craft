using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum StatusEffect
{
    Poison,
    Power,
    Swift,
    Fear,
    Slowness,
    Haunt,
    Sturdy,
    Cursed
}

public class StatusAilment : IEquatable<StatusAilment>
{
    public GameObject visual;
    public StatusEffect effect;
    public int duration;

    public StatusAilment(StatusEffect effect, int duration, GameObject visualPrefab) {
        this.effect = effect;
        this.duration = duration;
        visual = visualPrefab;
    }

    public bool Equals(StatusAilment other) {
        return this.visual == other.visual;
    }

    public void Terminate() {
        AnimationsManager.Instance.QueueAnimation(new DestructionAnimator(visual));
    }

    public static bool IsPositive(StatusEffect effect) {
        switch(effect) {
            case StatusEffect.Power:
            case StatusEffect.Swift:
            case StatusEffect.Sturdy:
            case StatusEffect.Cursed:
                return true;
            default:
                return false;
        }
    }
}
