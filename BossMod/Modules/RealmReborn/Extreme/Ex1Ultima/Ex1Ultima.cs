﻿namespace BossMod.RealmReborn.Extreme.Ex1Ultima
{
    public class Ex1Ultima : BossModule
    {
        public Ex1Ultima(BossModuleManager manager, Actor primary)
            : base(manager, primary, true)
        {
            Arena.WorldCenter = new();
            Arena.IsCircle = true;

            // TODO: reconsider...
            var states = new StateMachineBuilder(this);
            states.TrivialPhase(600);
            InitStates(states.Build());
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }
    }
}
