using System;
using System.Collections.Generic;
using UnityEngine;

class ZoneMove : Move
{
    public TileAffector TileEffect { get; private set; }

    public ZoneMove(string name, int cooldown, ISelector selector, TileAffector effect, AnimationFunction effectAnimation, string description = "") 
        : base(name, cooldown, MoveType.Terrain, Targets.ZonePlaceable, selector, null, effectAnimation, description)
    {
        TileEffect = effect;
        ApplyEffect = PlaceZone;
    }

    private void PlaceZone(Monster user, Vector2Int tile) {
        TileAffector.ApplyEffect(TileEffect, user.Controller, tile);
    }
}
