﻿using System;
using System.Numerics;

namespace BossMod.Endwalker.ARanks.Aegeiros
{
    public enum OID : uint
    {
        Boss = 0x3671,
    };

    public enum AID : uint
    {
        AutoAttack = 872,
        Leafstorm = 27708,
        Rimestorm = 27709,
        Snowball = 27710,
        Canopy = 27711,
        BackhandBlow = 27712,
    }

    public class Mechanics : BossModule.Component
    {
        private bool _showRimestorm;
        private AOEShapeCircle _leafstorm = new(10);
        private AOEShapeCone _rimestorm = new(40, MathF.PI / 2);
        private AOEShapeCircle _snowball = new(8);
        private AOEShapeCone _backhandBlow = new(12, MathF.PI / 3, MathF.PI);

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            bool inAOE = _showRimestorm && _rimestorm.Check(actor.Position, module.PrimaryActor);
            if (!inAOE)
            {
                var (aoe, pos) = ActiveAOE(module, actor);
                inAOE = aoe?.Check(actor.Position, pos, module.PrimaryActor.Rotation) ?? false;
            }
            if (inAOE)
                hints.Add("GTFO from aoe!");
        }

        public override void AddGlobalHints(BossModule module, BossModule.GlobalHints hints)
        {
            if (!(module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return;

            string hint = (AID)module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.Leafstorm or AID.Rimestorm or AID.Snowball or AID.BackhandBlow => "Avoidable AOE",
                _ => "",
            };
            if (hint.Length > 0)
                hints.Add(hint);
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_showRimestorm)
                _rimestorm.Draw(arena, module.PrimaryActor);

            var (aoe, pos) = ActiveAOE(module, pc);
            aoe?.Draw(arena, pos, module.PrimaryActor.Rotation);
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor == module.PrimaryActor && actor.CastInfo!.IsSpell(AID.Leafstorm))
                _showRimestorm = true;
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (actor == module.PrimaryActor && actor.CastInfo!.IsSpell(AID.Rimestorm))
                _showRimestorm = false;
        }

        private (AOEShape?, Vector3) ActiveAOE(BossModule module, Actor pc)
        {
            if (!(module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return (null, new());

            return (AID)module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.Leafstorm => (_leafstorm, module.PrimaryActor.Position),
                AID.Snowball => (_snowball, module.PrimaryActor.CastInfo.Location),
                AID.BackhandBlow => (_backhandBlow, module.PrimaryActor.Position),
                _ => (null, new())
            };
        }
    }

    public class Aegeiros : SimpleBossModule
    {
        public Aegeiros(BossModuleManager manager, Actor primary)
            : base(manager, primary)
        {
            BuildStateMachine<Mechanics>();
        }
    }
}
