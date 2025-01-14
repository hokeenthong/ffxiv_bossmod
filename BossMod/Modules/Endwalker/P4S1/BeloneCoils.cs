﻿using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.P4S1
{
    using static BossModule;

    // state related to belone coils mechanic (role towers)
    class BeloneCoils : Component
    {
        public enum Soaker { Unknown, TankOrHealer, DamageDealer }

        public Soaker ActiveSoakers { get; private set; } = Soaker.Unknown;
        private List<Actor> _activeTowers = new();

        private static float _towerRadius = 4;

        public bool IsValidSoaker(Actor player)
        {
            return ActiveSoakers switch
            {
                Soaker.TankOrHealer => player.Role == Role.Tank || player.Role == Role.Healer,
                Soaker.DamageDealer => player.Role == Role.Melee || player.Role == Role.Ranged,
                _ => false
            };
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (ActiveSoakers == Soaker.Unknown)
                return;

            bool isSoaking = _activeTowers.InRadius(actor.Position, _towerRadius).Any();
            if (IsValidSoaker(actor))
            {
                hints.Add("Soak the tower", !isSoaking);
            }
            else
            {
                hints.Add("GTFO from tower", isSoaking);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (ActiveSoakers == Soaker.Unknown)
                return;

            bool validSoaker = IsValidSoaker(pc);
            foreach (var tower in _activeTowers)
            {
                arena.AddCircle(tower.Position, _towerRadius, validSoaker ? arena.ColorSafe : arena.ColorDanger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.BeloneCoilsDPS) || actor.CastInfo!.IsSpell(AID.BeloneCoilsTH))
            {
                _activeTowers.Add(actor);
                ActiveSoakers = actor.CastInfo!.Action.ID == (uint)AID.BeloneCoilsDPS ? Soaker.DamageDealer : Soaker.TankOrHealer;
            }
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.BeloneCoilsDPS) || actor.CastInfo!.IsSpell(AID.BeloneCoilsTH))
            {
                _activeTowers.Remove(actor);
                if (_activeTowers.Count == 0)
                    ActiveSoakers = Soaker.Unknown;
            }
        }
    }
}
