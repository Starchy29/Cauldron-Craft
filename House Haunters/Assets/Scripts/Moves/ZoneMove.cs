using System;
using System.Collections.Generic;
using UnityEngine;

class ZoneMove : Move
{
    public TileAffector TileEffect { get; private set; }

    public ZoneMove(string name, int cooldown, ISelector selector, TileAffector effect, AnimationQueuer effectAnimation, string description = "") 
        : base(name, cooldown, DetermineMoveType(effect) , Targets.ZonePlaceable, selector, null, effectAnimation, description)
    {
        TileEffect = effect;
        ApplyEffect = PlaceZone;
    }

    private void PlaceZone(Monster user, Vector2Int tile) {
        TileAffector.ApplyEffect(TileEffect, user.Controller, tile);
    }

    private static MoveType DetermineMoveType(TileAffector effect) {
        if(effect.AppliedStatus.HasValue) {
            if(effect.AppliedStatus.Value == StatusEffect.Regeneration) {
                return MoveType.Heal;
            }

            if(effect.AppliedStatus.Value == StatusEffect.Poison) {
                return MoveType.Poison;
            }

            return StatusAilment.negativeStatuses.Contains(effect.AppliedStatus.Value) ? MoveType.Disrupt : MoveType.Boost;
        }

        return MoveType.Terrain;
    }
}
