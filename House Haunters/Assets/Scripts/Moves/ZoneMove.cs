using System;
using System.Collections.Generic;
using UnityEngine;

class ZoneMove : Move
{
    public TileEffect Effect { get; private set; }

    public ZoneMove(string name, int cooldown, ISelector selector, TileEffect effect, AnimationQueuer effectAnimation, string description = "") 
        : base(name, cooldown, MoveType.Zone, Targets.UnaffectedFloor, selector, effectAnimation, description)
    {
        Effect = effect;
    }

    protected override void ApplyEffect(Monster user, Vector2Int tile) {
        TileAffector.ApplyTileEffect(user.Controller, Effect, tile);
    }
}
