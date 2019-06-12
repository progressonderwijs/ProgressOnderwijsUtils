using System;
using System.Data.SqlClient;

namespace ProgressOnderwijsUtils
{
    public struct BatchTimeout
    {
        readonly ushort backingTimeout;
        public readonly TimeoutKind Kind;

        public enum TimeoutKind : byte
        {
            DeferToConnectionDefaultCommandTimeout,
            AbsoluteTimeout,
            ScaledTimeout,
            NoTimeout,
        }

        BatchTimeout(TimeoutKind kind, int backingTimeout)
        {
            Kind = kind;
            this.backingTimeout = checked((ushort)backingTimeout);
        }

        public static BatchTimeout DeferToConnectionDefault
            => new BatchTimeout(TimeoutKind.DeferToConnectionDefaultCommandTimeout, 0);

        public static BatchTimeout WithoutTimeout
            => new BatchTimeout(TimeoutKind.NoTimeout, 0);

        public static BatchTimeout AbsoluteSeconds(int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0) {
                return new BatchTimeout(TimeoutKind.AbsoluteTimeout, timeoutInSeconds);
            } else if (timeoutInSeconds == 0) { //TODO: ban.
                return new BatchTimeout(TimeoutKind.NoTimeout, 0);
            } else {
                throw new ArgumentOutOfRangeException(nameof(timeoutInSeconds), "timeouts must be positive");
            }
        }

        public static BatchTimeout ScaledSeconds(int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0) {
                return new BatchTimeout(TimeoutKind.ScaledTimeout, timeoutInSeconds);
            } else if (timeoutInSeconds == 0) { //TODO: ban.
                return new BatchTimeout(TimeoutKind.NoTimeout, 0);
            } else {
                throw new ArgumentOutOfRangeException(nameof(timeoutInSeconds), "timeouts must be positive");
            }
        }

        public int TimeoutWithFallback(SqlConnection conn)
        {
            switch (Kind) {
                case TimeoutKind.DeferToConnectionDefaultCommandTimeout:
                    return conn.DefaultCommandTimeout();
                case TimeoutKind.NoTimeout:
                    return 0;
                case TimeoutKind.AbsoluteTimeout:
                    return backingTimeout;
                case TimeoutKind.ScaledTimeout:
                    return conn.Site is IHasDefaultCommandTimeout defaultScale ? Math.Max(1, (int)(0.5 + backingTimeout * defaultScale.TimeoutScale)) : backingTimeout;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
