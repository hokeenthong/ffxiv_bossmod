﻿using System;
using System.Linq;

namespace BossMod.Endwalker.P2S
{
    using static BossModule;

    // state related to cataract mechanic
    class Cataract : Component
    {
        private AOEShapeRect _aoeBoss = new(50, 7.5f, 50);
        private AOEShapeRect _aoeHead = new(50, 50);

        public override void Init(BossModule module)
        {
            if (module.PrimaryActor.CastInfo?.IsSpell(AID.WingedCataract) ?? false)
                _aoeHead.DirectionOffset = MathF.PI;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_aoeBoss.Check(actor.Position, module.PrimaryActor) || _aoeHead.Check(actor.Position, module.Enemies(OID.CataractHead).FirstOrDefault()))
                hints.Add("GTFO from cataract!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            _aoeBoss.Draw(arena, module.PrimaryActor);
            _aoeHead.Draw(arena, module.Enemies(OID.CataractHead).FirstOrDefault());
        }
    }
}
