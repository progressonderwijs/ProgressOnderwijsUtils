using System;
using System.Data.SqlClient;
using System.Threading;
using ValueUtils;

namespace ProgressOnderwijsUtils
{
    public readonly struct CommandTimeoutDefaults
    {
        public readonly int AbsoluteDefaultBatchTimeout;
        public readonly double TimeoutScalingFactor;

        public CommandTimeoutDefaults(int absoluteDefaultBatchTimeout, double timeoutScalingFactor)
            => (AbsoluteDefaultBatchTimeout, TimeoutScalingFactor) = (absoluteDefaultBatchTimeout, timeoutScalingFactor);

        public static CommandTimeoutDefaults NoScalingNoTimeout
            => ScaledBy(1.0);

        public static CommandTimeoutDefaults ScaledBy(double timeoutScalingFactor)
            => new CommandTimeoutDefaults(0, timeoutScalingFactor);

        public CommandTimeoutDefaults Resolve(CommandTimeout commandTimeout)
            => new CommandTimeoutDefaults(commandTimeout.TimeoutWithFallback(this), TimeoutScalingFactor);
    }

    public struct CommandTimeout : IEquatable<CommandTimeout>
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

        CommandTimeout(TimeoutKind kind, int backingTimeout)
        {
            Kind = kind;
            this.backingTimeout = checked((ushort)backingTimeout);
        }

        public static CommandTimeout DeferToConnectionDefault
            => new CommandTimeout(TimeoutKind.DeferToConnectionDefaultCommandTimeout, 0);

        public static CommandTimeout WithoutTimeout
            => new CommandTimeout(TimeoutKind.NoTimeout, 0);

        public static CommandTimeout AbsoluteSeconds(int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0) {
                return new CommandTimeout(TimeoutKind.AbsoluteTimeout, timeoutInSeconds);
            } else if (timeoutInSeconds == 0) { //TODO: ban.
                return new CommandTimeout(TimeoutKind.NoTimeout, 0);
            } else {
                throw new ArgumentOutOfRangeException(nameof(timeoutInSeconds), "timeouts must be positive");
            }
        }

        public static CommandTimeout ScaledSeconds(int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0) {
                return new CommandTimeout(TimeoutKind.ScaledTimeout, timeoutInSeconds);
            } else if (timeoutInSeconds == 0) { //TODO: ban.
                return new CommandTimeout(TimeoutKind.NoTimeout, 0);
            } else {
                throw new ArgumentOutOfRangeException(nameof(timeoutInSeconds), "timeouts must be positive");
            }
        }

        public int TimeoutWithFallback(SqlConnection conn)
            => TimeoutWithFallback(conn.Site is IHasDefaultCommandTimeout hasDefaults ? hasDefaults.TimeoutDefaults : CommandTimeoutDefaults.NoScalingNoTimeout);

        public int TimeoutWithFallback(CommandTimeoutDefaults defaults)
        {
            switch (Kind) {
                case TimeoutKind.DeferToConnectionDefaultCommandTimeout:
                    return defaults.AbsoluteDefaultBatchTimeout;
                case TimeoutKind.NoTimeout:
                    return 0;
                case TimeoutKind.AbsoluteTimeout:
                    return backingTimeout;
                case TimeoutKind.ScaledTimeout:
                    return Math.Max(1, (int)(0.5 + backingTimeout * defaults.TimeoutScalingFactor));
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

        public bool Equals(CommandTimeout other)
            => this == other;

        public override bool Equals(object obj)
            => obj is CommandTimeout other && this == other;

        public static bool operator ==(CommandTimeout a, CommandTimeout b)
            => FieldwiseEquality.AreEqual(a, b);

        public static bool operator !=(CommandTimeout a, CommandTimeout b)
            => !(a == b);

        public override int GetHashCode()
            => FieldwiseHasher.Hash(this);
    }
}
