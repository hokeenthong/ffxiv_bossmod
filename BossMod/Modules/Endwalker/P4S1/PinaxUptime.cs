﻿using System.Numerics;

namespace BossMod.Endwalker.P4S1
{
    using static BossModule;

    // component showing where to drag boss for max pinax uptime
    class PinaxUptime : Component
    {
        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (pc.Role != Role.Tank)
                return;

            // draw position between lighting and fire squares
            var assignments = module.FindComponent<SettingTheScene>()!;
            var doubleOffset = assignments.Direction(assignments.Assignment(SettingTheScene.Element.Fire)) + assignments.Direction(assignments.Assignment(SettingTheScene.Element.Lightning));
            if (doubleOffset == Vector3.Zero)
                return;

            arena.AddCircle(arena.WorldCenter + 9 * doubleOffset, 2, arena.ColorSafe);
        }
    }
}
