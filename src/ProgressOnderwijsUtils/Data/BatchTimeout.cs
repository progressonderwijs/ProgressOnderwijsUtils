using System;
using System.Data.SqlClient;
using System.Threading;
using ValueUtils;

namespace ProgressOnderwijsUtils
{
    public struct BatchTimeout : IEquatable<BatchTimeout>
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
            var commandTimeoutDefaults = conn.Site as IHasDefaultCommandTimeout;
            return TimeoutWithFallback(commandTimeoutDefaults?.DefaultCommandTimeoutInS ?? 0, commandTimeoutDefaults?.TimeoutScale ?? 1.0);
        }

        public int TimeoutWithFallback(int connectionDefaultTimeout, double connectionDefaultScale)
        {
            switch (Kind) {
                case TimeoutKind.DeferToConnectionDefaultCommandTimeout:
                    return connectionDefaultTimeout;
                case TimeoutKind.NoTimeout:
                    return 0;
                case TimeoutKind.AbsoluteTimeout:
                    return backingTimeout;
                case TimeoutKind.ScaledTimeout:
                    return Math.Max(1, (int)(0.5 + backingTimeout * connectionDefaultScale));
                default:
                    throw new InvalidOperationException();
            }
        }

        public CancellationToken ToCancellationToken(SqlConnection sqlConn)
        {
            var timeoutInSqlFormat = TimeoutWithFallback(sqlConn);
            return timeoutInSqlFormat == 0 ? CancellationToken.None : new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSqlFormat)).Token;
        }

        public override string ToString()
            => $"{Kind}:{backingTimeout}";

        public bool Equals(BatchTimeout other)
            => this == other;

        public override bool Equals(object obj)
            => obj is BatchTimeout other && this == other;

        public static bool operator ==(BatchTimeout a, BatchTimeout b)
            => FieldwiseEquality.AreEqual(a, b);

        public static bool operator !=(BatchTimeout a, BatchTimeout b)
            => !(a == b);

        public override int GetHashCode()
            => FieldwiseHasher.Hash(this);
    }
}
