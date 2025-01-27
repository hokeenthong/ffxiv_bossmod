﻿using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.Ultimate.DSW1
{
    class Heavensflame : CommonComponents.CastCounter
    {
        private int[] _playerIcons = new int[PartyState.MaxSize]; // 0 = unassigned, 1 = circle/red, 2 = triangle/green, 3 = cross/blue, 4 = square/purple - matching waypoint colors...
        private Actor? _knockbackSource;
        private bool _active;

        private static float _knockbackDistance = 16;
        private static float _aoeRadius = 10;
        private static float _tetherBreakDistance = 32; // TODO: verify...

        public Heavensflame() : base(ActionID.MakeSpell(AID.HeavensflameAOE)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (!_active)
                return;

            var actorAdjPos = BossModule.AdjustPositionForKnockback(actor.Position, _knockbackSource, _knockbackDistance);
            if (_knockbackSource != null && !module.Arena.InBounds(actorAdjPos))
                hints.Add("About to be knocked into wall!");

            if (module.Raid.WithoutSlot().Exclude(actor).Any(p => GeometryUtils.PointInCircle(BossModule.AdjustPositionForKnockback(p.Position, _knockbackSource, _knockbackDistance) - actorAdjPos, _aoeRadius)))
                hints.Add("Spread!");

            int partner = FindPartner(slot);
            if (partner >= 0 && GeometryUtils.PointInCircle(BossModule.AdjustPositionForKnockback(module.Raid[partner]!.Position, _knockbackSource, _knockbackDistance) - actorAdjPos, _tetherBreakDistance))
                hints.Add("Aim to break tether!");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            int partner = FindPartner(pcSlot);
            if (_playerIcons[pcSlot] != 0)
            {
                DrawPreferredLocation(module, (Waymark)((int)Waymark.A + (_playerIcons[pcSlot] - 1)));
                DrawPreferredLocation(module, (Waymark)((int)Waymark.N1 + (_playerIcons[pcSlot] - 1)));
            }

            if (partner >= 0)
            {
                arena.Actor(module.Raid[partner], arena.ColorPlayerInteresting);
                arena.AddLine(pc.Position, module.Raid[partner]!.Position, arena.ColorSafe);
            }

            if (!_active)
                return;

            if (_knockbackSource != null)
            {
                var adjPos = BossModule.AdjustPositionForKnockback(pc.Position, _knockbackSource, _knockbackDistance);
                arena.Actor(adjPos, 0, arena.ColorDanger);
                arena.AddLine(pc.Position, adjPos, arena.ColorDanger);
            }

            foreach (var (slot, player) in module.Raid.WithSlot().Exclude(pc))
            {
                if (slot != partner)
                    arena.Actor(player, arena.ColorPlayerGeneric);
                arena.AddCircle(BossModule.AdjustPositionForKnockback(player.Position, _knockbackSource, _knockbackDistance), _aoeRadius, arena.ColorDanger);
            }
        }

        public override void OnUntethered(BossModule module, Actor actor)
        {
            SetIcon(module, actor.InstanceID, 0);
            SetIcon(module, actor.Tether.Target, 0);
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.FaithUnmoving))
            {
                _knockbackSource = actor;
                _active = true;
            }
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.FaithUnmoving))
            {
                _knockbackSource = null;
            }
        }

        public override void OnEventIcon(BossModule module, ulong actorID, uint iconID)
        {
            int icon = (IconID)iconID switch
            {
                IconID.HeavensflameCircle => 1,
                IconID.HeavensflameTriangle => 2,
                IconID.HeavensflameCross => 3,
                IconID.HeavensflameSquare => 4,
                _ => 0
            };
            if (icon != 0)
                SetIcon(module, actorID, icon);
        }

        private void SetIcon(BossModule module, ulong actorID, int icon)
        {
            var slot = module.Raid.FindSlot(actorID);
            if (slot >= 0)
                _playerIcons[slot] = icon;
        }

        private int FindPartner(int slot)
        {
            if (_playerIcons[slot] == 0)
                return -1;
            for (int i = 0; i < _playerIcons.Length; ++i)
                if (i != slot && _playerIcons[i] == _playerIcons[slot])
                    return i;
            return -1;
        }

        private void DrawPreferredLocation(BossModule module, Waymark wm)
        {
            var pos = module.WorldState.Waymarks[wm];
            if (pos != null)
            {
                module.Arena.AddCircle(pos.Value, 2, module.Arena.ColorSafe);
                //var dir = Vector3.Normalize(pos.Value - _knockbackSource.Position);
                //var adjPos = module.Arena.ClampToBounds(_knockbackSource.Position + 50 * dir);
                //module.Arena.AddLine(module.Arena.WorldCenter, adjPos, module.Arena.ColorSafe);
            }
        }
    }
}
