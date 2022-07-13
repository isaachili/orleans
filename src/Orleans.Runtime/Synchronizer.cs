using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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

    public class Synchronizer<TGrain, TKey> : Synchronizer
        where TGrain : Grain, IGrain
        where TKey : IComparable<TKey>, IComparable, IEquatable<TKey>
    {
        // Backing fields
        private TKey key;

        public Synchronizer(TKey key)
        {
            this.Key = key;
        }

        public Type GrainType => typeof(TGrain);

        public TKey Key
        {
            get => this.key;
            private set => this.key = value switch
            {
                string or long or Guid => value,
                _ => throw new InvalidOperationException("The provided type is not supported by 'key'."),
            };
        }

        internal bool VerifyActivationData(IActivationData activationData)
        {
            static TKey ChangeType<TValue>(TValue value)
            {
                return (TKey)Convert.ChangeType(value, typeof(TKey));
            }

            if (activationData is not ActivationData ad || ad.GrainType != typeof(TGrain))
            {
                return false;
            }

            var key = this.Key switch
            {
                string => ChangeType(activationData.GrainId.PrimaryKeyString),
                long => ChangeType(activationData.GrainId.PrimaryKeyLong),
                Guid => ChangeType(activationData.GrainId.PrimaryKey),
                _ => throw new InvalidOperationException("The provided type is not supported by 'key'."),
            };

            return key.Equals(this.Key);
        }
    }
}
