﻿using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.P4S1
{
    using static BossModule;

    // state related to elemental belone mechanic (3 of 4 corners exploding)
    class ElementalBelone : Component
    {
        public bool Visible = false;
        private SettingTheScene.Element _safeElement;
        private List<Vector3> _imminentExplodingCorners = new();

        public override void Init(BossModule module)
        {
            var assignments = module.FindComponent<SettingTheScene>()!;
            uint forbiddenCorners = 1; // 0 corresponds to 'unknown' corner
            foreach (var actor in module.WorldState.Actors.Where(a => a.OID == (uint)OID.Helper).Tethered(TetherID.Bloodrake))
                forbiddenCorners |= 1u << (int)assignments.FromPos(module, actor.Position);
            var safeCorner = (SettingTheScene.Corner)BitOperations.TrailingZeroCount(~forbiddenCorners);
            _safeElement = assignments.FindElement(safeCorner);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_imminentExplodingCorners.Where(p => GeometryUtils.PointInRect(actor.Position - p, Vector3.UnitX, 10, 10, 10)).Any())
            {
                hints.Add($"GTFO from exploding square");
            }
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add($"Safe square: {_safeElement}");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (Visible)
            {
                var assignments = module.FindComponent<SettingTheScene>()!;
                var safeCorner = assignments.Assignment(_safeElement);
                if (safeCorner != SettingTheScene.Corner.Unknown)
                {
                    var p = module.Arena.WorldCenter + 10 * assignments.Direction(safeCorner);
                    arena.ZoneQuad(p, Vector3.UnitX, 10, 10, 10, arena.ColorSafeFromAOE);
                }
            }
            foreach (var p in _imminentExplodingCorners)
            {
                arena.ZoneQuad(p, Vector3.UnitX, 10, 10, 10, arena.ColorAOE);
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo!.Action.ID)
            {
                case AID.PeriaktoiDangerAcid:
                case AID.PeriaktoiDangerLava:
                case AID.PeriaktoiDangerWell:
                case AID.PeriaktoiDangerLevinstrike:
                    _imminentExplodingCorners.Add(actor.Position);
                    break;
            }
        }
    }
}
