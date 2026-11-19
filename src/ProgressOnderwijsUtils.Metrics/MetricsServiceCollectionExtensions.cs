using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;

namespace ProgressOnderwijsUtils.Metrics;

public static class MetricsServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Enables the OpenTelemetry SqlClient instrumentation, whose <c>db.client.operation.duration</c> the collector
        /// listens to, starts the <paramref name="collector"/> when the host starts and disposes it when the host has
        /// stopped. The collector measures the requests of this host; another web host in the same process fails to
        /// start while the collector runs.
        /// </summary>
        /// <param name="collector">Typically a static instance, so that what it collected stays readable outside of the dependency injection container.</param>
        /// <param name="samplingFailed">See <see cref="MetricsCollector{TEndpoint,TDatabase,TPool}.Start"/>.</param>
        public IServiceCollection AddProgressOnderwijsMetrics<TEndpoint, TDatabase, TPool>(
            MetricsCollector<TEndpoint, TDatabase, TPool> collector,
            Action<Exception> samplingFailed
        )
            where TEndpoint : struct, Enum
            where TDatabase : struct, Enum
            where TPool : struct, Enum
        {
            _ = services.AddOpenTelemetry().WithMetrics(builder => builder.AddSqlClientInstrumentation());
            _ = services.AddHostedService(
                provider => {
                    var hostMeters = provider.GetRequiredService<IMeterFactory>();
                    return new MetricsCollectorLifetime(() => collector.Start(hostMeters, samplingFailed), collector.Dispose);
                }
            );
            return services;
        }
    }

    /// <summary>
    /// Starts in <see cref="IHostedLifecycleService.StartingAsync"/>, which the host calls on all hosted services
    /// before any of them gets its StartAsync, so also before the web server starts accepting requests. Starting any
    /// later misses the active_requests measurements of the first requests, and the running total stays skewed for
    /// good.
    /// Stops in <see cref="IHostedLifecycleService.StoppedAsync"/>, after the web server and all other hosted services
    /// have stopped, so that the requests still running during shutdown are measured as well.
    /// </summary>
    sealed class MetricsCollectorLifetime(Action start, Action stop) : IHostedLifecycleService, IDisposable
    {
        public void Dispose()
            => stop();

        public Task StartingAsync(CancellationToken cancel)
        {
            start();
            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancel)
            => Task.CompletedTask;

        public Task StartedAsync(CancellationToken cancel)
            => Task.CompletedTask;

        public Task StoppingAsync(CancellationToken cancel)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancel)
            => Task.CompletedTask;

        public Task StoppedAsync(CancellationToken cancel)
        {
            stop();
            return Task.CompletedTask;
        }
    }
}
