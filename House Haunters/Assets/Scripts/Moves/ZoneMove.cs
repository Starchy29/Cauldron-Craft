using System;
using System.Collections.Generic;
using UnityEngine;

class ZoneMove : Move
{
    public TileAffector Effect { get; private set; }

    public ZoneMove(string name, int cooldown, ISelector selector, TileAffector effect, AnimationQueuer effectAnimation, string description = "") 
        : base(name, cooldown, MoveType.Zone, Targets.ZonePlaceable, selector, effectAnimation, description)
    {
        Effect = effect;
    }

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        TileAffector.ApplyEffect(Effect, user.Controller, tile);
    }
}
