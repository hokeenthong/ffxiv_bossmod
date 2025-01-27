﻿using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.P2S
{
    using static BossModule;

    // state related to predatory avarice mechanic
    class PredatoryAvarice : Component
    {
        private ulong _playersWithTides = 0;
        private ulong _playersWithDepths = 0;
        private ulong _playersInTides = 0;
        private ulong _playersInDepths = 0;

        private static float _tidesRadius = 10;
        private static float _depthsRadius = 6;

        public bool Active => (_playersWithTides | _playersWithDepths) != 0;

        public override void Update(BossModule module)
        {
            _playersInTides = _playersInDepths = 0;
            if (!Active)
                return;

            foreach ((int i, var player) in module.Raid.WithSlot())
            {
                if (BitVector.IsVector64BitSet(_playersWithTides, i))
                {
                    _playersInTides |= module.Raid.WithSlot().InRadiusExcluding(player, _tidesRadius).Mask();
                }
                else if (BitVector.IsVector64BitSet(_playersWithDepths, i))
                {
                    _playersInDepths |= module.Raid.WithSlot().InRadiusExcluding(player, _depthsRadius).Mask();
                }
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (!Active)
                return;

            if (BitVector.IsVector64BitSet(_playersWithTides, slot))
            {
                if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _tidesRadius).Any())
                {
                    hints.Add("GTFO from raid!");
                }
            }
            else
            {
                if (BitVector.IsVector64BitSet(_playersInTides, slot))
                {
                    hints.Add("GTFO from avarice!");
                }

                bool warnToStack = BitVector.IsVector64BitSet(_playersWithDepths, slot)
                    ? BitOperations.PopCount(_playersInDepths) < 6
                    : !BitVector.IsVector64BitSet(_playersInDepths, slot);
                if (warnToStack)
                {
                    hints.Add("Stack with raid!");
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (!Active)
                return;

            bool pcHasTides = BitVector.IsVector64BitSet(_playersWithTides, pcSlot);
            bool pcHasDepths = BitVector.IsVector64BitSet(_playersWithDepths, pcSlot);
            foreach ((int i, var actor) in module.Raid.WithSlot())
            {
                if (BitVector.IsVector64BitSet(_playersWithTides, i))
                {
                    // tides are always drawn
                    arena.AddCircle(actor.Position, _tidesRadius, arena.ColorDanger);
                    arena.Actor(actor, arena.ColorDanger);
                }
                else if (BitVector.IsVector64BitSet(_playersWithDepths, i) && !pcHasTides)
                {
                    // depths are drawn only if pc has no tides - otherwise it is to be considered a generic player
                    arena.AddCircle(actor.Position, _tidesRadius, arena.ColorSafe);
                    arena.Actor(actor, arena.ColorDanger);
                }
                else if (pcHasTides || pcHasDepths)
                {
                    // other players are only drawn if pc has some debuff
                    bool playerInteresting = BitVector.IsVector64BitSet(pcHasTides ? _playersInTides : _playersInDepths, i);
                    arena.Actor(actor.Position, actor.Rotation, playerInteresting ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                }
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.MarkOfTides:
                    ModifyDebuff(module, actor, ref _playersWithTides, true);
                    break;
                case SID.MarkOfDepths:
                    ModifyDebuff(module, actor, ref _playersWithDepths, true);
                    break;
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.MarkOfTides:
                    ModifyDebuff(module, actor, ref _playersWithTides, false);
                    break;
                case SID.MarkOfDepths:
                    ModifyDebuff(module, actor, ref _playersWithDepths, false);
                    break;
            }
        }

        private void ModifyDebuff(BossModule module, Actor actor, ref ulong vector, bool active)
        {
            int slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                BitVector.ModifyVector64Bit(ref vector, slot, active);
        }
    }
}
