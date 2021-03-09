using System;
using Microsoft.Data.SqlClient;
using System.Threading;

namespace ProgressOnderwijsUtils
{
    public readonly struct CommandTimeoutDefaults
    {
        public readonly int AbsoluteDefaultCommandTimeout;
        public readonly double TimeoutScalingFactor;

        public CommandTimeoutDefaults(int absoluteDefaultCommandTimeout, double timeoutScalingFactor)
            => (AbsoluteDefaultCommandTimeout, TimeoutScalingFactor) = (absoluteDefaultCommandTimeout, timeoutScalingFactor);

        public static CommandTimeoutDefaults NoScalingNoTimeout
            => ScaledBy(1.0);

        public static CommandTimeoutDefaults ScaledBy(double timeoutScalingFactor)
            => new CommandTimeoutDefaults(0, timeoutScalingFactor);

        public CommandTimeoutDefaults Resolve(CommandTimeout commandTimeout)
            => new CommandTimeoutDefaults(commandTimeout.ComputeAbsoluteTimeout(this), TimeoutScalingFactor);
    }

    public readonly struct CommandTimeout : IEquatable<CommandTimeout>
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

        /// <summary>
        /// Represents whatever timeout the connection specifies as default.
        /// </summary>
        public static CommandTimeout DeferToConnectionDefault
            => new CommandTimeout(TimeoutKind.DeferToConnectionDefaultCommandTimeout, 0);

        /// <summary>
        /// Represents an infinite (or at least very, very long) timeout.
        /// </summary>
        public static CommandTimeout WithoutTimeout
            => new CommandTimeout(TimeoutKind.NoTimeout, 0);

        /// <summary>
        /// Represents a finite timeout in seconds that will NOT be scaled with the connection-specific timeout scaling factor.  If you want to work with scaling factors, use ScaledSeconds.
        /// </summary>
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

        /// <summary>
        /// Represents a finite timeout in seconds that will be scaled with the connection-specific timeout scaling factor (default 1.0).
        /// </summary>
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

        /// <summary>
        /// Converts this possibly scaled or default timeout into an absolute (SqlCommand.CommandTimeout compatible) timeout value (in seconds).
        /// </summary>
        public int ComputeAbsoluteTimeout(SqlConnection conn)
            => ComputeAbsoluteTimeout(conn.Site is IHasDefaultCommandTimeout hasDefaults ? hasDefaults.TimeoutDefaults : CommandTimeoutDefaults.NoScalingNoTimeout);

        /// <summary>
        /// Converts this possibly scaled or default timeout into an absolute (SqlCommand.CommandTimeout compatible) timeout value (in seconds).
        /// </summary>
        public int ComputeAbsoluteTimeout(CommandTimeoutDefaults defaults)
            => Kind switch {
                TimeoutKind.DeferToConnectionDefaultCommandTimeout => defaults.AbsoluteDefaultCommandTimeout,
                TimeoutKind.NoTimeout => 0,
                TimeoutKind.AbsoluteTimeout => backingTimeout,
                TimeoutKind.ScaledTimeout => Math.Max(1, (int)(0.5 + backingTimeout * defaults.TimeoutScalingFactor)),
                _ => throw new InvalidOperationException()
            };

        /// <summary>
        /// Converts this possibly scaled or default timeout into token cancelled after the timeout (starting now).
        /// </summary>
        public CancellationToken ToCancellationToken(SqlConnection sqlConn)
        {
            var timeoutInSqlFormat = ComputeAbsoluteTimeout(sqlConn);
            return timeoutInSqlFormat == 0 ? CancellationToken.None : new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSqlFormat)).Token;
        }

        public override string ToString()
            => $"{Kind}:{backingTimeout}";

        public bool Equals(CommandTimeout other)
            => this == other;

        public override bool Equals(object? obj)
            => obj is CommandTimeout other && this == other;

        public static bool operator ==(CommandTimeout a, CommandTimeout b)
            => a.backingTimeout == b.backingTimeout && a.Kind == b.Kind;

        public static bool operator !=(CommandTimeout a, CommandTimeout b)
            => !(a == b);

        public static implicit operator CommandTimeout(int scaledSeconds)
            => ScaledSeconds(scaledSeconds);

        public override int GetHashCode()
            => (backingTimeout, Kind).GetHashCode();
    }
}
