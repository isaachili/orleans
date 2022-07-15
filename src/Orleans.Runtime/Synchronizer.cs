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

        public static ConcurrentDictionary<(Type GrainType, object Key), Synchronizer> Synchronizers = new();

        internal static Synchronizer GetSynchronizer(IActivationData activationData)
        {
            if (activationData is not ActivationData ad)
            {
                return null;
            }

            Synchronizer synchronizer;

            if (ad.GrainId.IsLongKey)
            {
                var primaryKeyLong = ad.GrainId.Key.HasKeyExt
                    ? ad.GrainId.GetPrimaryKeyLong(out _)
                    : ad.GrainId.PrimaryKeyLong;

                return Synchronizers.TryGetValue((ad.GrainType, primaryKeyLong), out synchronizer)
                    ? synchronizer
                    : null;
            }

            if (Synchronizers.TryGetValue((ad.GrainType, ad.GrainId.PrimaryKeyString), out synchronizer))
            {
                return synchronizer;
            }

            var primaryKeyGuid = ad.GrainId.Key.HasKeyExt
                    ? ad.GrainId.GetPrimaryKey(out _)
                    : ad.GrainId.PrimaryKey;

            return Synchronizers.TryGetValue((ad.GrainType, primaryKeyGuid), out synchronizer)
                ? synchronizer
                : null;
        }

        #endregion

        public States State { get; internal set; }

        public IDisposable GrainTimer { get; set; }

        public States Break { get; set; }
    }
}
