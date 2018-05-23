using System.Linq;
using JetBrains.Annotations;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Microsoft.Extensions.Options;

namespace ProgressOnderwijsUtils.AspNetCore
{
    public sealed class SnapshotCollectorTelemetryProcessorFactory : ITelemetryProcessorFactory
    {
        readonly IOptions<SnapshotCollectorConfiguration> _options;

        public SnapshotCollectorTelemetryProcessorFactory(IOptions<SnapshotCollectorConfiguration> options)
        {
            _options = options;
        }

        [NotNull]
        public ITelemetryProcessor Create(ITelemetryProcessor next)
            => new SnapshotCollectorTelemetryProcessor(next, _options.Value);
    }
}
