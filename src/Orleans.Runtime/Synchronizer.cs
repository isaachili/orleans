using System;
using System.Collections.Concurrent;
using System.Diagnostics;

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

        #region Synchronizers

        public static ConcurrentDictionary<IGrainContext, Synchronizer> Synchronizers = new();

        internal static Synchronizer GetSynchronizer(IGrainContext grainContext)
        {
            if (grainContext is null)
            {
                return null;
            }

            return Synchronizers.TryGetValue(grainContext, out var synchronizer)
                ? synchronizer
                : null;
        }

        #endregion

        public States State { get; internal set; }

        public IDisposable GrainTimer { get; set; }

        public States Break { get; set; }
    }
}
