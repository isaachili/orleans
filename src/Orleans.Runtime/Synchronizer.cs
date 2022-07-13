using System;

namespace Orleans.Runtime
{
    public class Synchronizer
    {
        [Flags]
        public enum States
        {
            TimerCallback = 1,
            TimerDispose = 2,
            ActivationDispose = 4,
            Reactivation = 8
        }

        public static Synchronizer Instance { get; } = new();

        public States State { get; internal set; }

        public string GrainTypeName { get; set; }

        public States Break { get; set; }
    }
}
