﻿using System;
using System.Linq;

namespace BossMod.Endwalker.HydaelynEx
{
    using static BossModule;

    // component for infralateral arc mechanic (role stacks)
    class InfralateralArc : CommonComponents.CastCounter
    {
        private static float _coneHalfAngle = MathF.PI / 4;

        public InfralateralArc() : base(ActionID.MakeSpell(AID.InfralateralArcAOE)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var pcRole = EffectiveRole(actor);
            var pcDir = GeometryUtils.DirectionFromVec3(actor.Position - module.PrimaryActor.Position);
            if (module.Raid.WithoutSlot().Any(a => EffectiveRole(a) != pcRole && GeometryUtils.PointInCone(a.Position - module.PrimaryActor.Position, pcDir, _coneHalfAngle)))
                hints.Add("Spread by roles!");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var pcRole = EffectiveRole(pc);
            var pcDir = GeometryUtils.DirectionFromVec3(pc.Position - module.PrimaryActor.Position);
            foreach (var actor in module.Raid.WithoutSlot().Where(a => EffectiveRole(a) != pcRole))
                arena.Actor(actor, GeometryUtils.PointInCone(actor.Position - module.PrimaryActor.Position, pcDir, _coneHalfAngle) ? arena.ColorDanger : arena.ColorPlayerGeneric);
        }

        private Role EffectiveRole(Actor a) =>  a.Role == Role.Ranged ? Role.Melee : a.Role;
    }
}
