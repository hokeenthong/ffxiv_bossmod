﻿using System;
using System.Numerics;

namespace BossMod.Endwalker.P4S1
{
    using static BossModule;

    // component detecting corner assignments for 'setting the scene'; it is used by other components to show various warnings
    class SettingTheScene : Component
    {
        public enum Corner { Unknown, NE, SE, SW, NW }
        public enum Element { Fire, Lightning, Acid, Water }

        private Corner[] _assignments = new Corner[4];
        public Corner Assignment(Element elem) => _assignments[(int)elem];

        public Element FindElement(Corner corner)
        {
            return (Element)Array.IndexOf(_assignments, corner);
        }

        public Vector3 Direction(Corner corner)
        {
            return corner switch
            {
                Corner.NE => new( 1, 0, -1),
                Corner.SE => new( 1, 0,  1),
                Corner.SW => new(-1, 0,  1),
                Corner.NW => new(-1, 0, -1),
                _ => Vector3.Zero
            };
        }

        public Corner FromPos(BossModule module, Vector3 pos)
        {
            return pos.X > module.Arena.WorldCenter.X
                ? (pos.Z > module.Arena.WorldCenter.Z ? Corner.SE : Corner.NE)
                : (pos.Z > module.Arena.WorldCenter.Z ? Corner.SW : Corner.NW);
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            // this is a fallback in case env-control assignment doesn't work for some reason...
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo!.Action.ID)
            {
                case AID.PinaxAcid:
                    AssignFromCast(module, Element.Acid, actor.Position);
                    break;
                case AID.PinaxLava:
                    AssignFromCast(module, Element.Fire, actor.Position);
                    break;
                case AID.PinaxWell:
                    AssignFromCast(module, Element.Water, actor.Position);
                    break;
                case AID.PinaxLevinstrike:
                    AssignFromCast(module, Element.Lightning, actor.Position);
                    break;
            }
        }

        public override void OnEventEnvControl(BossModule module, uint featureID, byte index, uint state)
        {
            // 8003759C, state=00020001
            // what I've seen so far:
            // 1. WF arrangement: indices 1, 2, 3, 4, 5, 10, 15, 20
            //    AL
            // 2. FW arrangement: indices 1, 2, 3, 4, 8, 11, 14, 17
            //    LA
            // 2. WL arrangement: indices 1, 2, 3, 4, 6, 9, 15, 20
            //    AF
            // so indices are the following:
            //  5 => NE fire
            //  6 => SE fire
            //  7 => SW fire?
            //  8 => NW fire
            //  9 => NE lightning
            // 10 => SE lightning
            // 11 => SW lightning
            // 12 => NW lightning?
            // 13 => NE acid?
            // 14 => SE acid
            // 15 => SW acid
            // 16 => NW acid?
            // 17 => NE water
            // 18 => SE water?
            // 19 => SW water?
            // 20 => NW water
            if (featureID == 0x8003759C && state == 0x00020001 && index >= 5 && index <= 20)
            {
                int i = index - 5;
                _assignments[i >> 2] = (Corner)(1 + (i & 3));
            }
        }

        private void AssignFromCast(BossModule module, Element elem, Vector3 pos)
        {
            var corner = FromPos(module, pos);
            var prev = Assignment(elem);
            if (prev != Corner.Unknown && prev != corner)
            {
                module.ReportError(this, $"Assignment mismatch: {prev} from env-control, {corner} from cast");
            }
            _assignments[(int)elem] = corner;
        }
    }
}
