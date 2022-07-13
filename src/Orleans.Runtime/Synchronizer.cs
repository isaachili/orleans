using System;
using System.Collections.Concurrent;

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

        public static ConcurrentDictionary<(Type GrainType, object Key), Synchronizer> Synchronizers = new();

        internal static Synchronizer GetSynchronizer(IActivationData activationData)
        {
            if (activationData is not ActivationData ad)
            {
                return null;
            }

            if (Synchronizers.TryGetValue((ad.GrainType, ad.GrainId.PrimaryKeyString), out var synchronizer))
            {
                return synchronizer;
            }

            if (Synchronizers.TryGetValue((ad.GrainType, ad.GrainId.PrimaryKeyLong), out synchronizer))
            {
                return synchronizer;
            }

            if (Synchronizers.TryGetValue((ad.GrainType, ad.GrainId.PrimaryKey), out synchronizer))
            {
                return synchronizer;
            }

            return null;
        }

        #endregion

        protected Synchronizer()
        {

        }

        public States State { get; internal set; }

        public IDisposable GrainTimer { get; set; }
    }
}
